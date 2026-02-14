using Dapper;
using MySqlConnector;
using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;

namespace volvcontrol_api.repository.repo;

public class CompanyRepository : ICompanyRepository
{
    private readonly string _connectionString;

    public CompanyRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Company> CreateAsync(Company company, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            INSERT INTO company (name, document, created_date, updated_date)
            VALUES (@Name, @Document, @CreatedDate, @UpdatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var now = DateTime.UtcNow;
            var param = new
            {
                company.Name,
                company.Document,
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
                return new Company
                {
                    Id = id,
                    Name = company.Name,
                    Document = company.Document,
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
            throw new InvalidOperationException($"Database error on CreateAsync (Company): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on CreateAsync (Company): {ex.Message}", ex);
        }
    }

    public async Task<Company?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT id AS Id, name AS Name, document AS Document,
                   created_date AS CreatedDate, updated_date AS UpdatedDate
            FROM company
            WHERE id = @Id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            return await conn.QuerySingleOrDefaultAsync<Company>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on GetByIdAsync (Company): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on GetByIdAsync (Company): {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            SELECT id AS Id, name AS Name, document AS Document,
                   created_date AS CreatedDate, updated_date AS UpdatedDate
            FROM company
            ORDER BY id";

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            var list = await conn.QueryAsync<Company>(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return list.ToList();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on GetAllAsync (Company): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on GetAllAsync (Company): {ex.Message}", ex);
        }
    }

    public async Task<Company> UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = @"
            UPDATE company
            SET name = @Name, document = @Document, updated_date = @UpdatedDate
            WHERE id = @Id";

            var updatedDate = DateTime.UtcNow;
            var param = new
            {
                company.Id,
                company.Name,
                company.Document,
                UpdatedDate = updatedDate
            };

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
            try
            {
                await conn.ExecuteAsync(new CommandDefinition(sql, param, transaction: transaction, cancellationToken: cancellationToken));
                await transaction.CommitAsync(cancellationToken);
                return new Company
                {
                    Id = company.Id,
                    Name = company.Name,
                    Document = company.Document,
                    CreatedDate = company.CreatedDate,
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
            throw new InvalidOperationException($"Database error on UpdateAsync (Company): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on UpdateAsync (Company): {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = "DELETE FROM company WHERE id = @Id";

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
            throw new InvalidOperationException($"Database error on DeleteAsync (Company): {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error on DeleteAsync (Company): {ex.Message}", ex);
        }
    }
}
