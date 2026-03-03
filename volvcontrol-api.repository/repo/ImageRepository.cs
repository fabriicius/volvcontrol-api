using Dapper;
using MySqlConnector;
using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Model.Request;

namespace volvcontrol_api.repository.repo;

public class ImageRepository : IImageRepository
{
    private readonly string _connectionString;

    public ImageRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Image> CreateAsync(ImageCreateRequest request, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        try
        {
            const string sql = @"
            INSERT INTO image (url, description, created_date)
            VALUES (@Url, @Description, @CreatedDate);
            SELECT CAST(LAST_INSERT_ID() AS SIGNED);";

            var now = DateTime.UtcNow;
            var param = new
            {
                request.Url,
                request.Description,
                CreatedDate = now
            };

            var id = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));

            return new Image
            {
                Id = id,
                Url = request.Url,
                Description = request.Description,
                CreatedDate = now
            };
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on CreateAsync (Image): {ex.Message}", ex);
        }
    }

    public async Task<Image?> UpdateAsync(ImageUpdateRequest request, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        try
        {
            const string sql = @"
            UPDATE image
            SET url = @Url, description = @Description
            WHERE id = @Id";

            var param = new
            {
                request.Id,
                request.Url,
                request.Description
            };

            var rows = await conn.ExecuteAsync(new CommandDefinition(sql, param, cancellationToken: cancellationToken));
            if (rows == 0)
                return null;

            var existing = await GetByIdAsync(request.Id, cancellationToken);
            return existing;
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on UpdateAsync (Image): {ex.Message}", ex);
        }
    }

    public async Task<Image?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string sql = @"
            SELECT id AS Id, url AS Url, description AS Description, created_date AS CreatedDate
            FROM image
            WHERE id = @Id";

            return await conn.QuerySingleOrDefaultAsync<Image>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException($"Database error on GetByIdAsync (Image): {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        var rows = await conn.ExecuteAsync(new CommandDefinition("DELETE FROM image WHERE id = @Id", new { Id = id }, cancellationToken: cancellationToken));
        return rows > 0;
    }
}
