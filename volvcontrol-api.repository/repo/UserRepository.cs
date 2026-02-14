using Dapper;
using MySqlConnector;
using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Model;

namespace volvcontrol_api.repository.repo;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<User> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            INSERT INTO users (company_id, users_position_id, name, email, password, status, created_date, updated_date)
            VALUES (@CompanyId, @UsersPositionId, @Name, @Email, @Password, @Status, @CreatedDate, @UpdatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var now = DateTime.UtcNow;
            var param = new
            {
                request.CompanyId,
                request.UsersPositionId,
                request.Name,
                request.Email,
                request.Password,
                request.Status,
                CreatedDate = now,
                UpdatedDate = now
            };

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
            try
            {
                var id = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, param, transaction: transaction, cancellationToken: cancellationToken));
                await transaction.CommitAsync(cancellationToken);
                return new User
                {
                    Id = id,
                    Company = string.Empty,
                    PositionDescription = string.Empty,
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password,
                    Status = request.Status,
                    CreatedDate = now,
                    UpdatedDate = now
                };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on CreateAsync (User): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on CreateAsync (User): {ex.Message}", ex);
        }
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT u.id AS Id, c.name AS Company, p.description AS PositionDescription,
                   u.name AS Name, u.email AS Email, u.password AS Password, u.status AS Status,
                   u.created_date AS CreatedDate, u.updated_date AS UpdatedDate
            FROM users u
            INNER JOIN company c ON u.company_id = c.id
            INNER JOIN users_position p ON u.users_position_id = p.id
            WHERE u.id = @Id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            return await conn.QuerySingleOrDefaultAsync<User>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on GetByIdAsync (User): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on GetByIdAsync (User): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT u.id AS Id, c.name AS Company, p.description AS PositionDescription,
                   u.name AS Name, u.email AS Email, u.password AS Password, u.status AS Status,
                   u.created_date AS CreatedDate, u.updated_date AS UpdatedDate
            FROM users u
            INNER JOIN company c ON u.company_id = c.id
            INNER JOIN users_position p ON u.users_position_id = p.id
            ORDER BY u.id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<User>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on GetAllAsync (User): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on GetAllAsync (User): {ex.Message}", ex);
        }
    }

    public async Task<User> UpdateAsync(UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            UPDATE users
            SET company_id = @CompanyId, users_position_id = @UsersPositionId, name = @Name,
                email = @Email, password = @Password, status = @Status, updated_date = @UpdatedDate
            WHERE id = @Id";

            var updatedDate = DateTime.UtcNow;
            var param = new
            {
                request.Id,
                request.CompanyId,
                request.UsersPositionId,
                request.Name,
                request.Email,
                request.Password,
                request.Status,
                UpdatedDate = updatedDate
            };

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
            try
            {
                await conn.ExecuteAsync(new CommandDefinition(sql, param, transaction: transaction, cancellationToken: cancellationToken));
                await transaction.CommitAsync(cancellationToken);
                return new User
                {
                    Id = request.Id,
                    Company = string.Empty,
                    PositionDescription = string.Empty,
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password,
                    Status = request.Status,
                    CreatedDate = default,
                    UpdatedDate = updatedDate
                };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on UpdateAsync (User): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on UpdateAsync (User): {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = "DELETE FROM users WHERE id = @Id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
            try
            {
                var rows = await conn.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, transaction: transaction, cancellationToken: cancellationToken));
                await transaction.CommitAsync(cancellationToken);
                return rows > 0;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on DeleteAsync (User): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on DeleteAsync (User): {ex.Message}", ex);
        }
    }

    public async Task<User?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT u.id AS Id, c.name AS Company, p.description AS PositionDescription,
                   u.name AS Name, u.email AS Email, u.password AS Password, u.status AS Status,
                   u.created_date AS CreatedDate, u.updated_date AS UpdatedDate
            FROM users u
            INNER JOIN company c ON u.company_id = c.id
            INNER JOIN users_position p ON u.users_position_id = p.id
            WHERE u.email = @Email AND u.password = @Password AND u.status = 1";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            return await conn.QuerySingleOrDefaultAsync<User>(
                new CommandDefinition(sql, new { Email = email, Password = password }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on LoginAsync (User): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on LoginAsync (User): {ex.Message}", ex);
        }
    }
}
