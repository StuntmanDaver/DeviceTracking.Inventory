using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace DeviceTracking.Inventory.Infrastructure.Database;

/// <summary>
/// Database configuration and connection management for the Inventory system
/// </summary>
public static class DatabaseConfig
{
    /// <summary>
    /// Gets the connection string for the Inventory database
    /// </summary>
    public static string GetInventoryConnectionString(IConfiguration configuration)
    {
        return configuration.GetConnectionString("InventoryDb")
               ?? throw new InvalidOperationException("Inventory database connection string is not configured");
    }

    /// <summary>
    /// Gets the connection string for the Device Tracking database
    /// </summary>
    public static string GetDeviceTrackingConnectionString(IConfiguration configuration)
    {
        return configuration.GetConnectionString("DeviceTrackingDb")
               ?? throw new InvalidOperationException("Device Tracking database connection string is not configured");
    }

    /// <summary>
    /// Configures SQL Server options for optimal performance and reliability
    /// </summary>
    public static void ConfigureSqlServerOptions(Action<object> configureOptions)
    {
        // This will be configured in the DbContext OnConfiguring method
        // For now, we'll use the default EF Core SQL Server configuration
    }

    /// <summary>
    /// Creates and returns a new SQL Server connection with proper configuration
    /// </summary>
    public static SqlConnection CreateInventoryConnection(IConfiguration configuration)
    {
        var connectionString = GetInventoryConnectionString(configuration);
        var connection = new SqlConnection(connectionString);

        // Configure connection pooling and timeouts
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            MaxPoolSize = 100,
            MinPoolSize = 5,
            ConnectTimeout = 30,
            CommandTimeout = 30,
            Pooling = true
        };

        connection.ConnectionString = builder.ToString();
        return connection;
    }

    /// <summary>
    /// Validates database connectivity
    /// </summary>
    public static async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
