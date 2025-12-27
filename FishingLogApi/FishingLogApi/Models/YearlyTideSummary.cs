namespace FishingLogApi.Models;

public sealed record YearlyTideSummary(
    int Year,
    DateTimeOffset GeneratedAt,
    string Model,
    IReadOnlyList<TideEvent> Events);
