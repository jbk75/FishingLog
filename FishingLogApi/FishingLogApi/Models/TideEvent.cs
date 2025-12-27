namespace FishingLogApi.Models;

public sealed record TideEvent(DateTimeOffset Timestamp, string Level);
