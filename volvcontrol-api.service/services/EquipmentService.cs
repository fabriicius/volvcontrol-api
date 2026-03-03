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
        var photoUrl = await TryUploadBase64ImageAsync(request, cancellationToken);
        var entity = await _repository.CreateAsync(request, photoUrl, cancellationToken);
        return ToResponse(entity);
    }

    public async Task<EquipmentResponse?> UpdateAsync(EquipmentUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.UpdateAsync(request, cancellationToken);
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

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
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
            PhotoUrl = entity.PhotoUrl,
            ImageBase64 = string.Empty,
            Notes = entity.Notes,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        };
    }

    private async Task<string> TryUploadBase64ImageAsync(EquipmentCreateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
            return string.Empty;

        var (bytes, contentType, extension) = ParseBase64Image(request.ImageBase64);
        await using var content = new MemoryStream(bytes);
        var fileName = $"equipment-{Guid.NewGuid():N}{extension}";

        var objectName = await _photoStorage.UploadAsync(
            content,
            fileName,
            contentType,
            cancellationToken,
            "VolvControl",
            "Equipamento",
            request.ClientId.ToString());

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
