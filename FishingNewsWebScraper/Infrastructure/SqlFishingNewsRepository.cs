using System;
using System.Linq;
using System.Threading;
using Dapper;
using FishingNewsWebScraper.Models;
using FishingNewsWebScraper.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FishingNewsWebScraper.Infrastructure;

public sealed class SqlFishingNewsRepository : IFishingNewsRepository
{
    private readonly string _primaryConnectionString;
    private readonly string? _fallbackConnectionString;
    private string _connectionString;
    private readonly ILogger<SqlFishingNewsRepository> _logger;

    public SqlFishingNewsRepository(
        IOptions<DatabaseOptions> databaseOptions,
        ILogger<SqlFishingNewsRepository> logger)
    {
        var configuration = databaseOptions.Value.BuildConnectionConfiguration();

        _primaryConnectionString = configuration.ConnectionString;
        _fallbackConnectionString = configuration.FallbackConnectionString;
        _connectionString = configuration.ConnectionString;
        _logger = logger;
    }

    public async Task<int> EnsureFishingPlaceAsync(FishingPlaceDetails place, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var existingId = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                "SELECT Id FROM dbo.FishingPlace WHERE Name = @Name",
                new { place.Name },
                cancellationToken: cancellationToken));

        if (existingId.HasValue)
        {
            _logger.LogInformation("Found existing fishing place {Name} with id {Id}.", place.Name, existingId.Value);
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

        await using var connection = await OpenConnectionAsync(cancellationToken);

        var summary = record.WeatherDetails.Count == 0
            ? record.TideState
            : string.Join("; ", record.WeatherDetails.Select(w => $"{w.ObservationTime:HH:mm}: {w.Summary}"));

        var command = new CommandDefinition(
            "INSERT INTO dbo.FishingNews (Date, Time, FishingPlaceId, NumberOfFishesCaught, NumberOfFishesSeen, WeatherOnFishingDay, Description, TideState, PeakActivityTime, SourceOfNews) VALUES (@Date, @Time, @FishingPlaceId, @NumberOfFishesCaught, @NumberOfFishesSeen, @WeatherSummary, @Description, @TideState, @PeakActivityTime, @SourceOfNews); SELECT CAST(SCOPE_IDENTITY() AS INT);",
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
                SourceOfNews = string.IsNullOrWhiteSpace(record.SourceOfNews) ? null : record.SourceOfNews
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

        await using var connection = await OpenConnectionAsync(cancellationToken);

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

        _logger.LogInformation(
            "Persisted {Count} weather observations for fishing news {NewsId}.",
            weatherDetails.Count,
            fishingNewsId);
    }

    public async Task UpsertImagesAsync(int fishingNewsId, IReadOnlyCollection<FishingNewsImage> images, CancellationToken cancellationToken)
    {
        if (images.Count == 0)
        {
            return;
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);

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

        _logger.LogInformation(
            "Persisted {Count} images for fishing news {NewsId}.",
            images.Count,
            fishingNewsId);
    }

    private static int InferFishingPlaceType(string name)
    {
        if (name.Contains("vatn", StringComparison.OrdinalIgnoreCase) || name.Contains("lake", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 2;
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = Volatile.Read(ref _connectionString);
        var connection = new SqlConnection(connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
        catch (SqlException ex) when (ShouldFallbackToSqlAuthentication(ex, connectionString))
        {
            await connection.DisposeAsync().ConfigureAwait(false);

            if (_fallbackConnectionString is null)
            {
                throw new InvalidOperationException(
                    "Failed to connect using integrated security and no SQL authentication fallback was configured. Configure Database:UserId and Database:Password or disable Database:UseIntegratedSecurity.",
                    ex);
            }

            _logger.LogWarning(ex, "Integrated security connection failed. Falling back to SQL authentication.");

            var newConnectionString = _fallbackConnectionString;
            Interlocked.Exchange(ref _connectionString, newConnectionString);

            var fallbackConnection = new SqlConnection(newConnectionString);
            await fallbackConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return fallbackConnection;
        }
    }

    private bool ShouldFallbackToSqlAuthentication(SqlException exception, string attemptedConnectionString)
    {
        if (!ReferenceEquals(attemptedConnectionString, _primaryConnectionString) && attemptedConnectionString != _primaryConnectionString)
        {
            return false;
        }

        return exception.Message.Contains("SSPI", StringComparison.OrdinalIgnoreCase);
    }
}
