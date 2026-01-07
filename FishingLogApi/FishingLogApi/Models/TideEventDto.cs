namespace FishingLogApi.Models;

public sealed record TideEventDto(DateTimeOffset Timestamp, string Level);
