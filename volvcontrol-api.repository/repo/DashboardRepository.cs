using Dapper;
using MySqlConnector;
using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.repository.logging;

namespace volvcontrol_api.repository.repo;

public class DashboardRepository : IDashboardRepository
{
    private readonly string _connectionString;
    private readonly RepositoryErrorLogger _errorLogger;

    private const int StatusOpen = 5;
    private const int StatusScheduled = 6;
    private const int StatusWaitingParts = 7;
    private const int StatusCompleted = 9;
    private const int ActiveClientStatus = 1;

    public DashboardRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _errorLogger = new RepositoryErrorLogger(_connectionString);
    }

    public async Task<DashboardSummary> GetSummaryByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            var totalActiveClients = await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
                SELECT COUNT(*)
                FROM client c
                INNER JOIN users u ON u.id = @UserId
                WHERE c.status_client_id = @ActiveClientStatus
                  AND (
                        (u.users_position_id = 1 AND EXISTS (
                            SELECT 1
                            FROM service_request sr
                            WHERE sr.client_id = c.id
                              AND sr.user_id = @UserId
                        ))
                        OR
                        (u.users_position_id = 2 AND c.user_id = @UserId)
                  )",
                new { UserId = userId, ActiveClientStatus },
                cancellationToken: cancellationToken));

            var totalEquipments = await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
                SELECT COUNT(*)
                FROM equipment e
                INNER JOIN client c ON c.id = e.client_id
                INNER JOIN users u ON u.id = @UserId
                WHERE
                    (u.users_position_id = 1 AND EXISTS (
                        SELECT 1
                        FROM service_request sr
                        WHERE sr.client_id = c.id
                          AND sr.equipment_id = e.id
                          AND sr.user_id = @UserId
                    ))
                    OR
                    (u.users_position_id = 2 AND c.user_id = @UserId)",
                new { UserId = userId },
                cancellationToken: cancellationToken));

            var totalOpenServiceRequests = await GetServiceRequestCountByStatusAsync(conn, userId, StatusOpen, cancellationToken);
            var totalCompletedServiceRequests = await GetServiceRequestCountByStatusAsync(conn, userId, StatusCompleted, cancellationToken);
            var totalScheduledServiceRequests = await GetServiceRequestCountByStatusAsync(conn, userId, StatusScheduled, cancellationToken);
            var totalWaitingPartsServiceRequests = await GetServiceRequestCountByStatusAsync(conn, userId, StatusWaitingParts, cancellationToken);

            var latestRows = await conn.QueryAsync<DashboardLatestServiceRequestRow>(new CommandDefinition(@"
                SELECT
                    e.name AS EquipmentName,
                    c.name AS ClientName,
                    ssr.description AS StatusDescription
                FROM service_request sr
                INNER JOIN equipment e ON e.id = sr.equipment_id
                INNER JOIN client c ON c.id = sr.client_id
                INNER JOIN status_service_request ssr ON ssr.id = sr.status_service_request_id
                INNER JOIN users u ON u.id = @UserId
                WHERE
                    (u.users_position_id = 1 AND sr.user_id = @UserId)
                    OR
                    (u.users_position_id = 2 AND c.user_id = @UserId)
                ORDER BY sr.id DESC
                LIMIT 5",
                new { UserId = userId },
                cancellationToken: cancellationToken));

            return new DashboardSummary
            {
                TotalActiveClients = totalActiveClients,
                TotalEquipments = totalEquipments,
                TotalOpenServiceRequests = totalOpenServiceRequests,
                TotalCompletedServiceRequests = totalCompletedServiceRequests,
                TotalScheduledServiceRequests = totalScheduledServiceRequests,
                TotalWaitingPartsServiceRequests = totalWaitingPartsServiceRequests,
                LatestServiceRequests = latestRows.Select(i => new DashboardLatestServiceRequest
                {
                    EquipmentName = i.EquipmentName ?? string.Empty,
                    ClientName = i.ClientName ?? string.Empty,
                    StatusDescription = i.StatusDescription ?? string.Empty
                }).ToList()
            };
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(DashboardRepository), nameof(GetSummaryByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetSummaryByUserIdAsync (Dashboard): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(DashboardRepository), nameof(GetSummaryByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetSummaryByUserIdAsync (Dashboard): {ex.Message}", ex);
        }
    }

    private static async Task<int> GetServiceRequestCountByStatusAsync(MySqlConnection conn, int userId, int statusId, CancellationToken cancellationToken)
    {
        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(*)
            FROM service_request sr
            INNER JOIN client c ON c.id = sr.client_id
            INNER JOIN users u ON u.id = @UserId
            WHERE sr.status_service_request_id = @StatusId
              AND (
                    (u.users_position_id = 1 AND sr.user_id = @UserId)
                    OR
                    (u.users_position_id = 2 AND c.user_id = @UserId)
              )",
            new { UserId = userId, StatusId = statusId },
            cancellationToken: cancellationToken));
    }

    private sealed class DashboardLatestServiceRequestRow
    {
        public string? EquipmentName { get; set; }
        public string? ClientName { get; set; }
        public string? StatusDescription { get; set; }
    }
}
