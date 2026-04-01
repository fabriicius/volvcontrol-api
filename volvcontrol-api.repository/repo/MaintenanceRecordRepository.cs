using Dapper;
using MySqlConnector;
using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.repository.logging;

namespace volvcontrol_api.repository.repo;

public class MaintenanceRecordRepository : IMaintenanceRecordRepository
{
    private readonly string _connectionString;
    private readonly RepositoryErrorLogger _errorLogger;

    public MaintenanceRecordRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _errorLogger = new RepositoryErrorLogger(_connectionString);
    }

    public async Task<MaintenanceRecord> CreateAsync(MaintenanceRecordCreateRequest request, IReadOnlyList<MaintenancePhotoCreate> photos, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;

            const string sql = @"
            INSERT INTO maintenance_record
                (equipment_id, client_id, service_date, service_request_id, status_maintenance_record_id, service_type_maintenance_record_id,
                 created_by_user_id, start_time, end_time, symptoms_reported, diagnosis, services_performed, client_signature, observations,
                 next_maintenance_date, created_date, updated_date)
            VALUES
                (@EquipmentId, @ClientId, @ServiceDate, @ServiceRequestId, @StatusMaintenanceRecordId, @ServiceTypeMaintenanceRecordId,
                 @CreatedByUserId, @StartTime, @EndTime, @SymptomsReported, @Diagnosis, @ServicesPerformed, @ClientSignature, @Observations,
                 @NextMaintenanceDate, @CreatedDate, @UpdatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var param = new
            {
                request.EquipmentId,
                request.ClientId,
                ServiceDate = request.ServiceDate.HasValue ? request.ServiceDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                request.ServiceRequestId,
                request.StatusMaintenanceRecordId,
                request.ServiceTypeMaintenanceRecordId,
                request.CreatedByUserId,
                StartTime = request.StartTime.HasValue ? request.StartTime.Value.ToTimeSpan() : (TimeSpan?)null,
                EndTime = request.EndTime.HasValue ? request.EndTime.Value.ToTimeSpan() : (TimeSpan?)null,
                request.SymptomsReported,
                request.Diagnosis,
                request.ServicesPerformed,
                request.ClientSignature,
                request.Observations,
                NextMaintenanceDate = request.NextMaintenanceDate.HasValue ? request.NextMaintenanceDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                CreatedDate = now,
                UpdatedDate = now
            };

            var id = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, param, transaction: transaction, cancellationToken: cancellationToken));
            await InsertToolsAsync(conn, transaction, id, request.Tools, now, cancellationToken);
            await InsertPhotosAsync(conn, transaction, id, photos, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new MaintenanceRecord
            {
                Id = id,
                EquipmentId = request.EquipmentId,
                ClientId = request.ClientId,
                ServiceDate = request.ServiceDate,
                ServiceRequestId = request.ServiceRequestId,
                StatusMaintenanceRecordId = request.StatusMaintenanceRecordId,
                ServiceTypeMaintenanceRecordId = request.ServiceTypeMaintenanceRecordId,
                CreatedByUserId = request.CreatedByUserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                SymptomsReported = request.SymptomsReported,
                Diagnosis = request.Diagnosis,
                ServicesPerformed = request.ServicesPerformed,
                ClientSignature = request.ClientSignature,
                Observations = request.Observations,
                NextMaintenanceDate = request.NextMaintenanceDate,
                CreatedDate = now,
                UpdatedDate = now
            };
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(CreateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on CreateAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(CreateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on CreateAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    public async Task<MaintenanceRecord?> UpdateAsync(MaintenanceRecordUpdateRequest request, IReadOnlyList<MaintenancePhotoCreate> photos, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            const string sql = @"
            UPDATE maintenance_record
            SET equipment_id = @EquipmentId,
                client_id = @ClientId,
                service_date = @ServiceDate,
                service_request_id = @ServiceRequestId,
                status_maintenance_record_id = @StatusMaintenanceRecordId,
                service_type_maintenance_record_id = @ServiceTypeMaintenanceRecordId,
                created_by_user_id = @CreatedByUserId,
                start_time = @StartTime,
                end_time = @EndTime,
                symptoms_reported = @SymptomsReported,
                diagnosis = @Diagnosis,
                services_performed = @ServicesPerformed,
                client_signature = @ClientSignature,
                observations = @Observations,
                next_maintenance_date = @NextMaintenanceDate,
                updated_date = @UpdatedDate
            WHERE id = @Id";

            var updatedDate = DateTime.UtcNow;
            var param = new
            {
                request.Id,
                request.EquipmentId,
                request.ClientId,
                ServiceDate = request.ServiceDate.HasValue ? request.ServiceDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                request.ServiceRequestId,
                request.StatusMaintenanceRecordId,
                request.ServiceTypeMaintenanceRecordId,
                request.CreatedByUserId,
                StartTime = request.StartTime.HasValue ? request.StartTime.Value.ToTimeSpan() : (TimeSpan?)null,
                EndTime = request.EndTime.HasValue ? request.EndTime.Value.ToTimeSpan() : (TimeSpan?)null,
                request.SymptomsReported,
                request.Diagnosis,
                request.ServicesPerformed,
                request.ClientSignature,
                request.Observations,
                NextMaintenanceDate = request.NextMaintenanceDate.HasValue ? request.NextMaintenanceDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                UpdatedDate = updatedDate
            };

            var rows = await conn.ExecuteAsync(new CommandDefinition(sql, param, transaction: transaction, cancellationToken: cancellationToken));
            if (rows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            await conn.ExecuteAsync(new CommandDefinition(
                "DELETE FROM service_maintenance_tools WHERE maintenance_record_id = @MaintenanceRecordId",
                new { MaintenanceRecordId = request.Id },
                transaction: transaction,
                cancellationToken: cancellationToken));

            await InsertToolsAsync(conn, transaction, request.Id, request.Tools, updatedDate, cancellationToken);
            await ReplacePhotosAsync(conn, transaction, request.Id, photos, updatedDate, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new MaintenanceRecord
            {
                Id = request.Id,
                EquipmentId = request.EquipmentId,
                ClientId = request.ClientId,
                ServiceDate = request.ServiceDate,
                ServiceRequestId = request.ServiceRequestId,
                StatusMaintenanceRecordId = request.StatusMaintenanceRecordId,
                ServiceTypeMaintenanceRecordId = request.ServiceTypeMaintenanceRecordId,
                CreatedByUserId = request.CreatedByUserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                SymptomsReported = request.SymptomsReported,
                Diagnosis = request.Diagnosis,
                ServicesPerformed = request.ServicesPerformed,
                ClientSignature = request.ClientSignature,
                Observations = request.Observations,
                NextMaintenanceDate = request.NextMaintenanceDate,
                UpdatedDate = updatedDate
            };
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(UpdateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on UpdateAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(UpdateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on UpdateAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    public async Task<MaintenanceRecordDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT
                mr.id AS Id,
                mr.equipment_id AS EquipmentId,
                mr.client_id AS ClientId,
                mr.service_date AS ServiceDate,
                mr.service_request_id AS ServiceRequestId,
                mr.status_maintenance_record_id AS StatusMaintenanceRecordId,
                mr.service_type_maintenance_record_id AS ServiceTypeMaintenanceRecordId,
                mr.created_by_user_id AS CreatedByUserId,
                mr.start_time AS StartTime,
                mr.end_time AS EndTime,
                mr.symptoms_reported AS SymptomsReported,
                mr.diagnosis AS Diagnosis,
                mr.services_performed AS ServicesPerformed,
                mr.client_signature AS ClientSignature,
                mr.observations AS Observations,
                mr.next_maintenance_date AS NextMaintenanceDate,
                mr.created_date AS CreatedDate,
                mr.updated_date AS UpdatedDate,
                c.name AS ClientName,
                c.document AS ClientDocument,
                e.name AS EquipmentName,
                e.qr_code AS EquipmentQrCode,
                smr.description AS StatusMaintenanceRecordDescription,
                stmr.description AS ServiceTypeMaintenanceRecordDescription,
                u.name AS CreatedByUserName,
                u.email AS CreatedByUserEmail,
                COALESCE(sr.request_number, '') AS ServiceRequestNumber
            FROM maintenance_record mr
            INNER JOIN client c ON c.id = mr.client_id
            INNER JOIN equipment e ON e.id = mr.equipment_id
            INNER JOIN status_maintenance_record smr ON smr.id = mr.status_maintenance_record_id
            INNER JOIN service_type_maintenance_record stmr ON stmr.id = mr.service_type_maintenance_record_id
            INNER JOIN users u ON u.id = mr.created_by_user_id
            LEFT JOIN service_request sr ON sr.id = mr.service_request_id
            WHERE mr.id = @Id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            var row = await conn.QuerySingleOrDefaultAsync<MaintenanceRecordDetailRow>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
            if (row is null)
                return null;

            var tools = await conn.QueryAsync<ServiceMaintenanceTool>(
                new CommandDefinition(
                    "SELECT id AS Id, maintenance_record_id AS MaintenanceRecordId, description AS Description, created_date AS CreatedDate, updated_date AS UpdatedDate FROM service_maintenance_tools WHERE maintenance_record_id = @MaintenanceRecordId ORDER BY id",
                    new { MaintenanceRecordId = id },
                    cancellationToken: cancellationToken));

            var photos = await conn.QueryAsync<MaintenancePhoto>(
                new CommandDefinition(
                    "SELECT pm.id AS Id, pm.url AS Url, pm.description AS Description, pm.photo_moment AS PhotoMoment, pm.created_date AS CreatedDate, pm.updated_date AS UpdatedDate FROM photos_maintenance pm INNER JOIN photos_maintenance_record pmr ON pmr.photos_maintenance_id = pm.id WHERE pmr.maintenance_record_id = @MaintenanceRecordId ORDER BY pm.id",
                    new { MaintenanceRecordId = id },
                    cancellationToken: cancellationToken));

            return new MaintenanceRecordDetail
            {
                Id = row.Id,
                EquipmentId = row.EquipmentId,
                ClientId = row.ClientId,
                ServiceDate = row.ServiceDate.HasValue ? DateOnly.FromDateTime(row.ServiceDate.Value) : null,
                ServiceRequestId = row.ServiceRequestId,
                StatusMaintenanceRecordId = row.StatusMaintenanceRecordId,
                ServiceTypeMaintenanceRecordId = row.ServiceTypeMaintenanceRecordId,
                CreatedByUserId = row.CreatedByUserId,
                StartTime = row.StartTime.HasValue ? TimeOnly.FromTimeSpan(row.StartTime.Value) : null,
                EndTime = row.EndTime.HasValue ? TimeOnly.FromTimeSpan(row.EndTime.Value) : null,
                SymptomsReported = row.SymptomsReported ?? string.Empty,
                Diagnosis = row.Diagnosis ?? string.Empty,
                ServicesPerformed = row.ServicesPerformed ?? string.Empty,
                ClientSignature = row.ClientSignature ?? string.Empty,
                Observations = row.Observations ?? string.Empty,
                NextMaintenanceDate = row.NextMaintenanceDate.HasValue ? DateOnly.FromDateTime(row.NextMaintenanceDate.Value) : null,
                CreatedDate = row.CreatedDate,
                UpdatedDate = row.UpdatedDate,
                ClientName = row.ClientName ?? string.Empty,
                ClientDocument = row.ClientDocument ?? string.Empty,
                EquipmentName = row.EquipmentName ?? string.Empty,
                EquipmentQrCode = row.EquipmentQrCode ?? string.Empty,
                StatusMaintenanceRecordDescription = row.StatusMaintenanceRecordDescription ?? string.Empty,
                ServiceTypeMaintenanceRecordDescription = row.ServiceTypeMaintenanceRecordDescription ?? string.Empty,
                CreatedByUserName = row.CreatedByUserName ?? string.Empty,
                CreatedByUserEmail = row.CreatedByUserEmail ?? string.Empty,
                ServiceRequestNumber = row.ServiceRequestNumber ?? string.Empty,
                Tools = tools.ToList(),
                Photos = photos.ToList()
            };
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetByIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetByIdAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetByIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetByIdAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<MaintenanceRecordInfoItem>> GetInfoByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT
                mr.id AS Id,
                e.name AS EquipmentName,
                mr.services_performed AS ServicesPerformed,
                c.name AS ClientName,
                stmr.description AS ServiceTypeMaintenanceRecordDescription,
                smr.description AS StatusMaintenanceRecordDescription,
                mr.service_date AS ServiceDate,
                u_created.name AS TechnicianName
            FROM maintenance_record mr
            INNER JOIN equipment e ON e.id = mr.equipment_id
            INNER JOIN client c ON c.id = mr.client_id
            INNER JOIN service_type_maintenance_record stmr ON stmr.id = mr.service_type_maintenance_record_id
            INNER JOIN status_maintenance_record smr ON smr.id = mr.status_maintenance_record_id
            INNER JOIN users u_filter ON u_filter.id = @UserId
            INNER JOIN users u_created ON u_created.id = mr.created_by_user_id
            WHERE
                (u_filter.users_position_id = 1 AND mr.created_by_user_id = @UserId)
                OR
                (u_filter.users_position_id = 2 AND c.user_id = @UserId)
            ORDER BY mr.id DESC";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var rows = await conn.QueryAsync<MaintenanceRecordInfoRow>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
            return rows.Select(r => new MaintenanceRecordInfoItem
            {
                Id = r.Id,
                EquipmentName = r.EquipmentName ?? string.Empty,
                ServicesPerformed = r.ServicesPerformed ?? string.Empty,
                ClientName = r.ClientName ?? string.Empty,
                ServiceTypeMaintenanceRecordDescription = r.ServiceTypeMaintenanceRecordDescription ?? string.Empty,
                StatusMaintenanceRecordDescription = r.StatusMaintenanceRecordDescription ?? string.Empty,
                ServiceDate = r.ServiceDate.HasValue ? DateOnly.FromDateTime(r.ServiceDate.Value) : null,
                TechnicianName = r.TechnicianName ?? string.Empty
            }).ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetInfoByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetInfoByUserIdAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetInfoByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetInfoByUserIdAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<MaintenanceRecordListItem>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT
                mr.id AS Id,
                mr.client_id AS ClientId,
                c.name AS ClientName,
                c.document AS ClientDocument,
                mr.equipment_id AS EquipmentId,
                e.name AS EquipmentName,
                e.qr_code AS EquipmentQrCode,
                mr.status_maintenance_record_id AS StatusMaintenanceRecordId,
                smr.description AS StatusMaintenanceRecordDescription,
                mr.service_type_maintenance_record_id AS ServiceTypeMaintenanceRecordId,
                stmr.description AS ServiceTypeMaintenanceRecordDescription,
                mr.service_date AS ServiceDate,
                mr.created_date AS CreatedDate
            FROM maintenance_record mr
            INNER JOIN client c ON c.id = mr.client_id
            INNER JOIN equipment e ON e.id = mr.equipment_id
            INNER JOIN users u ON u.id = @UserId
            INNER JOIN status_maintenance_record smr ON smr.id = mr.status_maintenance_record_id
            INNER JOIN service_type_maintenance_record stmr ON stmr.id = mr.service_type_maintenance_record_id
            WHERE
                (u.users_position_id = 1 AND mr.created_by_user_id = @UserId)
                OR
                (u.users_position_id = 2 AND c.user_id = @UserId)
            ORDER BY mr.id DESC";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var rows = await conn.QueryAsync<MaintenanceRecordListRow>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
            return rows.Select(r => new MaintenanceRecordListItem
            {
                Id = r.Id,
                ClientId = r.ClientId,
                ClientName = r.ClientName ?? string.Empty,
                ClientDocument = r.ClientDocument ?? string.Empty,
                EquipmentId = r.EquipmentId,
                EquipmentName = r.EquipmentName ?? string.Empty,
                EquipmentQrCode = r.EquipmentQrCode ?? string.Empty,
                StatusMaintenanceRecordId = r.StatusMaintenanceRecordId,
                StatusMaintenanceRecordDescription = r.StatusMaintenanceRecordDescription ?? string.Empty,
                ServiceTypeMaintenanceRecordId = r.ServiceTypeMaintenanceRecordId,
                ServiceTypeMaintenanceRecordDescription = r.ServiceTypeMaintenanceRecordDescription ?? string.Empty,
                ServiceDate = r.ServiceDate.HasValue ? DateOnly.FromDateTime(r.ServiceDate.Value) : null,
                CreatedDate = r.CreatedDate
            }).ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetAllByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetAllByUserIdAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetAllByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetAllByUserIdAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<MaintenanceRecordListItem>> GetAllByServiceRequestIdAsync(int serviceRequestId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT
                mr.id AS Id,
                mr.client_id AS ClientId,
                c.name AS ClientName,
                c.document AS ClientDocument,
                mr.equipment_id AS EquipmentId,
                e.name AS EquipmentName,
                e.qr_code AS EquipmentQrCode,
                mr.status_maintenance_record_id AS StatusMaintenanceRecordId,
                smr.description AS StatusMaintenanceRecordDescription,
                mr.service_type_maintenance_record_id AS ServiceTypeMaintenanceRecordId,
                stmr.description AS ServiceTypeMaintenanceRecordDescription,
                mr.service_date AS ServiceDate,
                mr.created_date AS CreatedDate
            FROM maintenance_record mr
            INNER JOIN client c ON c.id = mr.client_id
            INNER JOIN equipment e ON e.id = mr.equipment_id
            INNER JOIN status_maintenance_record smr ON smr.id = mr.status_maintenance_record_id
            INNER JOIN service_type_maintenance_record stmr ON stmr.id = mr.service_type_maintenance_record_id
            WHERE mr.service_request_id = @ServiceRequestId
            ORDER BY mr.id DESC";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var rows = await conn.QueryAsync<MaintenanceRecordListRow>(new CommandDefinition(sql, new { ServiceRequestId = serviceRequestId }, cancellationToken: cancellationToken));
            return rows.Select(r => new MaintenanceRecordListItem
            {
                Id = r.Id,
                ClientId = r.ClientId,
                ClientName = r.ClientName ?? string.Empty,
                ClientDocument = r.ClientDocument ?? string.Empty,
                EquipmentId = r.EquipmentId,
                EquipmentName = r.EquipmentName ?? string.Empty,
                EquipmentQrCode = r.EquipmentQrCode ?? string.Empty,
                StatusMaintenanceRecordId = r.StatusMaintenanceRecordId,
                StatusMaintenanceRecordDescription = r.StatusMaintenanceRecordDescription ?? string.Empty,
                ServiceTypeMaintenanceRecordId = r.ServiceTypeMaintenanceRecordId,
                ServiceTypeMaintenanceRecordDescription = r.ServiceTypeMaintenanceRecordDescription ?? string.Empty,
                ServiceDate = r.ServiceDate.HasValue ? DateOnly.FromDateTime(r.ServiceDate.Value) : null,
                CreatedDate = r.CreatedDate
            }).ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetAllByServiceRequestIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetAllByServiceRequestIdAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetAllByServiceRequestIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetAllByServiceRequestIdAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(
                "DELETE FROM service_maintenance_tools WHERE maintenance_record_id = @MaintenanceRecordId",
                new { MaintenanceRecordId = id },
                transaction: transaction,
                cancellationToken: cancellationToken));

            await DeletePhotosByMaintenanceRecordAsync(conn, transaction, id, cancellationToken);

            var rows = await conn.ExecuteAsync(new CommandDefinition(
                "DELETE FROM maintenance_record WHERE id = @Id",
                new { Id = id },
                transaction: transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return rows > 0;
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(DeleteAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on DeleteAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(DeleteAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on DeleteAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<ServiceTypeMaintenanceRecord>> GetServiceTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT id AS Id, description AS Description, created_date AS CreatedDate, updated_date AS UpdatedDate
            FROM service_type_maintenance_record
            ORDER BY id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<ServiceTypeMaintenanceRecord>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetServiceTypesAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetServiceTypesAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetServiceTypesAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetServiceTypesAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<StatusMaintenanceRecord>> GetStatusesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT id AS Id, description AS Description, created_date AS CreatedDate, updated_date AS UpdatedDate
            FROM status_maintenance_record
            ORDER BY id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<StatusMaintenanceRecord>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetStatusesAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetStatusesAsync (MaintenanceRecord): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(MaintenanceRecordRepository), nameof(GetStatusesAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetStatusesAsync (MaintenanceRecord): {ex.Message}", ex);
        }
    }

    private static async Task InsertToolsAsync(
        MySqlConnection conn,
        MySqlTransaction transaction,
        int maintenanceRecordId,
        IEnumerable<string>? tools,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var normalized = tools?
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (normalized.Count == 0)
            return;

        const string sql = @"
        INSERT INTO service_maintenance_tools (maintenance_record_id, description, created_date, updated_date)
        VALUES (@MaintenanceRecordId, @Description, @CreatedDate, @UpdatedDate)";

        foreach (var tool in normalized)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    MaintenanceRecordId = maintenanceRecordId,
                    Description = tool,
                    CreatedDate = now,
                    UpdatedDate = now
                },
                transaction: transaction,
                cancellationToken: cancellationToken));
        }
    }

    private static async Task InsertPhotosAsync(
        MySqlConnection conn,
        MySqlTransaction transaction,
        int maintenanceRecordId,
        IReadOnlyList<MaintenancePhotoCreate>? photos,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (photos is null || photos.Count == 0)
            return;

        const string sqlPhoto = @"
        INSERT INTO photos_maintenance (url, description, photo_moment, created_date, updated_date)
        VALUES (@Url, @Description, @PhotoMoment, @CreatedDate, @UpdatedDate);
        SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

        const string sqlLink = @"
        INSERT INTO photos_maintenance_record (maintenance_record_id, photos_maintenance_id)
        VALUES (@MaintenanceRecordId, @PhotosMaintenanceId)";

        foreach (var photo in photos)
        {
            var photoId = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                sqlPhoto,
                new
                {
                    photo.Url,
                    photo.Description,
                    PhotoMoment = photo.PhotoMoment,
                    CreatedDate = now,
                    UpdatedDate = now
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            await conn.ExecuteAsync(new CommandDefinition(
                sqlLink,
                new
                {
                    MaintenanceRecordId = maintenanceRecordId,
                    PhotosMaintenanceId = photoId
                },
                transaction: transaction,
                cancellationToken: cancellationToken));
        }
    }

    private static async Task ReplacePhotosAsync(
        MySqlConnection conn,
        MySqlTransaction transaction,
        int maintenanceRecordId,
        IReadOnlyList<MaintenancePhotoCreate>? photos,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await DeletePhotosByMaintenanceRecordAsync(conn, transaction, maintenanceRecordId, cancellationToken);
        await InsertPhotosAsync(conn, transaction, maintenanceRecordId, photos, now, cancellationToken);
    }

    private static async Task DeletePhotosByMaintenanceRecordAsync(
        MySqlConnection conn,
        MySqlTransaction transaction,
        int maintenanceRecordId,
        CancellationToken cancellationToken)
    {
        var photoIds = (await conn.QueryAsync<int>(new CommandDefinition(
            "SELECT photos_maintenance_id FROM photos_maintenance_record WHERE maintenance_record_id = @MaintenanceRecordId",
            new { MaintenanceRecordId = maintenanceRecordId },
            transaction: transaction,
            cancellationToken: cancellationToken))).ToList();

        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM photos_maintenance_record WHERE maintenance_record_id = @MaintenanceRecordId",
            new { MaintenanceRecordId = maintenanceRecordId },
            transaction: transaction,
            cancellationToken: cancellationToken));

        if (photoIds.Count == 0)
            return;

        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM photos_maintenance WHERE id IN @Ids",
            new { Ids = photoIds },
            transaction: transaction,
            cancellationToken: cancellationToken));
    }

    private sealed class MaintenanceRecordListRow
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientDocument { get; set; }
        public int EquipmentId { get; set; }
        public string? EquipmentName { get; set; }
        public string? EquipmentQrCode { get; set; }
        public int StatusMaintenanceRecordId { get; set; }
        public string? StatusMaintenanceRecordDescription { get; set; }
        public int ServiceTypeMaintenanceRecordId { get; set; }
        public string? ServiceTypeMaintenanceRecordDescription { get; set; }
        public DateTime? ServiceDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    private sealed class MaintenanceRecordInfoRow
    {
        public int Id { get; set; }
        public string? EquipmentName { get; set; }
        public string? ServicesPerformed { get; set; }
        public string? ClientName { get; set; }
        public string? ServiceTypeMaintenanceRecordDescription { get; set; }
        public string? StatusMaintenanceRecordDescription { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string? TechnicianName { get; set; }
    }

    private sealed class MaintenanceRecordDetailRow
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public int ClientId { get; set; }
        public DateTime? ServiceDate { get; set; }
        public int? ServiceRequestId { get; set; }
        public int StatusMaintenanceRecordId { get; set; }
        public int ServiceTypeMaintenanceRecordId { get; set; }
        public int CreatedByUserId { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? SymptomsReported { get; set; }
        public string? Diagnosis { get; set; }
        public string? ServicesPerformed { get; set; }
        public string? ClientSignature { get; set; }
        public string? Observations { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? ClientName { get; set; }
        public string? ClientDocument { get; set; }
        public string? EquipmentName { get; set; }
        public string? EquipmentQrCode { get; set; }
        public string? StatusMaintenanceRecordDescription { get; set; }
        public string? ServiceTypeMaintenanceRecordDescription { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? CreatedByUserEmail { get; set; }
        public string? ServiceRequestNumber { get; set; }
    }
}
