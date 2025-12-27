using FishingLogApi.Models;

namespace FishingLogApi.Services;

public sealed class TideCalculator
{
    private static readonly TimeSpan HalfPeriod = TimeSpan.FromMinutes(372.5);
    private const string ModelDescription = "Semi-diurnal 12h25m cycle anchored at Jan 1 00:00 UTC.";

    public YearlyTideSummary CalculateYear(int year)
    {
        var start = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddYears(1);
        var events = new List<TideEvent>();
        var next = start;
        var isHigh = true;

        while (next < end)
        {
            events.Add(new TideEvent(next, isHigh ? "High" : "Low"));
            next = next.Add(HalfPeriod);
            isHigh = !isHigh;
        }

        return new YearlyTideSummary(year, DateTimeOffset.UtcNow, ModelDescription, events);
    }
}
