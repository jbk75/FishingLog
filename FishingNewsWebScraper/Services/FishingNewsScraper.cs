using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FishingNewsWebScraper.Models;
using FishingNewsWebScraper.Options;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FishingNewsWebScraper.Services;

public sealed class FishingNewsScraper : IFishingNewsScraper
{   
    private const int MaxHtmlDepth = 2;
    private static readonly Regex DateRegex = new("(\\d{4})[-/.](\\d{1,2})[-/.](\\d{1,2})", RegexOptions.Compiled);
    private static readonly Regex TimeRegex = new("(\\d{1,2}[:.][0-5]\\d)", RegexOptions.Compiled);
    private static readonly Regex IntegerRegex = new("\\b(\\d{1,3})\\b", RegexOptions.Compiled);
    private static readonly Regex LengthRegex = new("(?<!\\d)(\\d{2,3}(?:[.,]\\d+)?)\\s*(?:cm|sm)\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ScraperOptions _options;
    private readonly ILogger<FishingNewsScraper> _logger;

    public FishingNewsScraper(
        IHttpClientFactory httpClientFactory,
        IOptions<ScraperOptions> options,
        ILogger<FishingNewsScraper> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FishingNewsRecord>> ScrapeAsync(DateTime fromDate, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var records = new List<FishingNewsRecord>();

        _logger.LogInformation(
            "Scraping {SourceCount} configured sources and {SocialSourceCount} social feeds newer than {FromDate}.",
            _options.Sources.Count,
            _options.SocialMediaSources.Count,
            fromDate.ToString("yyyy-MM-dd"));

        foreach (var source in _options.Sources)
        {
            try
            {
                _logger.LogInformation("Scraping source {Name} ({Url}).", source.Name, source.Url);
                var sourceRecords = await ScrapeSourceAsync(client, source, fromDate, cancellationToken);
                records.AddRange(sourceRecords);
                _logger.LogInformation("Finished source {Name}, found {RecordCount} records.", source.Name, sourceRecords.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download or parse source {Name} ({Url}).", source.Name, source.Url);
            }
        }

        foreach (var socialSource in _options.SocialMediaSources)
        {
            if (string.IsNullOrWhiteSpace(socialSource.SearchUrlTemplate) || socialSource.Queries.Count == 0)
            {
                continue;
            }

            foreach (var query in socialSource.Queries.Where(q => !string.IsNullOrWhiteSpace(q)))
            {
                var searchUrl = socialSource.SearchUrlTemplate.Replace("{query}", Uri.EscapeDataString(query), StringComparison.OrdinalIgnoreCase);

                try
                {
                    _logger.LogInformation(
                        "Scraping social source {Platform} for query {Query} using {Url}.",
                        socialSource.Platform,
                        query,
                        searchUrl);
                    var response = await client.GetAsync(searchUrl, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var candidates = ParseSocialMediaContent(socialSource, query, searchUrl, content, fromDate);

                    records.AddRange(candidates);
                    _logger.LogInformation(
                        "Finished social scrape for {Platform} query {Query}, found {RecordCount} records.",
                        socialSource.Platform,
                        query,
                        candidates.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to download or parse social media source {Platform} ({Query}).", socialSource.Platform, query);
                }
            }
        }

        _logger.LogInformation("Completed scraping run with {TotalRecords} total records.", records.Count);
        return records;
    }

    private IReadOnlyList<FishingNewsRecord> ParseRssContent(ScraperSourceOptions source, string content, DateTime fromDate)
    {
        var results = new List<FishingNewsRecord>();
        var document = new HtmlDocument();
        document.LoadHtml(content);
        var items = document.DocumentNode.SelectNodes("//item") ?? document.DocumentNode.SelectNodes("//entry");
        if (items is null)
        {
            return results;
        }

        foreach (var item in items)
        {
            var title = item.SelectSingleNode("title")?.InnerText?.Trim() ?? source.Name;
            var description = HtmlEntity.DeEntitize(item.SelectSingleNode("description")?.InnerText ?? string.Empty).Trim();
            var pubDateString = item.SelectSingleNode("pubDate")?.InnerText ?? item.SelectSingleNode("updated")?.InnerText;
            if (!TryParseDate(pubDateString, out var published))
            {
                continue;
            }

            if (published.Date < fromDate.Date)
            {
                continue;
            }

            if (!ContainsFishKeyword(title) && !ContainsFishKeyword(description))
            {
                continue;
            }

            var link = item.SelectSingleNode("link")?.InnerText?.Trim();
            var record = BuildRecordFromText(source.Name, title + " " + description, published, ResolveLink(source.Url, link));
            results.Add(record);
        }

        return results;
    }

    private async Task<IReadOnlyList<FishingNewsRecord>> ScrapeSourceAsync(HttpClient client, ScraperSourceOptions source, DateTime fromDate, CancellationToken cancellationToken)
    {
        var results = new List<FishingNewsRecord>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<(string Url, int Depth)>();
        queue.Enqueue((source.Url, 0));
        var rootHost = Uri.TryCreate(source.Url, UriKind.Absolute, out var rootUri)
            ? rootUri.Host
            : null;

        while (queue.Count > 0)
        {
            var (currentUrl, depth) = queue.Dequeue();
            if (!visited.Add(currentUrl))
            {
                continue;
            }

            try
            {
                var response = await client.GetAsync(currentUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Skipping source page {Url} for {Name} because it returned status code {StatusCode}.",
                        currentUrl,
                        source.Name,
                        (int)response.StatusCode);
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (IsRssContent(content))
                {
                    results.AddRange(ParseRssContent(source, content, fromDate));
                    continue;
                }

                var document = new HtmlDocument();
                document.LoadHtml(content);
                results.AddRange(ParseHtmlContent(source, document, fromDate, currentUrl));

                if (depth >= MaxHtmlDepth || rootHost is null)
                {
                    continue;
                }

                foreach (var link in ExtractChildLinks(document, currentUrl, rootHost))
                {
                    if (!visited.Contains(link))
                    {
                        queue.Enqueue((link, depth + 1));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download or parse source page {Url} for {Name}.", currentUrl, source.Name);
            }
        }

        return results;
    }

    private static bool IsRssContent(string content) =>
        content.Contains("<rss", StringComparison.OrdinalIgnoreCase) ||
        content.Contains("<feed", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<string> ExtractChildLinks(HtmlDocument document, string currentUrl, string rootHost)
    {
        var anchors = document.DocumentNode.SelectNodes("//a[@href]");
        if (anchors is null)
        {
            return Array.Empty<string>();
        }

        var links = new List<string>();
        foreach (var anchor in anchors)
        {
            var href = anchor.GetAttributeValue("href", null);
            if (string.IsNullOrWhiteSpace(href))
            {
                continue;
            }

            var resolved = ResolveLink(currentUrl, href);
            if (!Uri.TryCreate(resolved, UriKind.Absolute, out var uri))
            {
                continue;
            }

            if (!IsHttpOrHttps(uri))
            {
                continue;
            }

            if (!string.Equals(uri.Host, rootHost, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Normalize by trimming query string parameters and fragments so that we only
            // crawl the canonical path for an article. Some sources append tracking
            // parameters (e.g. "?share=reddit") that would otherwise cause duplicate
            // entries and unnecessary crawling.
            var normalized = uri.GetLeftPart(UriPartial.Path);
            links.Add(normalized);
        }

        return links;
    }

    private static bool IsHttpOrHttps(Uri uri) =>
        string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    private IReadOnlyList<FishingNewsRecord> ParseHtmlContent(ScraperSourceOptions source, HtmlDocument document, DateTime fromDate, string pageUrl)
    {
        var results = new List<FishingNewsRecord>();
        var nodes = !string.IsNullOrWhiteSpace(source.Query)
            ? document.DocumentNode.SelectNodes(source.Query)
            : document.DocumentNode.SelectNodes("//article") ?? document.DocumentNode.SelectNodes("//div[contains(@class,'news')]");

        if (nodes is null)
        {
            return results;
        }

        foreach (var node in nodes)
        {
            var text = HtmlEntity.DeEntitize(node.InnerText).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (!ContainsFishKeyword(text))
            {
                continue;
            }

            var published = ExtractDateTime(node, text);
            if (published.Date < fromDate.Date)
            {
                continue;
            }

            var anchor = node.SelectSingleNode(".//a[@href]");
            var link = anchor is null ? null : anchor.GetAttributeValue("href", string.Empty);
            var record = BuildRecordFromText(source.Name, text, published, ResolveLink(pageUrl, link));

            var imageNodes = node.SelectNodes(".//img[@src]");
            if (imageNodes is not null)
            {
                foreach (var imageNode in imageNodes)
                {
                    if (Uri.TryCreate(imageNode.GetAttributeValue("src", string.Empty), UriKind.Absolute, out var uri))
                    {
                        record.Images.Add(new FishingNewsImage
                        {
                            SourceUri = uri,
                            Caption = HtmlEntity.DeEntitize(imageNode.GetAttributeValue("alt", string.Empty))
                        });
                    }
                }
            }

            results.Add(record);
        }

        return results;
    }

    

    private IReadOnlyList<FishingNewsRecord> ParseSocialMediaContent(SocialMediaSourceOptions source, string query, string requestUrl, string content, DateTime fromDate)
    {
        var results = new List<FishingNewsRecord>();
        var document = new HtmlDocument();
        document.LoadHtml(content);
        var nodes = string.IsNullOrWhiteSpace(source.PostSelector)
            ? document.DocumentNode.SelectNodes("//article")
            : document.DocumentNode.SelectNodes(source.PostSelector);

        if (nodes is null)
        {
            return results;
        }

        foreach (var node in nodes)
        {
            var contentNode = string.IsNullOrWhiteSpace(source.ContentSelector)
                ? node
                : node.SelectSingleNode(source.ContentSelector);

            var textContent = HtmlEntity.DeEntitize(contentNode?.InnerText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(textContent))
            {
                continue;
            }

            if (!ContainsFishKeyword(textContent))
            {
                continue;
            }

            var published = ExtractSocialTimestamp(node, source) ?? DateTime.UtcNow;
            if (published.Date < fromDate.Date)
            {
                continue;
            }

            var link = ResolveSocialLink(node, source, requestUrl);
            var place = new FishingPlaceDetails
            {
                Name = $"{source.Platform} ({query})",
                Description = $"Auto-detected from {source.Platform} search for '{query}'."
            };

            var record = BuildRecordFromText(place.Name, textContent, published, link, place);
            results.Add(record);
        }

        return results;
    }

    private FishingNewsRecord BuildRecordFromText(string sourceName, string text, DateTime published, string? sourceReference = null, FishingPlaceDetails? placeOverride = null)
    {
        var record = new FishingNewsRecord
        {
            Date = DetermineCatchDate(text, published),
            FishingPlace = placeOverride ?? new FishingPlaceDetails
            {
                Name = sourceName
            },
            Description = text,
            SourceOfNews = sourceReference ?? sourceName
        };

        if (TimeRegex.Match(text) is { Success: true } timeMatch && TimeOnly.TryParse(timeMatch.Value.Replace('.', ':'), out var time))
        {
            record.Time = time;
            record.PeakActivityTime = time;
        }

        foreach (var keyword in _options.FishKeywords)
        {
            if (!text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!record.FishSpecies.Any(species => string.Equals(species, keyword, StringComparison.OrdinalIgnoreCase)))
            {
                record.FishSpecies.Add(keyword);
            }
        }

        record.NumberOfFishesCaught = ExtractNumberNearKeywords(text, ["caught", "veiddist", "landaði", "fiskar"]);
        record.NumberOfFishesSeen = ExtractNumberNearKeywords(text, ["saw", "sást", "spotted", "fish seen"]);

        PopulateCatchDetails(record, text);

        return record;
    }

    private string ResolveSocialLink(HtmlNode node, SocialMediaSourceOptions source, string requestUrl)
    {
        if (string.IsNullOrWhiteSpace(source.LinkSelector))
        {
            return requestUrl;
        }

        var linkNode = node.SelectSingleNode(source.LinkSelector);
        var href = linkNode?.GetAttributeValue("href", null);
        return ResolveLink(requestUrl, href);
    }

    private DateTime? ExtractSocialTimestamp(HtmlNode node, SocialMediaSourceOptions source)
    {
        if (string.IsNullOrWhiteSpace(source.TimestampSelector))
        {
            return null;
        }

        var timestampNode = node.SelectSingleNode(source.TimestampSelector);
        if (timestampNode is null)
        {
            return null;
        }

        var attributeName = string.IsNullOrWhiteSpace(source.TimestampAttribute) ? "datetime" : source.TimestampAttribute;
        var attributeValue = timestampNode.GetAttributeValue(attributeName, null);
        if (!string.IsNullOrWhiteSpace(attributeValue) && TryParseDate(attributeValue, out var parsedFromAttribute))
        {
            return parsedFromAttribute;
        }

        var innerText = HtmlEntity.DeEntitize(timestampNode.InnerText).Trim();
        if (!string.IsNullOrWhiteSpace(innerText) && TryParseDate(innerText, out var parsedFromInner))
        {
            return parsedFromInner;
        }

        return null;
    }

    private bool ContainsFishKeyword(string text)
    {
        foreach (var keyword in _options.FishKeywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveLink(string baseUrl, string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return baseUrl;
        }

        if (Uri.TryCreate(link, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) && Uri.TryCreate(baseUri, link, out var combined))
        {
            return combined.ToString();
        }

        return link;
    }

    private static bool TryParseDate(string? value, out DateTime result)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            var match = DateRegex.Match(value);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var year) && int.TryParse(match.Groups[2].Value, out var month) && int.TryParse(match.Groups[3].Value, out var day))
            {
                result = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                return true;
            }
        }

        result = default;
        return false;
    }

    private DateTime ExtractDateTime(HtmlNode node, string text)
    {
        var timeNode = node.SelectSingleNode(".//time[@datetime]");
        if (timeNode is not null && DateTime.TryParse(timeNode.GetAttributeValue("datetime", string.Empty), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedTime))
        {
            return parsedTime;
        }

        if (TryParseDate(timeNode?.InnerText, out var parsed))
        {
            return parsed;
        }

        if (TryParseDate(text, out parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }

    private int? ExtractNumberNearKeywords(string text, string[] keywords)
    {
        foreach (Match match in IntegerRegex.Matches(text))
        {
            var index = match.Index;
            var window = text[Math.Max(0, index - 20)..Math.Min(text.Length, index + match.Length + 20)];
            if (keywords.Any(keyword => window.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                if (int.TryParse(match.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                {
                    return number;
                }
            }
        }

        return null;
    }

    private DateOnly DetermineCatchDate(string text, DateTime published)
    {
        var publishedDate = DateOnly.FromDateTime(published);
        foreach (Match match in DateRegex.Matches(text))
        {
            if (DateTime.TryParse(match.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                var candidate = DateOnly.FromDateTime(parsed);
                if (candidate <= publishedDate)
                {
                    return candidate;
                }
            }
        }

        return publishedDate;
    }

    private void PopulateCatchDetails(FishingNewsRecord record, string text)
    {
        foreach (Match match in LengthRegex.Matches(text))
        {
            if (!decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var lengthCm))
            {
                continue;
            }

            if (record.CatchDetails.Any(detail => detail.LengthCm == lengthCm))
            {
                continue;
            }

            var species = InferSpeciesNearIndex(text, match.Index) ?? record.FishSpecies.FirstOrDefault() ?? "Unknown";

            record.CatchDetails.Add(new FishCatchDetail
            {
                Species = species,
                LengthCm = lengthCm
            });
        }
    }

    private string? InferSpeciesNearIndex(string text, int index)
    {
        var start = Math.Max(0, index - 40);
        var length = Math.Min(text.Length - start, 80);
        var window = text.Substring(start, length);

        foreach (var keyword in _options.FishKeywords)
        {
            if (window.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return keyword;
            }
        }

        return null;
    }
}
