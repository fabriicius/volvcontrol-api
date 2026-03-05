using Dapper;
using MySqlConnector;
using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.repository.logging;

namespace volvcontrol_api.repository.repo;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly string _connectionString;
    private readonly RepositoryErrorLogger _errorLogger;

    public EquipmentRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _errorLogger = new RepositoryErrorLogger(_connectionString);
    }

    public async Task<Equipment> CreateAsync(EquipmentCreateRequest request, string photoUrl, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;

            const string sql = @"
            INSERT INTO equipment (name, client_id, qr_code, equipment_category_id, equipment_type_id, brand, model, installation_date, warranty_until, status_equipment_id, photo_url, notes, created_date, updated_date)
            VALUES (@Name, @ClientId, @QrCode, @EquipmentCategoryId, @EquipmentTypeId, @Brand, @Model, @InstallationDate, @WarrantyUntil, @StatusEquipmentId, @PhotoUrl, @Notes, @CreatedDate, @UpdatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var param = new
            {
                request.Name,
                request.ClientId,
                request.QrCode,
                request.EquipmentCategoryId,
                request.EquipmentTypeId,
                request.Brand,
                request.Model,
                InstallationDate = request.InstallationDate.ToDateTime(TimeOnly.MinValue),
                WarrantyUntil = request.WarrantyUntil.ToDateTime(TimeOnly.MinValue),
                request.StatusEquipmentId,
                PhotoUrl = photoUrl,
                request.Notes,
                CreatedDate = now,
                UpdatedDate = now
            };

            var id = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, param, transaction: transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);

            return new Equipment
            {
                Id = id,
                Name = request.Name,
                ClientId = request.ClientId,
                QrCode = request.QrCode,
                EquipmentCategoryId = request.EquipmentCategoryId,
                EquipmentTypeId = request.EquipmentTypeId,
                Brand = request.Brand,
                Model = request.Model,
                InstallationDate = request.InstallationDate,
                WarrantyUntil = request.WarrantyUntil,
                StatusEquipmentId = request.StatusEquipmentId,
                PhotoUrl = photoUrl,
                Notes = request.Notes,
                CreatedDate = now,
                UpdatedDate = now
            };
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(CreateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on CreateAsync (Equipment): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(CreateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on CreateAsync (Equipment): {ex.Message}", ex);
        }
    }

    public async Task<Equipment?> UpdateAsync(EquipmentUpdateRequest request, string photoUrl, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        try
        {
            const string sql = @"
            UPDATE equipment
            SET name = @Name, client_id = @ClientId, qr_code = @QrCode,
                equipment_category_id = @EquipmentCategoryId, equipment_type_id = @EquipmentTypeId, brand = @Brand, model = @Model,
                installation_date = @InstallationDate, warranty_until = @WarrantyUntil, status_equipment_id = @StatusEquipmentId,
                photo_url = @PhotoUrl, notes = @Notes, updated_date = @UpdatedDate
            WHERE id = @Id";

            var updatedDate = DateTime.UtcNow;
            var param = new
            {
                request.Id,
                request.Name,
                request.ClientId,
                request.QrCode,
                request.EquipmentCategoryId,
                request.EquipmentTypeId,
                request.Brand,
                request.Model,
                InstallationDate = request.InstallationDate.HasValue ? request.InstallationDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                WarrantyUntil = request.WarrantyUntil.HasValue ? request.WarrantyUntil.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                request.StatusEquipmentId,
                PhotoUrl = photoUrl,
                request.Notes,
                UpdatedDate = updatedDate
            };

            var rows = await conn.ExecuteAsync(new CommandDefinition(sql, param, cancellationToken: cancellationToken));
            if (rows == 0)
                return null;

            return new Equipment
            {
                Id = request.Id,
                Name = request.Name,
                ClientId = request.ClientId,
                QrCode = request.QrCode,
                EquipmentCategoryId = request.EquipmentCategoryId,
                EquipmentTypeId = request.EquipmentTypeId,
                Brand = request.Brand,
                Model = request.Model,
                InstallationDate = request.InstallationDate,
                WarrantyUntil = request.WarrantyUntil,
                StatusEquipmentId = request.StatusEquipmentId,
                PhotoUrl = photoUrl,
                Notes = request.Notes,
                UpdatedDate = updatedDate
            };
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(UpdateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on UpdateAsync (Equipment): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(UpdateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on UpdateAsync (Equipment): {ex.Message}", ex);
        }
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string sql = @"
            SELECT e.id AS Id, e.name AS Name, e.client_id AS ClientId, e.qr_code AS QrCode,
                   e.equipment_category_id AS EquipmentCategoryId, e.equipment_type_id AS EquipmentTypeId,
                   ec.description AS CategoryDescription, et.description AS TypeDescription,
                   e.brand AS Brand, e.model AS Model,
                   e.installation_date AS InstallationDate, e.warranty_until AS WarrantyUntil, e.status_equipment_id AS StatusEquipmentId,
                   se.description AS StatusEquipmentDescription,
                   e.photo_url AS PhotoUrl, e.notes AS Notes, e.created_date AS CreatedDate, e.updated_date AS UpdatedDate
            FROM equipment e
            LEFT JOIN equipment_category ec ON e.equipment_category_id = ec.id
            LEFT JOIN equipment_type et ON e.equipment_type_id = et.id
            LEFT JOIN status_equipment se ON e.status_equipment_id = se.id
            WHERE e.id = @Id";

            var row = await conn.QuerySingleOrDefaultAsync<EquipmentRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
            if (row is null)
                return null;

            return new Equipment
            {
                Id = row.Id,
                Name = row.Name ?? string.Empty,
                ClientId = row.ClientId,
                QrCode = row.QrCode ?? string.Empty,
                EquipmentCategoryId = row.EquipmentCategoryId,
                EquipmentTypeId = row.EquipmentTypeId,
                Brand = row.Brand ?? string.Empty,
                Model = row.Model ?? string.Empty,
                CategoryDescription = row.CategoryDescription,
                TypeDescription = row.TypeDescription,
                StatusEquipmentDescription = row.StatusEquipmentDescription,
                InstallationDate = row.InstallationDate.HasValue ? DateOnly.FromDateTime(row.InstallationDate.Value) : null,
                WarrantyUntil = row.WarrantyUntil.HasValue ? DateOnly.FromDateTime(row.WarrantyUntil.Value) : null,
                StatusEquipmentId = row.StatusEquipmentId,
                PhotoUrl = row.PhotoUrl ?? string.Empty,
                Notes = row.Notes ?? string.Empty,
                CreatedDate = row.CreatedDate,
                UpdatedDate = row.UpdatedDate
            };
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(GetByIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetByIdAsync (Equipment): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(GetByIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetByIdAsync (Equipment): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<Equipment>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string sql = @"
            SELECT e.id AS Id, e.name AS Name, e.client_id AS ClientId, e.qr_code AS QrCode,
                   e.equipment_category_id AS EquipmentCategoryId, e.equipment_type_id AS EquipmentTypeId,
                   ec.description AS CategoryDescription, et.description AS TypeDescription,
                   e.brand AS Brand, e.model AS Model,
                   e.installation_date AS InstallationDate, e.warranty_until AS WarrantyUntil, e.status_equipment_id AS StatusEquipmentId,
                   se.description AS StatusEquipmentDescription,
                   e.photo_url AS PhotoUrl, e.notes AS Notes, e.created_date AS CreatedDate, e.updated_date AS UpdatedDate
            FROM equipment e
            INNER JOIN client c ON c.id = e.client_id
            INNER JOIN users u ON u.id = @UserId
            LEFT JOIN equipment_category ec ON e.equipment_category_id = ec.id
            LEFT JOIN equipment_type et ON e.equipment_type_id = et.id
            LEFT JOIN status_equipment se ON e.status_equipment_id = se.id
            WHERE
                (u.users_position_id = 1 AND c.user_id = @UserId)
                OR
                (u.users_position_id = 2 AND c.email = u.email)
            ORDER BY e.id DESC";

            var rows = await conn.QueryAsync<EquipmentRow>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
            return rows.Select(row => new Equipment
            {
                Id = row.Id,
                Name = row.Name ?? string.Empty,
                ClientId = row.ClientId,
                QrCode = row.QrCode ?? string.Empty,
                EquipmentCategoryId = row.EquipmentCategoryId,
                EquipmentTypeId = row.EquipmentTypeId,
                Brand = row.Brand ?? string.Empty,
                Model = row.Model ?? string.Empty,
                CategoryDescription = row.CategoryDescription,
                TypeDescription = row.TypeDescription,
                StatusEquipmentDescription = row.StatusEquipmentDescription,
                InstallationDate = row.InstallationDate.HasValue ? DateOnly.FromDateTime(row.InstallationDate.Value) : null,
                WarrantyUntil = row.WarrantyUntil.HasValue ? DateOnly.FromDateTime(row.WarrantyUntil.Value) : null,
                StatusEquipmentId = row.StatusEquipmentId,
                PhotoUrl = row.PhotoUrl ?? string.Empty,
                Notes = row.Notes ?? string.Empty,
                CreatedDate = row.CreatedDate,
                UpdatedDate = row.UpdatedDate
            }).ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(GetAllByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetAllByUserIdAsync (Equipment): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(GetAllByUserIdAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetAllByUserIdAsync (Equipment): {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var rows = await conn.ExecuteAsync(new CommandDefinition("DELETE FROM equipment WHERE id = @Id", new { Id = id }, cancellationToken: cancellationToken));
            return rows > 0;
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(DeleteAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on DeleteAsync (Equipment): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(DeleteAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on DeleteAsync (Equipment): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<EquipmentCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT id AS Id, description AS Description
            FROM equipment_category
            ORDER BY id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<EquipmentCategory>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(GetCategoriesAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetCategoriesAsync (Equipment): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(GetCategoriesAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetCategoriesAsync (Equipment): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<EquipmentType>> GetTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT id AS Id, description AS Description
            FROM equipment_type
            ORDER BY id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<EquipmentType>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(GetTypesAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetTypesAsync (Equipment): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(EquipmentRepository), nameof(GetTypesAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetTypesAsync (Equipment): {ex.Message}", ex);
        }
    }
}
