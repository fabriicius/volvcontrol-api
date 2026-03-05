using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;
using volvcontrol_api.service.firebase;

namespace volvcontrol_api.service.services;

public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _repository;
    private readonly IPhotoStorage _photoStorage;

    public EquipmentService(IEquipmentRepository repository, IPhotoStorage photoStorage)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _photoStorage = photoStorage ?? throw new ArgumentNullException(nameof(photoStorage));
    }

    public async Task<EquipmentResponse> CreateAsync(EquipmentCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EquipmentCategoryId <= 0 || request.EquipmentTypeId <= 0)
            throw new ArgumentException("equipmentCategoryId e equipmentTypeId são obrigatórios e devem ser maiores que zero.");

        var photoUrl = await TryUploadBase64ImageAsync(request.ImageBase64, request.ClientId, cancellationToken);
        var entity = await _repository.CreateAsync(request, photoUrl, cancellationToken);
        return ToResponse(entity);
    }

    public async Task<EquipmentResponse?> UpdateAsync(EquipmentUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EquipmentCategoryId <= 0 || request.EquipmentTypeId <= 0)
            throw new ArgumentException("equipmentCategoryId e equipmentTypeId são obrigatórios e devem ser maiores que zero.");
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
            throw new ArgumentException("imagebase64 é obrigatório no update do equipamento.");

        var current = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (current is null)
            return null;

        var photoUrl = await TryUploadBase64ImageAsync(request.ImageBase64, request.ClientId, cancellationToken);

        var oldObjectName = ExtractObjectNameFromPhotoUrl(current.PhotoUrl);
        if (!string.IsNullOrWhiteSpace(oldObjectName))
            await _photoStorage.DeleteAsync(oldObjectName, cancellationToken);

        var entity = await _repository.UpdateAsync(request, photoUrl, cancellationToken);
        if (entity is null)
            return null;
        var updated = await _repository.GetByIdAsync(entity.Id, cancellationToken);
        return updated is null ? null : ToResponse(updated);
    }

    public async Task<EquipmentResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        var response = ToResponse(entity);
        response.ImageBase64 = await TryGetImageBase64Async(entity.PhotoUrl, cancellationToken);
        return response;
    }

    public async Task<IReadOnlyList<EquipmentResponse>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("userId deve ser maior que zero.");

        var entities = await _repository.GetAllByUserIdAsync(userId, cancellationToken);
        return entities.Select(ToResponse).ToList();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<EquipmentCategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetCategoriesAsync(cancellationToken);
        return entities.Select(c => new EquipmentCategoryResponse
        {
            Id = c.Id,
            Description = c.Description
        }).ToList();
    }

    public async Task<IReadOnlyList<EquipmentTypeResponse>> GetTypesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetTypesAsync(cancellationToken);
        return entities.Select(t => new EquipmentTypeResponse
        {
            Id = t.Id,
            Description = t.Description
        }).ToList();
    }

    private static EquipmentResponse ToResponse(Equipment entity)
    {
        return new EquipmentResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            ClientId = entity.ClientId,
            QrCode = entity.QrCode,
            EquipmentCategoryId = entity.EquipmentCategoryId,
            EquipmentTypeId = entity.EquipmentTypeId,
            CategoryDescription = entity.CategoryDescription ?? string.Empty,
            TypeDescription = entity.TypeDescription ?? string.Empty,
            Brand = entity.Brand,
            Model = entity.Model,
            InstallationDate = entity.InstallationDate,
            WarrantyUntil = entity.WarrantyUntil,
            StatusEquipmentId = entity.StatusEquipmentId,
            StatusEquipmentDescription = entity.StatusEquipmentDescription ?? string.Empty,
            PhotoUrl = entity.PhotoUrl,
            ImageBase64 = string.Empty,
            Notes = entity.Notes,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        };
    }

    private async Task<string> TryUploadBase64ImageAsync(string imageBase64, int clientId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imageBase64))
            return string.Empty;

        var (bytes, contentType, extension) = ParseBase64Image(imageBase64);
        await using var content = new MemoryStream(bytes);
        var fileName = $"equipment-{Guid.NewGuid():N}{extension}";

        var objectName = await _photoStorage.UploadAsync(
            content,
            fileName,
            contentType,
            cancellationToken,
            "VolvControl",
            "Equipamento",
            clientId.ToString());

        return _photoStorage.GetPermanentUrl(objectName);
    }

    private async Task<string> TryGetImageBase64Async(string photoUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
            return string.Empty;

        try
        {
            var objectName = ExtractObjectNameFromPhotoUrl(photoUrl);
            if (string.IsNullOrWhiteSpace(objectName))
                return string.Empty;

            var bytes = await _photoStorage.DownloadAsync(objectName, cancellationToken);
            return Convert.ToBase64String(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractObjectNameFromPhotoUrl(string photoUrl)
    {
        if (!Uri.TryCreate(photoUrl, UriKind.Absolute, out var uri))
            return photoUrl.TrimStart('/');

        if (uri.Host.Contains("firebasestorage.googleapis.com", StringComparison.OrdinalIgnoreCase))
        {
            var marker = "/o/";
            var absolutePath = uri.AbsolutePath;
            var idx = absolutePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var encoded = absolutePath[(idx + marker.Length)..];
                return Uri.UnescapeDataString(encoded).TrimStart('/');
            }
        }

        if (uri.Host.Contains("storage.googleapis.com", StringComparison.OrdinalIgnoreCase))
        {
            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
                return string.Join("/", segments.Skip(1));
        }

        return string.Empty;
    }

    private static (byte[] Bytes, string ContentType, string Extension) ParseBase64Image(string value)
    {
        var trimmed = value.Trim();
        var contentType = "application/octet-stream";
        var extension = ".bin";
        var base64 = trimmed;

        if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var commaIndex = trimmed.IndexOf(',');
            if (commaIndex > 5)
            {
                var meta = trimmed[5..commaIndex];
                base64 = trimmed[(commaIndex + 1)..];
                var parts = meta.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0 && parts[0].Contains('/'))
                    contentType = parts[0];
            }
        }

        extension = contentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => ".bin"
        };

        var bytes = Convert.FromBase64String(base64);
        return (bytes, contentType, extension);
    }

}
