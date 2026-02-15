using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Map;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.service.services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        var created = await _userRepository.CreateAsync(request, cancellationToken);
        return UserMap.ToResponse(created);
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _userRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : UserMap.ToResponse(entity);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _userRepository.GetAllAsync(cancellationToken);
        return entities.Select(UserMap.ToResponse).ToList();
    }

    public async Task<UserResponse> UpdateAsync(UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var updated = await _userRepository.UpdateAsync(request, cancellationToken);
        return UserMap.ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<UserResponse?> LoginAsync(LoginUserPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _userRepository.LoginAsync(request.Email, request.Password, cancellationToken);
        return entity is null ? null : UserMap.ToResponse(entity);
    }
}
