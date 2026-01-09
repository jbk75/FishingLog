using FishingLogApi.Models;
using FishingLogApi.Utilities;

namespace FishingLogApi.Services
{
public static class SpringTideService
{
    public const int DefaultWindowDays = 1;

    public static IReadOnlyList<SpringTideDayDto> GetSpringTideDaysForMonth(
        GeoLocation location,
        int year,
        int month,
        int windowDays = DefaultWindowDays)
    {
        if (location is null) throw new ArgumentNullException(nameof(location));
        if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));
        if (windowDays < 0 || windowDays > 5) throw new ArgumentOutOfRangeException(nameof(windowDays));

        var tz = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var daily = new (DateOnly date, LunarPhase phase)[daysInMonth];

        // 1) Calculate lunar phase per local calendar day (converted to UTC)
        for (int day = 1; day <= daysInMonth; day++)
        {
            var localDate = new DateOnly(year, month, day);
            var localNoon = localDate.ToDateTime(new TimeOnly(12, 0));
            var utcNoon = TimeZoneInfo.ConvertTimeToUtc(localNoon, tz);

            daily[day - 1] = (localDate, MoonPhaseCalculator.GetMoonPhase(utcNoon));
        }

        // 2) Determine spring-tide anchor days + window
        var springTideDates = new HashSet<DateOnly>();

        foreach (var (date, phase) in daily)
        {
            if (phase is LunarPhase.NewMoon or LunarPhase.FullMoon)
            {
                for (int offset = -windowDays; offset <= windowDays; offset++)
                {
                    var d = date.AddDays(offset);
                    if (d.Year == year && d.Month == month)
                        springTideDates.Add(d);
                }
            }
        }

        // 3) Build result
        var result = new List<SpringTideDayDto>(daysInMonth);

        foreach (var (date, phase) in daily)
        {
            var isSpring = springTideDates.Contains(date);

            var reason = phase switch
            {
                LunarPhase.NewMoon => "Nýtt tungl (stórstreymi)",
                LunarPhase.FullMoon => "Fullt tungl (stórstreymi)",
                _ when isSpring => $"Innan ±{windowDays} daga frá nýju/fullt tungli",
                _ => "Ekki stórstreymi"
            };

            result.Add(new SpringTideDayDto(
                Date: date,
                Location: location.Name,
                Phase: phase,
                IsSpringTide: isSpring,
                Reason: reason
            ));
        }

        return result;
    }
}

}
