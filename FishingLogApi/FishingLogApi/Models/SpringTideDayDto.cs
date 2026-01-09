using FishingLogApi.Utilities;

namespace FishingLogApi.Models
{
public sealed record SpringTideDayDto(
    DateOnly Date,
    string Location,
    LunarPhase Phase,
    bool IsSpringTide,
    string Reason
);

}
