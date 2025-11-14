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
    private static readonly Regex DateRegex = new("(\\d{4})[-/.](\\d{1,2})[-/.](\\d{1,2})", RegexOptions.Compiled);
    private static readonly Regex TimeRegex = new("(\\d{1,2}[:.][0-5]\\d)", RegexOptions.Compiled);
    private static readonly Regex IntegerRegex = new("\\b(\\d{1,3})\\b", RegexOptions.Compiled);

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

        foreach (var source in _options.Sources)
        {
            try
            {
                var response = await client.GetAsync(source.Url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                var candidates = content.Contains("<rss", StringComparison.OrdinalIgnoreCase)
                    ? ParseRssContent(source, content, fromDate)
                    : ParseHtmlContent(source, content, fromDate);

                records.AddRange(candidates);
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
                    var response = await client.GetAsync(searchUrl, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var candidates = ParseSocialMediaContent(socialSource, query, searchUrl, content, fromDate);

                    records.AddRange(candidates);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to download or parse social media source {Platform} ({Query}).", socialSource.Platform, query);
                }
            }
        }

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

    private IReadOnlyList<FishingNewsRecord> ParseHtmlContent(ScraperSourceOptions source, string content, DateTime fromDate)
    {
        var results = new List<FishingNewsRecord>();
        var document = new HtmlDocument();
        document.LoadHtml(content);
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
            var record = BuildRecordFromText(source.Name, text, published, ResolveLink(source.Url, link));

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
            Date = DateOnly.FromDateTime(published),
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
}
