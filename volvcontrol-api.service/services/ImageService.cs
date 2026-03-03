using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.service.services;

public class ImageService : IImageService
{
    private readonly IImageRepository _repository;

    public ImageService(IImageRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ImageResponse> CreateAsync(ImageCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.CreateAsync(request, cancellationToken);
        return ToResponse(entity);
    }

    public async Task<ImageResponse?> UpdateAsync(ImageUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.UpdateAsync(request, cancellationToken);
        return entity is null ? null : ToResponse(entity);
    }

    public async Task<ImageResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : ToResponse(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static ImageResponse ToResponse(Image entity)
    {
        return new ImageResponse
        {
            Id = entity.Id,
            Url = entity.Url,
            Description = entity.Description,
            CreatedDate = entity.CreatedDate
        };
    }
}
