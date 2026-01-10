using FishingLogApi.DAL;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;
using FishingLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FishingLogApi.Controllers;

[Produces("application/json")]
[Route("api/suggestedtrips")]
public class SuggestedFishingTripsController : ControllerBase
{
    private readonly TripRepository _tripRepository;
    private readonly FishingNewsRepository _newsRepository;
    private readonly VeidistadurRepository _placeRepository;

    public SuggestedFishingTripsController(
        TripRepository tripRepository,
        FishingNewsRepository newsRepository,
        VeidistadurRepository placeRepository)
    {
        _tripRepository = tripRepository;
        _newsRepository = newsRepository;
        _placeRepository = placeRepository;
    }

    [HttpGet]
    public ActionResult<SuggestedFishingTripsResponse> GetSuggestedTrips()
    {
        var seasonRange = GetUpcomingSeasonRange(DateTime.Today);
        var trips = _tripRepository.GetTrips() ?? [];
        var newsItems = _newsRepository.GetFishingNews() ?? [];
        var places = _placeRepository.GetVeidistadir() ?? [];

        var placeMap = BuildPlaceMap(places);
        var grouped = BuildDateGroups(trips, newsItems);
        var suggestions = BuildSuggestions(grouped, placeMap, seasonRange);

        return Ok(new SuggestedFishingTripsResponse
        {
            SeasonYear = seasonRange.Year,
            Suggestions = suggestions
        });
    }

    private static Dictionary<int, string> BuildPlaceMap(IEnumerable<FishingPlace> places)
    {
        var map = new Dictionary<int, string>();
        foreach (var place in places)
        {
            map[place.Id] = string.IsNullOrWhiteSpace(place.Name) ? "Unknown place" : place.Name;
        }
        return map;
    }

    private static Dictionary<int, Dictionary<string, SuggestionEntry>> BuildDateGroups(
        IEnumerable<TripDto> trips,
        IEnumerable<FishingNewsDto> newsItems)
    {
        var grouped = new Dictionary<int, Dictionary<string, SuggestionEntry>>();

        foreach (var trip in trips)
        {
            AddEntry(grouped, trip.VsId, trip.DagsFra, trip.Description ?? string.Empty, SuggestionSource.Trip);
        }

        foreach (var news in newsItems)
        {
            AddEntry(grouped, news.FishingPlaceId, news.Date, news.Description ?? string.Empty, SuggestionSource.News);
        }

        return grouped;
    }

    private static void AddEntry(
        Dictionary<int, Dictionary<string, SuggestionEntry>> grouped,
        int placeId,
        DateTime dateValue,
        string description,
        SuggestionSource source)
    {
        if (placeId <= 0)
        {
            return;
        }

        var month = dateValue.Month;
        var day = dateValue.Day;
        var key = $"{month}-{day}";

        if (!grouped.TryGetValue(placeId, out var placeEntries))
        {
            placeEntries = new Dictionary<string, SuggestionEntry>();
            grouped[placeId] = placeEntries;
        }

        if (!placeEntries.TryGetValue(key, out var entry))
        {
            entry = new SuggestionEntry(month, day);
            placeEntries[key] = entry;
        }

        entry.Score += 1 + GetDescriptionBoost(description);
        if (source == SuggestionSource.Trip)
        {
            entry.TripCount += 1;
        }
        else
        {
            entry.NewsCount += 1;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            entry.Descriptions.Add(description);
        }
    }

    private static int GetDescriptionBoost(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return 0;
        }

        var keywords = new[]
        {
            "good", "great", "best", "many", "lots", "plenty", "caught", "catch", "fish", "fishing",
            "lax", "silung", "bleik", "tide", "high"
        };

