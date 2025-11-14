using System.Data.SqlClient;
using Dapper;
using FishingNewsWebScraper.Models;
using FishingNewsWebScraper.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace FishingNewsWebScraper.Infrastructure;

public sealed class SqlFishingNewsRepository : IFishingNewsRepository
{
    private readonly DatabaseOptions _databaseOptions;
    private readonly ILogger<SqlFishingNewsRepository> _logger;

    public SqlFishingNewsRepository(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<SqlFishingNewsRepository> logger)
    {
        _databaseOptions = databaseOptions.Value;
        _logger = logger;
    }

    public async Task<int> EnsureFishingPlaceAsync(FishingPlaceDetails place, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_databaseOptions.ConnectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        await using var connection = new SqlConnection(_databaseOptions.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var existingId = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                "SELECT Id FROM dbo.FishingPlace WHERE Name = @Name",
                new { place.Name },
                cancellationToken: cancellationToken));

        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        var typeId = place.FishingPlaceTypeId ?? InferFishingPlaceType(place.Name);

        var insertId = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                "INSERT INTO dbo.FishingPlace (Name, Longitude, Latitude, NumberOfSpots, Description, FishingPlaceTypeID) VALUES (@Name, @Longitude, @Latitude, @NumberOfSpots, @Description, @FishingPlaceTypeID); SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new
                {
                    place.Name,
                    place.Longitude,
                    place.Latitude,
                    place.NumberOfSpots,
                    place.Description,
                    FishingPlaceTypeID = typeId
                },
                cancellationToken: cancellationToken));

        _logger.LogInformation("Created fishing place {Name} with id {Id}.", place.Name, insertId);
        return insertId;
    }

    public async Task<int> InsertFishingNewsAsync(FishingNewsRecord record, CancellationToken cancellationToken)
    {
        if (!record.FishingPlaceId.HasValue)
        {
            throw new InvalidOperationException("Fishing place id must be populated before inserting the news record.");
        }

        await using var connection = new SqlConnection(_databaseOptions.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var summary = record.WeatherDetails.Count == 0
            ? record.TideState
            : string.Join("; ", record.WeatherDetails.Select(w => $"{w.ObservationTime:HH:mm}: {w.Summary}"));

        var command = new CommandDefinition(
            "INSERT INTO dbo.FishingNews (Date, Time, FishingPlaceId, NumberOfFishesCaught, NumberOfFishesSeen, WeatherOnFishingDay, Description, TideState, PeakActivityTime, Source) VALUES (@Date, @Time, @FishingPlaceId, @NumberOfFishesCaught, @NumberOfFishesSeen, @WeatherSummary, @Description, @TideState, @PeakActivityTime, @Source); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            new
            {
                Date = record.Date.ToDateTime(TimeOnly.MinValue),
                Time = record.Time?.ToTimeSpan(),
                record.FishingPlaceId,
                record.NumberOfFishesCaught,
                record.NumberOfFishesSeen,
                WeatherSummary = summary,
                record.Description,
                record.TideState,
                PeakActivityTime = record.PeakActivityTime?.ToTimeSpan(),
                Source = string.IsNullOrWhiteSpace(record.Source) ? null : record.Source
            },
            cancellationToken: cancellationToken);

        var id = await connection.ExecuteScalarAsync<int>(command);
        _logger.LogInformation("Inserted fishing news record {Id} for place {Place}.", id, record.FishingPlace.Name);
        return id;
    }

    public async Task UpsertWeatherDetailsAsync(int fishingNewsId, IReadOnlyCollection<FishingNewsWeatherDetail> weatherDetails, CancellationToken cancellationToken)
    {
        if (weatherDetails.Count == 0)
        {
            return;
        }

        await using var connection = new SqlConnection(_databaseOptions.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM dbo.FishingNewsWeatherDetail WHERE FishingNewsId = @FishingNewsId",
            new { FishingNewsId = fishingNewsId },
            transaction,
            cancellationToken: cancellationToken));

        foreach (var detail in weatherDetails)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT INTO dbo.FishingNewsWeatherDetail (FishingNewsId, ObservationTime, Summary, TemperatureC, WindSpeedMetersPerSecond, PrecipitationMillimeters, TideState, WindDirection) VALUES (@FishingNewsId, @ObservationTime, @Summary, @TemperatureC, @WindSpeedMetersPerSecond, @PrecipitationMillimeters, @TideState, @WindDirection)",
                new
                {
                    FishingNewsId = fishingNewsId,
                    detail.ObservationTime,
                    detail.Summary,
                    detail.TemperatureC,
                    detail.WindSpeedMetersPerSecond,
                    detail.PrecipitationMillimeters,
                    detail.TideState,
                    detail.WindDirection
                },
                transaction,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task UpsertImagesAsync(int fishingNewsId, IReadOnlyCollection<FishingNewsImage> images, CancellationToken cancellationToken)
    {
        if (images.Count == 0)
        {
            return;
        }

        await using var connection = new SqlConnection(_databaseOptions.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM dbo.FishingNewsImage WHERE FishingNewsId = @FishingNewsId",
            new { FishingNewsId = fishingNewsId },
            transaction,
            cancellationToken: cancellationToken));

        foreach (var image in images)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT INTO dbo.FishingNewsImage (FishingNewsId, SourceUri, LocalPath, Caption) VALUES (@FishingNewsId, @SourceUri, @LocalPath, @Caption)",
                new
                {
                    FishingNewsId = fishingNewsId,
                    SourceUri = image.SourceUri.ToString(),
                    image.LocalPath,
                    image.Caption
                },
                transaction,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static int InferFishingPlaceType(string name)
    {
        if (name.Contains("vatn", StringComparison.OrdinalIgnoreCase) || name.Contains("lake", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 2;
    }
}
