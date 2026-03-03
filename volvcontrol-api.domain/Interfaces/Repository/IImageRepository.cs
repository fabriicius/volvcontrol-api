using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Request;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface IImageRepository
{
    Task<Image> CreateAsync(ImageCreateRequest request, CancellationToken cancellationToken = default);
    Task<Image?> UpdateAsync(ImageUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Image?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
