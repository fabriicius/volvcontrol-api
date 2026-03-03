using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.domain.Interfaces.Service;

public interface IImageService
{
    Task<ImageResponse> CreateAsync(ImageCreateRequest request, CancellationToken cancellationToken = default);
    Task<ImageResponse?> UpdateAsync(ImageUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ImageResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
