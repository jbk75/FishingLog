namespace FishingLogApi.Utilities
{
public enum LunarPhase
{
    NewMoon,
    FirstQuarter,
    FullMoon,
    LastQuarter,
    Other
}

public static class MoonPhaseCalculator
{
    private const double SynodicMonth = 29.530588853;

    public static LunarPhase GetMoonPhase(DateTime dateUtc)
    {
        var julian = ToJulianDate(dateUtc);
        var days = julian - 2451550.1;
        var phase = (days % SynodicMonth) / SynodicMonth;
        if (phase < 0) phase += 1;

        return phase switch
        {
            < 0.03 or > 0.97 => LunarPhase.NewMoon,
            < 0.28 => LunarPhase.FirstQuarter,
            < 0.53 => LunarPhase.FullMoon,
            < 0.78 => LunarPhase.LastQuarter,
            _ => LunarPhase.Other
        };
    }

    private static double ToJulianDate(DateTime dateUtc)
    {
        int a = (14 - dateUtc.Month) / 12;
        int y = dateUtc.Year + 4800 - a;
        int m = dateUtc.Month + 12 * a - 3;

        var jdn =
            dateUtc.Day +
            (153 * m + 2) / 5 +
            365 * y +
            y / 4 -
            y / 100 +
            y / 400 -
            32045;

        return jdn
            + (dateUtc.Hour - 12) / 24.0
            + dateUtc.Minute / 1440.0
            + dateUtc.Second / 86400.0;
    }
}

}
