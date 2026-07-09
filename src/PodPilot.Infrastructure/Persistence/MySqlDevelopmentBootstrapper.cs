using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace PodPilot.Infrastructure.Persistence;

/// <summary>
/// Ensures the local MySQL database and application user exist during Development startup.
/// Requires <c>ConnectionStrings:AdminConnection</c> with credentials that can create databases and users.
/// </summary>
internal static class MySqlDevelopmentBootstrapper
{
    /// <summary>
    /// Creates the database and application user from <c>ConnectionStrings:DefaultConnection</c> when missing.
    /// </summary>
    public static async Task EnsureDatabaseAndUserAsync(
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var adminConnection = configuration.GetConnectionString("AdminConnection");
        if (string.IsNullOrWhiteSpace(adminConnection))
        {
            return;
        }

        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            return;
        }

        var appConnection = new MySqlConnectionStringBuilder(defaultConnection);
        var databaseName = appConnection.Database;
        var userId = appConnection.UserID;
        var password = appConnection.Password;

        if (string.IsNullOrWhiteSpace(databaseName)
            || string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Skipping MySQL development bootstrap because DefaultConnection is missing Database, User, or Password.");
            return;
        }

        logger.LogInformation("Ensuring MySQL database {Database} and user {User} exist...", databaseName, userId);

        await using var connection = new MySqlConnection(adminConnection);
        await connection.OpenAsync(cancellationToken);

        await ExecuteNonQueryAsync(
            connection,
            $"CREATE DATABASE IF NOT EXISTS `{EscapeMySqlIdentifier(databaseName)}`;",
            cancellationToken);

        var escapedUser = EscapeMySqlStringLiteral(userId);
        var escapedPassword = EscapeMySqlStringLiteral(password);
        var escapedDatabase = EscapeMySqlIdentifier(databaseName);

        var bootstrapSql =
            $"CREATE USER IF NOT EXISTS '{escapedUser}'@'localhost' IDENTIFIED BY '{escapedPassword}'; "
            + $"ALTER USER '{escapedUser}'@'localhost' IDENTIFIED BY '{escapedPassword}'; "
            + $"GRANT ALL PRIVILEGES ON `{escapedDatabase}`.* TO '{escapedUser}'@'localhost'; "
            + "FLUSH PRIVILEGES;";

        await ExecuteNonQueryAsync(connection, bootstrapSql, cancellationToken);

        logger.LogInformation("MySQL database and user are ready.");
    }

    private static async Task ExecuteNonQueryAsync(
        MySqlConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string EscapeMySqlIdentifier(string value) =>
        value.Replace("`", "``");

    private static string EscapeMySqlStringLiteral(string value) =>
        value.Replace("\\", "\\\\").Replace("'", "''");
}
