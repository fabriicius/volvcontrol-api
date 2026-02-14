using volvcontrol_api.domain.Model;

namespace volvcontrol_api.domain.Interfaces.Service;

public interface IUserService
{
    Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateAsync(UserUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<UserResponse?> LoginAsync(LoginUserPasswordRequest request, CancellationToken cancellationToken = default);
}
