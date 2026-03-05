using Dapper;
using MySqlConnector;
using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.repository.logging;

namespace volvcontrol_api.repository.repo;

public class ClientRepository : IClientRepository
{
    private readonly string _connectionString;
    private readonly RepositoryErrorLogger _errorLogger;

    public ClientRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _errorLogger = new RepositoryErrorLogger(_connectionString);
    }

    public async Task<Client> CreateAsync(ClientCreateRequest request, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;

            const string sqlClient = @"
            INSERT INTO client (user_id, plans_id, status_client_id, name, document, email, phone, notes, created_date, updated_date)
            VALUES (@UserId, @PlansId, @StatusClientId, @Name, @Document, @Email, @Phone, @Notes, @CreatedDate, @UpdatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var paramClient = new
            {
                request.UserId,
                request.PlansId,
                request.StatusClientId,
                request.Name,
                request.Document,
                request.Email,
                request.Phone,
                request.Notes,
                CreatedDate = now,
                UpdatedDate = now
            };

            var clientId = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sqlClient, paramClient, transaction: transaction, cancellationToken: cancellationToken));

            const string sqlAddress = @"
            INSERT INTO address (street, neighborhood, number, zip_code, sigla, city, complement, created_by, created_date, updated_date)
            VALUES (@Street, @Neighborhood, @Number, @ZipCode, @Sigla, @City, @Complement, @CreatedBy, @CreatedDate, @UpdatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var paramAddress = new
            {
                request.Street,
                request.Neighborhood,
                request.Number,
                request.ZipCode,
                request.Sigla,
                request.City,
                request.Complement,
                request.CreatedBy,
                CreatedDate = now,
                UpdatedDate = now
            };

            var addressId = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sqlAddress, paramAddress, transaction: transaction, cancellationToken: cancellationToken));

            const string sqlAddressClient = @"
            INSERT INTO address_client (address_id, client_id)
            VALUES (@AddressId, @ClientId);";

            await conn.ExecuteAsync(new CommandDefinition(sqlAddressClient, new { AddressId = addressId, ClientId = clientId }, transaction: transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            return new Client
            {
                Id = clientId,
                UserId = request.UserId,
                PlansId = request.PlansId,
                StatusClientId = request.StatusClientId,
                Name = request.Name,
                Document = request.Document,
                Email = request.Email,
                Phone = request.Phone,
                Notes = request.Notes,
                CreatedDate = now,
                UpdatedDate = now
            };
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(CreateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on CreateAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(CreateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on CreateAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<Client?> UpdateAsync(ClientUpdateRequest request, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            const string sqlUpdate = @"
            UPDATE client
            SET plans_id = @PlansId, status_client_id = @StatusClientId, name = @Name, document = @Document,
                email = @Email, phone = @Phone, notes = @Notes, updated_date = @UpdatedDate
            WHERE id = @Id";

            var updatedDate = DateTime.UtcNow;
            var paramUpdate = new
            {
                request.Id,
                request.PlansId,
                request.StatusClientId,
                request.Name,
                request.Document,
                request.Email,
                request.Phone,
                request.Notes,
                UpdatedDate = updatedDate
            };

            var rowsAffected = await conn.ExecuteAsync(new CommandDefinition(sqlUpdate, paramUpdate, transaction: transaction, cancellationToken: cancellationToken));
            if (rowsAffected == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            if (request.Addresses is { Count: > 0 })
            {
                const string sqlCurrentIds = "SELECT address_id FROM address_client WHERE client_id = @ClientId";
                var currentIds = (await conn.QueryAsync<int>(new CommandDefinition(sqlCurrentIds, new { ClientId = request.Id }, transaction: transaction, cancellationToken: cancellationToken))).ToHashSet();
                var idsInRequest = new HashSet<int>();

                foreach (var addr in request.Addresses)
                {
                    if (addr.Id > 0)
                    {
                        const string sqlUpdateAddr = @"
                        UPDATE address a
                        INNER JOIN address_client ac ON a.id = ac.address_id AND ac.client_id = @ClientId
                        SET a.street = @Street, a.neighborhood = @Neighborhood, a.number = @Number,
                            a.zip_code = @ZipCode, a.sigla = @Sigla, a.city = @City, a.complement = @Complement, a.updated_date = @UpdatedDate
                        WHERE a.id = @Id";
                        await conn.ExecuteAsync(new CommandDefinition(sqlUpdateAddr, new
                        {
                            ClientId = request.Id,
                            addr.Id,
                            addr.Street,
                            addr.Neighborhood,
                            addr.Number,
                            addr.ZipCode,
                            addr.Sigla,
                            addr.City,
                            addr.Complement,
                            UpdatedDate = updatedDate
                        }, transaction: transaction, cancellationToken: cancellationToken));
                        idsInRequest.Add(addr.Id);
                    }
                    else
                    {
                        const string sqlInsertAddr = @"
                        INSERT INTO address (street, neighborhood, number, zip_code, sigla, city, complement, created_by, created_date, updated_date)
                        VALUES (@Street, @Neighborhood, @Number, @ZipCode, @Sigla, @City, @Complement, @CreatedBy, @CreatedDate, @UpdatedDate);
                        SELECT CAST(LAST_INSERT_ID() AS SIGNED);";
                        var newId = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sqlInsertAddr, new
                        {
                            addr.Street,
                            addr.Neighborhood,
                            addr.Number,
                            addr.ZipCode,
                            addr.Sigla,
                            addr.City,
                            addr.Complement,
                            addr.CreatedBy,
                            CreatedDate = updatedDate,
                            UpdatedDate = updatedDate
                        }, transaction: transaction, cancellationToken: cancellationToken));
                        const string sqlLink = "INSERT INTO address_client (address_id, client_id) VALUES (@AddressId, @ClientId)";
                        await conn.ExecuteAsync(new CommandDefinition(sqlLink, new { AddressId = newId, ClientId = request.Id }, transaction: transaction, cancellationToken: cancellationToken));
                        idsInRequest.Add(newId);
                    }
                }

                var toRemove = currentIds.Except(idsInRequest).ToList();
                foreach (var addressId in toRemove)
                {
                    await conn.ExecuteAsync(new CommandDefinition("DELETE FROM address_client WHERE address_id = @AddressId AND client_id = @ClientId", new { AddressId = addressId, ClientId = request.Id }, transaction: transaction, cancellationToken: cancellationToken));
                    await conn.ExecuteAsync(new CommandDefinition("DELETE FROM address WHERE id = @Id", new { Id = addressId }, transaction: transaction, cancellationToken: cancellationToken));
                }
            }

            await transaction.CommitAsync(cancellationToken);

            const string sqlSelect = @"
            SELECT id AS Id, user_id AS UserId, plans_id AS PlansId, status_client_id AS StatusClientId, name AS Name,
                   document AS Document, email AS Email, phone AS Phone, notes AS Notes,
                   created_date AS CreatedDate, updated_date AS UpdatedDate
            FROM client WHERE id = @Id";
            return await conn.QuerySingleOrDefaultAsync<Client>(new CommandDefinition(sqlSelect, new { request.Id }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(UpdateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on UpdateAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(UpdateAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on UpdateAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"SELECT id AS Id, user_id AS UserId, plans_id AS PlansId, status_client_id AS StatusClientId, name AS Name,
                document AS Document, email AS Email, phone AS Phone, notes AS Notes, created_date AS CreatedDate, updated_date AS UpdatedDate
                FROM client WHERE email = @Email LIMIT 1";
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            return await conn.QuerySingleOrDefaultAsync<Client>(new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetByEmailAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetByEmailAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetByEmailAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetByEmailAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<Client?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"SELECT id AS Id, user_id AS UserId, plans_id AS PlansId, status_client_id AS StatusClientId, name AS Name,
                document AS Document, email AS Email, phone AS Phone, notes AS Notes, created_date AS CreatedDate, updated_date AS UpdatedDate
                FROM client WHERE document = @Document LIMIT 1";
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            return await conn.QuerySingleOrDefaultAsync<Client>(new CommandDefinition(sql, new { Document = document }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetByDocumentAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetByDocumentAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetByDocumentAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetByDocumentAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<Client?> GetAnotherClientByEmailAsync(string email, int excludeClientId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"SELECT id AS Id, user_id AS UserId, plans_id AS PlansId, status_client_id AS StatusClientId, name AS Name,
                document AS Document, email AS Email, phone AS Phone, notes AS Notes, created_date AS CreatedDate, updated_date AS UpdatedDate
                FROM client WHERE email = @Email AND id != @ExcludeClientId LIMIT 1";
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            return await conn.QuerySingleOrDefaultAsync<Client>(new CommandDefinition(sql, new { Email = email, ExcludeClientId = excludeClientId }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetAnotherClientByEmailAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetAnotherClientByEmailAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetAnotherClientByEmailAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetAnotherClientByEmailAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<Client?> GetAnotherClientByDocumentAsync(string document, int excludeClientId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"SELECT id AS Id, user_id AS UserId, plans_id AS PlansId, status_client_id AS StatusClientId, name AS Name,
                document AS Document, email AS Email, phone AS Phone, notes AS Notes, created_date AS CreatedDate, updated_date AS UpdatedDate
                FROM client WHERE document = @Document AND id != @ExcludeClientId LIMIT 1";
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            return await conn.QuerySingleOrDefaultAsync<Client>(new CommandDefinition(sql, new { Document = document, ExcludeClientId = excludeClientId }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetAnotherClientByDocumentAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetAnotherClientByDocumentAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetAnotherClientByDocumentAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetAnotherClientByDocumentAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int clientId, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            var addressIds = (await conn.QueryAsync<int>(new CommandDefinition("SELECT address_id FROM address_client WHERE client_id = @ClientId", new { ClientId = clientId }, transaction: transaction, cancellationToken: cancellationToken))).ToList();
            await conn.ExecuteAsync(new CommandDefinition("DELETE FROM address_client WHERE client_id = @ClientId", new { ClientId = clientId }, transaction: transaction, cancellationToken: cancellationToken));
            foreach (var addressId in addressIds)
                await conn.ExecuteAsync(new CommandDefinition("DELETE FROM address WHERE id = @Id", new { Id = addressId }, transaction: transaction, cancellationToken: cancellationToken));
            var rows = await conn.ExecuteAsync(new CommandDefinition("DELETE FROM client WHERE id = @Id", new { Id = clientId }, transaction: transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return rows > 0;
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(DeleteAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on DeleteAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(DeleteAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on DeleteAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<ClientWithDetails?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string sqlClient = @"
            SELECT c.id AS Id, c.user_id AS UserId, c.plans_id AS PlansId, c.status_client_id AS StatusClientId,
                   p.description AS PlanDescription, s.description AS StatusClientDescription,
                   c.name AS Name, c.document AS Document, c.email AS Email, c.phone AS Phone, c.notes AS Notes,
                   c.created_date AS CreatedDate, c.updated_date AS UpdatedDate
            FROM client c
            LEFT JOIN plans p ON c.plans_id = p.id
            LEFT JOIN status_client s ON c.status_client_id = s.id
            WHERE c.id = @Id";

            var client = await conn.QuerySingleOrDefaultAsync<ClientWithDetails>(new CommandDefinition(sqlClient, new { Id = id }, cancellationToken: cancellationToken));
            if (client is null)
                return null;

            const string sqlAddresses = @"
            SELECT a.id AS Id, a.street AS Street, a.neighborhood AS Neighborhood, a.number AS Number,
                   a.zip_code AS ZipCode, a.sigla AS Sigla, a.city AS City, a.complement AS Complement,
                   a.created_by AS CreatedBy, a.created_date AS CreatedDate, a.updated_date AS UpdatedDate
            FROM address a
            INNER JOIN address_client ac ON a.id = ac.address_id
            WHERE ac.client_id = @ClientId";

            var addresses = (await conn.QueryAsync<Address>(new CommandDefinition(sqlAddresses, new { ClientId = id }, cancellationToken: cancellationToken))).ToList();
            client.Addresses = addresses;

            const string sqlEquipments = @"
            SELECT e.name AS Name,
                   e.qr_code AS QrCode,
                   CASE
                       WHEN e.status_equipment_id = 1 THEN 'Ativo'
                       ELSE 'Inativo'
                   END AS Status
            FROM client c
            INNER JOIN equipment e ON e.client_id = c.id
            WHERE c.id = @ClientId
            ORDER BY e.id DESC";

            var equipments = (await conn.QueryAsync<ClientEquipmentInfo>(new CommandDefinition(sqlEquipments, new { ClientId = id }, cancellationToken: cancellationToken))).ToList();
            client.Equipments = equipments;
            client.EquipmentCount = equipments.Count;

            return client;
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetByIdWithDetailsAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetByIdWithDetailsAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetByIdWithDetailsAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetByIdWithDetailsAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<(IReadOnlyList<ClientListItem> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var offset = (page - 1) * pageSize;

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string sqlCount = "SELECT COUNT(*) FROM client";
            var totalCount = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sqlCount, cancellationToken: cancellationToken));

            const string sqlPage = @"
            SELECT c.id AS Id, c.name AS Name, c.document AS Document,
                   s.description AS StatusDescription, p.description AS PlanDescription
            FROM client c
            LEFT JOIN plans p ON c.plans_id = p.id
            LEFT JOIN status_client s ON c.status_client_id = s.id
            ORDER BY c.id
            LIMIT @PageSize OFFSET @Offset";

            var param = new { PageSize = pageSize, Offset = offset };
            var items = (await conn.QueryAsync<ClientListItem>(new CommandDefinition(sqlPage, param, cancellationToken: cancellationToken))).ToList();

            return (items, totalCount);
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetPagedAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetPagedAsync (Client): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetPagedAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetPagedAsync (Client): {ex.Message}", ex);
        }
    }

    public async Task<Address> CreateAddressForClientAsync(AddressCreateRequest request, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            const string sqlAddress = @"
            INSERT INTO address (street, neighborhood, number, zip_code, sigla, city, complement, created_by, created_date, updated_date)
            VALUES (@Street, @Neighborhood, @Number, @ZipCode, @Sigla, @City, @Complement, @CreatedBy, @CreatedDate, @UpdatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var now = DateTime.UtcNow;
            var paramAddress = new
            {
                request.Street,
                request.Neighborhood,
                request.Number,
                request.ZipCode,
                request.Sigla,
                request.City,
                request.Complement,
                request.CreatedBy,
                CreatedDate = now,
                UpdatedDate = now
            };

            var addressId = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sqlAddress, paramAddress, transaction: transaction, cancellationToken: cancellationToken));

            const string sqlAddressClient = @"
            INSERT INTO address_client (address_id, client_id)
            VALUES (@AddressId, @ClientId);";

            await conn.ExecuteAsync(new CommandDefinition(sqlAddressClient, new { AddressId = addressId, ClientId = request.ClientId }, transaction: transaction, cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            return new Address
            {
                Id = addressId,
                Street = request.Street,
                Neighborhood = request.Neighborhood,
                Number = request.Number,
                ZipCode = request.ZipCode,
                Sigla = request.Sigla,
                City = request.City,
                Complement = request.Complement,
                CreatedBy = request.CreatedBy,
                CreatedDate = now,
                UpdatedDate = now
            };
        }
        catch (MySqlException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(CreateAddressForClientAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on CreateAddressForClientAsync: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(CreateAddressForClientAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on CreateAddressForClientAsync: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<Plan>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"SELECT id AS Id, description AS Description, created_date AS CreatedDate, updated_date AS UpdatedDate FROM plans ORDER BY id";
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<Plan>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetPlansAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetPlansAsync: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetPlansAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetPlansAsync: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<StatusClient>> GetStatusClientsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"SELECT id AS Id, description AS Description, created_date AS CreatedDate, updated_date AS UpdatedDate FROM status_client ORDER BY id";
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<StatusClient>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetStatusClientsAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Database error on GetStatusClientsAsync: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogAsync(nameof(ClientRepository), nameof(GetStatusClientsAsync), ex, cancellationToken);
            throw new InvalidOperationException($"Error on GetStatusClientsAsync: {ex.Message}", ex);
        }
    }
}
