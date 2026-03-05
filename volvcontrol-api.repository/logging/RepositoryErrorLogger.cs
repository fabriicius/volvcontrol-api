using Dapper;
using MySqlConnector;

namespace volvcontrol_api.repository.logging;

public class RepositoryErrorLogger
{
    private readonly string _connectionString;

    public RepositoryErrorLogger(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task LogAsync(string repository, string method, Exception ex, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            INSERT INTO app_error_log (
                occurred_at, repository, method, error_type, message, stack_trace, inner_exception
            )
            VALUES (
                @OccurredAt, @Repository, @Method, @ErrorType, @Message, @StackTrace, @InnerException
            );";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await conn.ExecuteAsync(new CommandDefinition(sql, new
            {
                OccurredAt = DateTime.UtcNow,
                Repository = repository,
                Method = method,
                ErrorType = ex.GetType().FullName ?? "Exception",
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException?.ToString()
            }, cancellationToken: cancellationToken));
        }
        catch
        {
            // Intencionalmente ignorado para não interromper o fluxo original da exceção.
        }
    }
}
