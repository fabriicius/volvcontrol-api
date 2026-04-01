using Dapper;
using MySqlConnector;
using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.repository.logging;

namespace volvcontrol_api.repository.repo;

public class ServiceRequestRepository : IServiceRequestRepository
{
    private readonly string _connectionString;
    private readonly RepositoryErrorLogger _errorLogger;

    public ServiceRequestRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _errorLogger = new RepositoryErrorLogger(_connectionString);
    }

    public async Task<ServiceRequest> CreateAsync(ServiceRequestCreateRequest request, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;

            const string sql = @"
            INSERT INTO service_request
                (client_id, equipment_id, user_id, status_service_request_id, request_number, type_maintenance_record_id,
                 created_by, description, scheduled_date, scheduled_time, assigned_technician, completion_date, notes, created_date, updated_date)
            VALUES
                (@ClientId, @EquipmentId, @UserId, @StatusServiceRequestId, @RequestNumber, @TypeMaintenanceRecordId,
                 @CreatedBy, @Description, @ScheduledDate, @ScheduledTime, @AssignedTechnician, @CompletionDate, @Notes, @CreatedDate, @UpdatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var param = new
            {
                request.ClientId,
                request.EquipmentId,
                request.UserId,
                request.StatusServiceRequestId,
                request.RequestNumber,
                request.TypeMaintenanceRecordId,
                request.CreatedBy,
                request.Description,
                ScheduledDate = request.ScheduledDate.HasValue ? request.ScheduledDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                ScheduledTime = request.ScheduledTime.HasValue ? request.ScheduledTime.Value.ToTimeSpan() : (TimeSpan?)null,
                request.AssignedTechnician,
                CompletionDate = request.CompletionDate.HasValue ? request.CompletionDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                request.Notes,
                CreatedDate = now,
                UpdatedDate = now
            };

            var id = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, param, transaction: transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);

            return new ServiceRequest
            {
                Id = id,
                ClientId = request.ClientId,
                EquipmentId = request.EquipmentId,
                UserId = request.UserId,
                StatusServiceRequestId = request.StatusServiceRequestId,
                RequestNumber = request.RequestNumber,
                TypeMaintenanceRecordId = request.TypeMaintenanceRecordId,
                CreatedBy = request.CreatedBy,
                Description = request.Description,
                ScheduledDate = request.ScheduledDate,
                ScheduledTime = request.ScheduledTime,
                AssignedTechnician = request.AssignedTechnician,
                CompletionDate = request.CompletionDate,
                Notes = request.Notes,
                CreatedDate = now,
                UpdatedDate = now
            };
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(CreateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on CreateAsync (ServiceRequest): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(CreateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on CreateAsync (ServiceRequest): {ex.Message}", ex);
        }
    }

    public async Task<ServiceRequestDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT
                sr.id AS Id,
                sr.client_id AS ClientId,
                sr.equipment_id AS EquipmentId,
                sr.user_id AS UserId,
                sr.status_service_request_id AS StatusServiceRequestId,
                sr.request_number AS RequestNumber,
                sr.type_maintenance_record_id AS TypeMaintenanceRecordId,
                sr.created_by AS CreatedBy,
                sr.description AS Description,
                sr.scheduled_date AS ScheduledDate,
                sr.scheduled_time AS ScheduledTime,
                sr.assigned_technician AS AssignedTechnician,
                sr.completion_date AS CompletionDate,
                sr.notes AS Notes,
                sr.created_date AS CreatedDate,
                sr.updated_date AS UpdatedDate,
                c.name AS ClientName,
                c.document AS ClientDocument,
                ssr.description AS StatusServiceRequestDescription,
                tmr.description AS TypeMaintenanceRecordDescription,
                e.name AS EquipmentName,
                e.qr_code AS EquipmentQrCode,
                u.name AS UserName,
                u.email AS UserEmail
            FROM service_request sr
            INNER JOIN client c ON c.id = sr.client_id
            INNER JOIN status_service_request ssr ON ssr.id = sr.status_service_request_id
            INNER JOIN type_maintenance_record tmr ON tmr.id = sr.type_maintenance_record_id
            INNER JOIN equipment e ON e.id = sr.equipment_id
            INNER JOIN users u ON u.id = sr.user_id
            WHERE sr.id = @Id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var row = await conn.QuerySingleOrDefaultAsync<ServiceRequestDetailRow>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

            if (row is null)
                return null;

            return new ServiceRequestDetail
            {
                Id = row.Id,
                ClientId = row.ClientId,
                EquipmentId = row.EquipmentId,
                UserId = row.UserId,
                StatusServiceRequestId = row.StatusServiceRequestId,
                RequestNumber = row.RequestNumber ?? string.Empty,
                TypeMaintenanceRecordId = row.TypeMaintenanceRecordId,
                CreatedBy = row.CreatedBy ?? string.Empty,
                Description = row.Description ?? string.Empty,
                ScheduledDate = row.ScheduledDate.HasValue ? DateOnly.FromDateTime(row.ScheduledDate.Value) : null,
                ScheduledTime = row.ScheduledTime.HasValue ? TimeOnly.FromTimeSpan(row.ScheduledTime.Value) : null,
                AssignedTechnician = row.AssignedTechnician ?? string.Empty,
                CompletionDate = row.CompletionDate.HasValue ? DateOnly.FromDateTime(row.CompletionDate.Value) : null,
                Notes = row.Notes ?? string.Empty,
                CreatedDate = row.CreatedDate,
                UpdatedDate = row.UpdatedDate,
                ClientName = row.ClientName ?? string.Empty,
                ClientDocument = row.ClientDocument ?? string.Empty,
                StatusServiceRequestDescription = row.StatusServiceRequestDescription ?? string.Empty,
                TypeMaintenanceRecordDescription = row.TypeMaintenanceRecordDescription ?? string.Empty,
                EquipmentName = row.EquipmentName ?? string.Empty,
                EquipmentQrCode = row.EquipmentQrCode ?? string.Empty,
                UserName = row.UserName ?? string.Empty,
                UserEmail = row.UserEmail ?? string.Empty
            };
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(GetByIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetByIdAsync (ServiceRequest): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(GetByIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetByIdAsync (ServiceRequest): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<ServiceRequestListItem>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT
                sr.id AS Id,
                sr.request_number AS RequestNumber,
                sr.client_id AS ClientId,
                c.name AS ClientName,
                c.document AS ClientDocument,
                sr.status_service_request_id AS StatusServiceRequestId,
                ssr.description AS StatusServiceRequestDescription,
                sr.type_maintenance_record_id AS TypeMaintenanceRecordId,
                tmr.description AS TypeMaintenanceRecordDescription
            FROM service_request sr
            INNER JOIN client c ON c.id = sr.client_id
            INNER JOIN users u ON u.id = @UserId
            INNER JOIN status_service_request ssr ON ssr.id = sr.status_service_request_id
            INNER JOIN type_maintenance_record tmr ON tmr.id = sr.type_maintenance_record_id
            WHERE
                (u.users_position_id = 1 AND sr.user_id = @UserId)
                OR
                (u.users_position_id = 2 AND c.user_id = @UserId)
            ORDER BY sr.id DESC";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<ServiceRequestListItem>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(GetAllByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetAllByUserIdAsync (ServiceRequest): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(GetAllByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetAllByUserIdAsync (ServiceRequest): {ex.Message}", ex);
        }
    }

    public async Task<bool> ExistsByRequestNumberAsync(string requestNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"SELECT 1 FROM service_request WHERE request_number = @RequestNumber LIMIT 1";
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var exists = await conn.QueryFirstOrDefaultAsync<int?>(
                new CommandDefinition(sql, new { RequestNumber = requestNumber }, cancellationToken: cancellationToken));
            return exists.HasValue;
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(ExistsByRequestNumberAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on ExistsByRequestNumberAsync (ServiceRequest): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(ExistsByRequestNumberAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on ExistsByRequestNumberAsync (ServiceRequest): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<ServiceTypeMaintenanceRecord>> GetServiceTypeMaintenanceRecordsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT id AS Id, description AS Description, created_date AS CreatedDate, updated_date AS UpdatedDate
            FROM type_maintenance_record
            ORDER BY id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<ServiceTypeMaintenanceRecord>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(GetServiceTypeMaintenanceRecordsAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetServiceTypeMaintenanceRecordsAsync (ServiceRequest): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(GetServiceTypeMaintenanceRecordsAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetServiceTypeMaintenanceRecordsAsync (ServiceRequest): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<StatusServiceRequest>> GetStatusServiceRequestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT id AS Id, description AS Description, created_date AS CreatedDate, updated_date AS UpdatedDate
            FROM status_service_request
            ORDER BY id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<StatusServiceRequest>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(GetStatusServiceRequestsAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetStatusServiceRequestsAsync (ServiceRequest): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ServiceRequestRepository), nameof(GetStatusServiceRequestsAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetStatusServiceRequestsAsync (ServiceRequest): {ex.Message}", ex);
        }
    }

    private sealed class ServiceRequestDetailRow
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int EquipmentId { get; set; }
        public int UserId { get; set; }
        public int StatusServiceRequestId { get; set; }
        public string? RequestNumber { get; set; }
        public int TypeMaintenanceRecordId { get; set; }
        public string? CreatedBy { get; set; }
        public string? Description { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public TimeSpan? ScheduledTime { get; set; }
        public string? AssignedTechnician { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? ClientName { get; set; }
        public string? ClientDocument { get; set; }
        public string? StatusServiceRequestDescription { get; set; }
        public string? TypeMaintenanceRecordDescription { get; set; }
        public string? EquipmentName { get; set; }
        public string? EquipmentQrCode { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
    }
}
