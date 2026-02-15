using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Request;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface IUserRepository
{
    Task<User> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(UserUpdateRequest request, CancellationToken cancellationToken = default);
    Task UpdateEmailAsync(string oldEmail, string newEmail, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
