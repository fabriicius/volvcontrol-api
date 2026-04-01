using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;
using volvcontrol_api.service.firebase;

namespace volvcontrol_api.service.services;

public class MaintenanceRecordService : IMaintenanceRecordService
{
    private readonly IMaintenanceRecordRepository _repository;
    private readonly IPhotoStorage _photoStorage;

    public MaintenanceRecordService(IMaintenanceRecordRepository repository, IPhotoStorage photoStorage)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _photoStorage = photoStorage ?? throw new ArgumentNullException(nameof(photoStorage));
    }

    public async Task<MaintenanceRecordDetailResponse> CreateAsync(MaintenanceRecordCreateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.EquipmentId, request.ClientId, request.CreatedByUserId, request.StatusMaintenanceRecordId, request.ServiceTypeMaintenanceRecordId);
        var photos = await UploadMaintenancePhotosAsync(request.ClientId, request.EquipmentId, request.BeforeImagesBase64, request.AfterImagesBase64, cancellationToken);
        var created = await _repository.CreateAsync(request, photos, cancellationToken);
        var detail = await _repository.GetByIdAsync(created.Id, cancellationToken);
        if (detail is null)
            throw new InvalidOperationException("Nao foi possivel carregar os detalhes da manutencao apos criar.");
        return MapDetail(detail);
    }

    public async Task<MaintenanceRecordDetailResponse?> UpdateAsync(MaintenanceRecordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Id <= 0)
            throw new ArgumentException("id deve ser maior que zero.");
        ValidateRequest(request.EquipmentId, request.ClientId, request.CreatedByUserId, request.StatusMaintenanceRecordId, request.ServiceTypeMaintenanceRecordId);
        var photos = await UploadMaintenancePhotosAsync(request.ClientId, request.EquipmentId, request.BeforeImagesBase64, request.AfterImagesBase64, cancellationToken);
        var updated = await _repository.UpdateAsync(request, photos, cancellationToken);
        if (updated is null)
            return null;
        var detail = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return detail is null ? null : MapDetail(detail);
    }

    public async Task<MaintenanceRecordDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var detail = await _repository.GetByIdAsync(id, cancellationToken);
        return detail is null ? null : MapDetail(detail);
    }

    public async Task<IReadOnlyList<MaintenanceRecordInfoResponse>> GetInfoByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("userId deve ser maior que zero.");

        var list = await _repository.GetInfoByUserIdAsync(userId, cancellationToken);
        return list.Select(i => new MaintenanceRecordInfoResponse
        {
            Id = i.Id,
            EquipmentName = i.EquipmentName,
            ServicesPerformed = i.ServicesPerformed,
            ClientName = i.ClientName,
            ServiceTypeMaintenanceRecordDescription = i.ServiceTypeMaintenanceRecordDescription,
            StatusMaintenanceRecordDescription = i.StatusMaintenanceRecordDescription,
            ServiceDate = i.ServiceDate,
            TechnicianName = i.TechnicianName
        }).ToList();
    }

    public async Task<IReadOnlyList<MaintenanceRecordListResponse>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("userId deve ser maior que zero.");

        var list = await _repository.GetAllByUserIdAsync(userId, cancellationToken);
        return list.Select(i => new MaintenanceRecordListResponse
        {
            Id = i.Id,
            ClientId = i.ClientId,
            ClientName = i.ClientName,
            ClientDocument = i.ClientDocument,
            EquipmentId = i.EquipmentId,
            EquipmentName = i.EquipmentName,
            EquipmentQrCode = i.EquipmentQrCode,
            StatusMaintenanceRecordId = i.StatusMaintenanceRecordId,
            StatusMaintenanceRecordDescription = i.StatusMaintenanceRecordDescription,
            ServiceTypeMaintenanceRecordId = i.ServiceTypeMaintenanceRecordId,
            ServiceTypeMaintenanceRecordDescription = i.ServiceTypeMaintenanceRecordDescription,
            ServiceDate = i.ServiceDate,
            CreatedDate = i.CreatedDate
        }).ToList();
    }

    public async Task<IReadOnlyList<MaintenanceRecordListResponse>> GetAllByServiceRequestIdAsync(int serviceRequestId, CancellationToken cancellationToken = default)
    {
        if (serviceRequestId <= 0)
            throw new ArgumentException("serviceRequestId deve ser maior que zero.");

        var list = await _repository.GetAllByServiceRequestIdAsync(serviceRequestId, cancellationToken);
        return list.Select(i => new MaintenanceRecordListResponse
        {
            Id = i.Id,
            ClientId = i.ClientId,
            ClientName = i.ClientName,
            ClientDocument = i.ClientDocument,
            EquipmentId = i.EquipmentId,
            EquipmentName = i.EquipmentName,
            EquipmentQrCode = i.EquipmentQrCode,
            StatusMaintenanceRecordId = i.StatusMaintenanceRecordId,
            StatusMaintenanceRecordDescription = i.StatusMaintenanceRecordDescription,
            ServiceTypeMaintenanceRecordId = i.ServiceTypeMaintenanceRecordId,
            ServiceTypeMaintenanceRecordDescription = i.ServiceTypeMaintenanceRecordDescription,
            ServiceDate = i.ServiceDate,
            CreatedDate = i.CreatedDate
        }).ToList();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("id deve ser maior que zero.");
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceTypeMaintenanceRecordResponse>> GetServiceTypesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetServiceTypesAsync(cancellationToken);
        return list.Select(i => new ServiceTypeMaintenanceRecordResponse
        {
            Id = i.Id,
            Description = i.Description
        }).ToList();
    }

    public async Task<IReadOnlyList<StatusMaintenanceRecordResponse>> GetStatusesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetStatusesAsync(cancellationToken);
        return list.Select(i => new StatusMaintenanceRecordResponse
        {
            Id = i.Id,
            Description = i.Description
        }).ToList();
    }

    private static void ValidateRequest(int equipmentId, int clientId, int createdByUserId, int statusMaintenanceRecordId, int serviceTypeMaintenanceRecordId)
    {
        if (equipmentId <= 0 || clientId <= 0 || createdByUserId <= 0 || statusMaintenanceRecordId <= 0 || serviceTypeMaintenanceRecordId <= 0)
            throw new ArgumentException("equipmentId, clientId, createdByUserId, statusMaintenanceRecordId e serviceTypeMaintenanceRecordId devem ser maiores que zero.");
    }

    private static MaintenanceRecordDetailResponse MapDetail(MaintenanceRecordDetail detail)
    {
        return new MaintenanceRecordDetailResponse
        {
            Id = detail.Id,
            EquipmentId = detail.EquipmentId,
            ClientId = detail.ClientId,
            ServiceDate = detail.ServiceDate,
            ServiceRequestId = detail.ServiceRequestId,
            StatusMaintenanceRecordId = detail.StatusMaintenanceRecordId,
            ServiceTypeMaintenanceRecordId = detail.ServiceTypeMaintenanceRecordId,
            CreatedByUserId = detail.CreatedByUserId,
            StartTime = detail.StartTime,
            EndTime = detail.EndTime,
            SymptomsReported = detail.SymptomsReported,
            Diagnosis = detail.Diagnosis,
            ServicesPerformed = detail.ServicesPerformed,
            ClientSignature = detail.ClientSignature,
            Observations = detail.Observations,
            NextMaintenanceDate = detail.NextMaintenanceDate,
            CreatedDate = detail.CreatedDate,
            UpdatedDate = detail.UpdatedDate,
            ClientName = detail.ClientName,
            ClientDocument = detail.ClientDocument,
            EquipmentName = detail.EquipmentName,
            EquipmentQrCode = detail.EquipmentQrCode,
            StatusMaintenanceRecordDescription = detail.StatusMaintenanceRecordDescription,
            ServiceTypeMaintenanceRecordDescription = detail.ServiceTypeMaintenanceRecordDescription,
            CreatedByUserName = detail.CreatedByUserName,
            CreatedByUserEmail = detail.CreatedByUserEmail,
            ServiceRequestNumber = detail.ServiceRequestNumber,
            Tools = detail.Tools.Select(t => new ServiceMaintenanceToolResponse
            {
                Id = t.Id,
                Description = t.Description
            }).ToList(),
            Photos = detail.Photos.Select(p => new MaintenancePhotoResponse
            {
                Id = p.Id,
                Url = p.Url,
                Description = p.Description,
                PhotoMoment = p.PhotoMoment
            }).ToList()
        };
    }

    private async Task<IReadOnlyList<MaintenancePhotoCreate>> UploadMaintenancePhotosAsync(
        int clientId,
        int equipmentId,
        IEnumerable<string>? beforeImagesBase64,
        IEnumerable<string>? afterImagesBase64,
        CancellationToken cancellationToken)
    {
        var photos = new List<MaintenancePhotoCreate>();
        photos.AddRange(await UploadPhotosByMomentAsync("before", beforeImagesBase64, clientId, equipmentId, cancellationToken));
        photos.AddRange(await UploadPhotosByMomentAsync("after", afterImagesBase64, clientId, equipmentId, cancellationToken));
        return photos;
    }

    private async Task<IReadOnlyList<MaintenancePhotoCreate>> UploadPhotosByMomentAsync(
        string moment,
        IEnumerable<string>? imagesBase64,
        int clientId,
        int equipmentId,
        CancellationToken cancellationToken)
    {
        var list = new List<MaintenancePhotoCreate>();
        if (imagesBase64 is null)
            return list;

        foreach (var imageBase64 in imagesBase64)
        {
            if (string.IsNullOrWhiteSpace(imageBase64))
                continue;

            var (bytes, contentType, extension) = ParseBase64Image(imageBase64);
            await using var content = new MemoryStream(bytes);
            var fileName = $"maintenance-{moment}-{Guid.NewGuid():N}{extension}";
            var objectName = await _photoStorage.UploadAsync(
                content,
                fileName,
                contentType,
                cancellationToken,
                "VolvControl",
                "Manutencao",
                clientId.ToString(),
                equipmentId.ToString());

            list.Add(new MaintenancePhotoCreate
            {
                Url = _photoStorage.GetPermanentUrl(objectName),
                Description = moment == "before" ? "Antes da manutencao" : "Depois da manutencao",
                PhotoMoment = moment
            });
        }

        return list;
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
