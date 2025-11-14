namespace FishingNewsWebScraper.Options;

public sealed class ScraperOptions
{
    /// <summary>
    /// Number of years back in time that the scraper should search for fishing news.
    /// </summary>
    public int YearsBack { get; set; } = 1;

    /// <summary>
    /// Base directory used to persist downloaded images. Relative paths are resolved from the application base path.
    /// </summary>
    public string ImageDirectory { get; set; } = "images";

    /// <summary>
    /// List of Icelandic fishing news sources that should be parsed.
    /// Each entry represents a landing page or RSS feed.
    /// </summary>
    public IList<ScraperSourceOptions> Sources { get; set; } = new List<ScraperSourceOptions>();

    /// <summary>
    /// Social media endpoints that should be queried for fishing news (e.g. Instagram, Facebook).
    /// </summary>
    public IList<SocialMediaSourceOptions> SocialMediaSources { get; set; } = [];

    /// <summary>
    /// Keywords that should be matched when searching for relevant fishing news.
    /// </summary>
    public IList<string> FishKeywords { get; set; } = new List<string> { "lax", "bleikja", "urriði", "sjóbleikja", "salmon", "trout", "arctic char" };
}

public sealed class ScraperSourceOptions
{
    /// <summary>
    /// Friendly name for the source.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Absolute URI to an RSS feed or a HTML landing page containing fishing reports.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Optional CSS selector or XPath query that can be used to narrow down the
    /// HTML nodes that contain relevant articles. This is ignored for RSS feeds.
    /// </summary>
    public string? Query { get; set; }
}

public sealed class SocialMediaSourceOptions
{
    /// <summary>
    /// Name of the social media platform.
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// Format string used to construct a search URL. The token {query} is replaced with the encoded search phrase.
    /// </summary>
    public string SearchUrlTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Set of search phrases that should be queried on the platform.
    /// </summary>
    public IList<string> Queries { get; set; } = new List<string>();

    /// <summary>
    /// XPath expression that matches individual posts within the returned HTML.
    /// </summary>
    public string? PostSelector { get; set; }

    /// <summary>
    /// XPath expression resolved relative to a post node that retrieves the textual content.
    /// </summary>
    public string? ContentSelector { get; set; }

    /// <summary>
    /// XPath expression resolved relative to a post node that retrieves a link to the post.
    /// </summary>
    public string? LinkSelector { get; set; }

    /// <summary>
    /// XPath expression resolved relative to a post node that retrieves a timestamp element.
    /// </summary>
    public string? TimestampSelector { get; set; }

    /// <summary>
    /// Name of the attribute on the timestamp node that contains the actual value. Defaults to "datetime".
    /// </summary>
    public string TimestampAttribute { get; set; } = "datetime";
}
