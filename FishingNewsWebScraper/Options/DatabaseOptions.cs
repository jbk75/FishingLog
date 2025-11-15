using System;
using Microsoft.Data.SqlClient;

namespace FishingNewsWebScraper.Options;

public sealed class DatabaseOptions
{
    /// <summary>
    /// Raw connection string that, when provided, is used as-is.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Optional SQL Server host name.
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// Optional SQL Server database name.
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// Controls whether to use integrated security/Windows authentication.
    /// </summary>
    public bool UseIntegratedSecurity { get; set; } = true;

    /// <summary>
    /// SQL login user id when <see cref="UseIntegratedSecurity"/> is <c>false</c>.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// SQL login password when <see cref="UseIntegratedSecurity"/> is <c>false</c>.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Controls the TrustServerCertificate setting when building the connection string.
    /// Defaults to <c>true</c> to match the previous behaviour.
    /// </summary>
    public bool TrustServerCertificate { get; set; } = true;

    public DatabaseConnectionConfiguration BuildConnectionConfiguration()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);

            if (!UseIntegratedSecurity)
            {
                ApplySqlAuthentication(builder);
            }
            else if (builder.IntegratedSecurity && !OperatingSystem.IsWindows())
            {
                ApplySqlAuthentication(builder, requiredForNonWindows: true);
            }

            if (!builder.ContainsKey(nameof(SqlConnectionStringBuilder.TrustServerCertificate)))
            {
                builder.TrustServerCertificate = TrustServerCertificate;
            }

            var usesIntegratedSecurity = builder.IntegratedSecurity;
            var fallbackConnectionString = BuildFallbackConnectionString(builder, usesIntegratedSecurity);

            return new DatabaseConnectionConfiguration(
                builder.ConnectionString,
                fallbackConnectionString,
                usesIntegratedSecurity);
        }

        if (string.IsNullOrWhiteSpace(Server))
        {
            throw new InvalidOperationException("Database server is not configured. Provide Database:ConnectionString or Database:Server.");
        }

        if (string.IsNullOrWhiteSpace(Database))
        {
            throw new InvalidOperationException("Database name is not configured. Provide Database:ConnectionString or Database:Database.");
        }

        var connectionBuilder = new SqlConnectionStringBuilder
        {
            DataSource = Server,
            InitialCatalog = Database,
            TrustServerCertificate = TrustServerCertificate
        };

        bool usesIntegratedSecurity;

        if (UseIntegratedSecurity)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new InvalidOperationException("Integrated security requires Windows. Disable Database:UseIntegratedSecurity or provide Database:ConnectionString for SQL authentication.");
            }

            connectionBuilder.IntegratedSecurity = true;
            usesIntegratedSecurity = true;
        }
        else
        {
            ApplySqlAuthentication(connectionBuilder);
            usesIntegratedSecurity = false;
        }

        var fallbackConnectionString = BuildFallbackConnectionString(connectionBuilder, usesIntegratedSecurity);

        return new DatabaseConnectionConfiguration(
            connectionBuilder.ConnectionString,
            fallbackConnectionString,
            usesIntegratedSecurity);
    }

    public string BuildConnectionString() => BuildConnectionConfiguration().ConnectionString;

    private void ApplySqlAuthentication(SqlConnectionStringBuilder builder, bool requiredForNonWindows = false)
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            throw new InvalidOperationException(requiredForNonWindows
                ? "Integrated security is unavailable on non-Windows platforms. Configure Database:UserId to use SQL authentication."
                : "Database:UserId must be configured when integrated security is disabled.");
        }

        if (string.IsNullOrEmpty(Password))
        {
            throw new InvalidOperationException(requiredForNonWindows
                ? "Integrated security is unavailable on non-Windows platforms. Configure Database:Password to use SQL authentication."
                : "Database:Password must be configured when integrated security is disabled.");
        }

        builder.UserID = UserId;
        builder.Password = Password;
        builder.IntegratedSecurity = false;
    }

    private string? BuildFallbackConnectionString(SqlConnectionStringBuilder builder, bool usesIntegratedSecurity)
    {
        if (!usesIntegratedSecurity)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrEmpty(Password))
        {
            return null;
        }

        var fallbackBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
        {
            UserID = UserId,
            Password = Password,
            IntegratedSecurity = false
        };

        return fallbackBuilder.ConnectionString;
    }
}

public sealed record DatabaseConnectionConfiguration(string ConnectionString, string? FallbackConnectionString, bool UsesIntegratedSecurity);