        var text = description.ToLowerInvariant();
        var boost = 0;

        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.Ordinal))
            {
                boost += 1;
            }
        }

        if (text.Any(char.IsDigit))
        {
            boost += 1;
        }

        return boost;
    }

    private static List<SuggestedFishingTripDto> BuildSuggestions(
        Dictionary<int, Dictionary<string, SuggestionEntry>> grouped,
        Dictionary<int, string> placeMap,
        SeasonRange seasonRange)
    {
        var suggestions = new List<SuggestedFishingTripDto>();

        foreach (var (placeId, entries) in grouped)
        {
            SuggestionEntry? bestEntry = null;
            foreach (var entry in entries.Values)
            {
                if (!IsEntryInSeason(entry, seasonRange))
                {
                    continue;
                }

                if (bestEntry == null ||
                    entry.Score > bestEntry.Score ||
                    (entry.Score == bestEntry.Score &&
                     entry.TripCount + entry.NewsCount > bestEntry.TripCount + bestEntry.NewsCount))
                {
                    bestEntry = entry;
                }
            }

            if (bestEntry == null)
            {
                continue;
            }

            var suggestedDate = CreateDateInYear(bestEntry.Month, bestEntry.Day, seasonRange.Year);
            if (suggestedDate < seasonRange.Start || suggestedDate > seasonRange.End)
            {
                continue;
            }

            suggestions.Add(new SuggestedFishingTripDto
            {
                PlaceId = placeId,
                PlaceName = placeMap.TryGetValue(placeId, out var name) ? name : "Unknown place",
                Date = suggestedDate,
                Reason = BuildReason(bestEntry),
                TripCount = bestEntry.TripCount,
                NewsCount = bestEntry.NewsCount
            });
        }

        suggestions.Sort((a, b) => a.Date.CompareTo(b.Date));

        if (suggestions.Count == 0)
        {
            suggestions = BuildSeasonFallbackSuggestions(placeMap, seasonRange);
        }

        return suggestions;
    }

    private static SeasonRange GetUpcomingSeasonRange(DateTime today)
    {
        var seasonYear = today.Month > 11 ? today.Year + 1 : today.Year;
        var seasonStart = new DateTime(seasonYear, 4, 1);
        var seasonEnd = new DateTime(seasonYear, 11, 30);

        return new SeasonRange(seasonYear, seasonStart, seasonEnd);
    }

    private static bool IsEntryInSeason(SuggestionEntry entry, SeasonRange seasonRange)
    {
        if (entry.Month < 4 || entry.Month > 11)
        {
            return false;
        }

        var date = CreateDateInYear(entry.Month, entry.Day, seasonRange.Year);
        return date >= seasonRange.Start && date <= seasonRange.End;
    }

    private static List<SuggestedFishingTripDto> BuildSeasonFallbackSuggestions(
        Dictionary<int, string> placeMap,
        SeasonRange seasonRange)
    {
        if (placeMap.Count == 0)
        {
            return [];
        }

        var placeIds = placeMap
            .OrderBy(kvp => kvp.Value, StringComparer.OrdinalIgnoreCase)
            .Select(kvp => kvp.Key)
            .ToList();

        var maxSuggestions = Math.Min(5, placeIds.Count);
        var totalDays = Math.Max(1, (seasonRange.End - seasonRange.Start).Days);
        var step = Math.Max(1, totalDays / (maxSuggestions + 1));

        var suggestions = new List<SuggestedFishingTripDto>();
        for (var i = 0; i < maxSuggestions; i += 1)
        {
            var offsetDays = step * (i + 1);
            var date = seasonRange.Start.AddDays(offsetDays);
            if (date > seasonRange.End)
            {
                date = seasonRange.End;
            }

            var placeId = placeIds[i];
            suggestions.Add(new SuggestedFishingTripDto
            {
                PlaceId = placeId,
                PlaceName = placeMap[placeId],
                Date = date,
                Reason = "Suggested for the upcoming April-November season based on available fishing places.",
                TripCount = 0,
                NewsCount = 0
            });
        }

        return suggestions;
    }

    private static DateTime CreateDateInYear(int month, int day, int year)
    {
        var maxDay = DateTime.DaysInMonth(year, month);
        var normalizedDay = Math.Min(day, maxDay);
        return new DateTime(year, month, normalizedDay);
    }

    private static string BuildReason(SuggestionEntry entry)
    {
        var tripLabel = entry.TripCount == 1 ? "trip log entry" : "trip log entries";
        var newsLabel = entry.NewsCount == 1 ? "fishing news report" : "fishing news reports";
        var reason = $"Based on {entry.TripCount} {tripLabel} and {entry.NewsCount} {newsLabel} around this time.";

        var snippet = GetDescriptionSnippet(entry.Descriptions);
        if (!string.IsNullOrWhiteSpace(snippet))
        {
            reason += $" Notes mention: \"{snippet}\".";
        }

        return reason;
    }

    private static string GetDescriptionSnippet(List<string> descriptions)
    {
        var snippet = descriptions.FirstOrDefault(description => !string.IsNullOrWhiteSpace(description));
        if (string.IsNullOrWhiteSpace(snippet))
        {
            return string.Empty;
        }

        snippet = string.Join(" ", snippet.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries));
        return snippet.Length > 140 ? string.Concat(snippet.AsSpan(0, 137), "...") : snippet;
    }

    private sealed record SeasonRange(int Year, DateTime Start, DateTime End);

    private sealed class SuggestionEntry
    {
        public SuggestionEntry(int month, int day)
        {
            Month = month;
            Day = day;
        }

        public int Month { get; }
        public int Day { get; }
        public int Score { get; set; }
        public int TripCount { get; set; }
        public int NewsCount { get; set; }
        public List<string> Descriptions { get; } = [];
    }

    private enum SuggestionSource
    {
        Trip,
        News
    }
}
