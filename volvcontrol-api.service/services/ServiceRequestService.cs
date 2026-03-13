using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;
using System.Security.Cryptography;

namespace volvcontrol_api.service.services;

public class ServiceRequestService : IServiceRequestService
{
    private const int MaxRequestNumberAttempts = 1500;
    private readonly IServiceRequestRepository _repository;
    private readonly IUserRepository _userRepository;

    public ServiceRequestService(IServiceRequestRepository repository, IUserRepository userRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<ServiceRequestResponse> CreateAsync(ServiceRequestCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ClientId <= 0 || request.EquipmentId <= 0 || request.UserId <= 0 || request.TypeMaintenanceRecordId <= 0 || request.StatusServiceRequestId <= 0)
            throw new ArgumentException("clientId, equipmentId, userId, typeMaintenanceRecordId e statusServiceRequestId são obrigatórios e devem ser maiores que zero.");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("Usuario informado nao foi encontrado para preencher o tecnico responsavel.");

        request.AssignedTechnician = user.Email;
        request.RequestNumber = await GenerateUniqueRequestNumberAsync(cancellationToken);
        var entity = await _repository.CreateAsync(request, cancellationToken);
        return ToResponse(entity);
    }

    public async Task<ServiceRequestDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var detail = await _repository.GetByIdAsync(id, cancellationToken);
        if (detail is null)
            return null;

        return new ServiceRequestDetailResponse
        {
            Id = detail.Id,
            ClientId = detail.ClientId,
            EquipmentId = detail.EquipmentId,
            UserId = detail.UserId,
            StatusServiceRequestId = detail.StatusServiceRequestId,
            RequestNumber = detail.RequestNumber,
            TypeMaintenanceRecordId = detail.TypeMaintenanceRecordId,
            CreatedBy = detail.CreatedBy,
            Description = detail.Description,
            ScheduledDate = detail.ScheduledDate,
            ScheduledTime = detail.ScheduledTime,
            AssignedTechnician = detail.AssignedTechnician,
            CompletionDate = detail.CompletionDate,
            Notes = detail.Notes,
            CreatedDate = detail.CreatedDate,
            UpdatedDate = detail.UpdatedDate,
            ClientName = detail.ClientName,
            ClientDocument = detail.ClientDocument,
            StatusServiceRequestDescription = detail.StatusServiceRequestDescription,
            TypeMaintenanceRecordDescription = detail.TypeMaintenanceRecordDescription,
            EquipmentName = detail.EquipmentName,
            EquipmentQrCode = detail.EquipmentQrCode,
            UserName = detail.UserName,
            UserEmail = detail.UserEmail
        };
    }

    public async Task<IReadOnlyList<ServiceRequestListResponse>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("userId deve ser maior que zero.");

        var items = await _repository.GetAllByUserIdAsync(userId, cancellationToken);
        return items.Select(i => new ServiceRequestListResponse
        {
            Id = i.Id,
            RequestNumber = i.RequestNumber,
            ClientId = i.ClientId,
            ClientName = i.ClientName,
            ClientDocument = i.ClientDocument,
            StatusServiceRequestId = i.StatusServiceRequestId,
            StatusServiceRequestDescription = i.StatusServiceRequestDescription,
            TypeMaintenanceRecordId = i.TypeMaintenanceRecordId,
            TypeMaintenanceRecordDescription = i.TypeMaintenanceRecordDescription
        }).ToList();
    }

    public async Task<IReadOnlyList<TypeMaintenanceRecordResponse>> GetTypeMaintenanceRecordsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetTypeMaintenanceRecordsAsync(cancellationToken);
        return entities.Select(t => new TypeMaintenanceRecordResponse
        {
            Id = t.Id,
            Description = t.Description
        }).ToList();
    }

    public async Task<IReadOnlyList<StatusServiceRequestResponse>> GetStatusServiceRequestsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetStatusServiceRequestsAsync(cancellationToken);
        return entities.Select(t => new StatusServiceRequestResponse
        {
            Id = t.Id,
            Description = t.Description
        }).ToList();
    }

    private static ServiceRequestResponse ToResponse(ServiceRequest entity)
    {
        return new ServiceRequestResponse
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            EquipmentId = entity.EquipmentId,
            UserId = entity.UserId,
            StatusServiceRequestId = entity.StatusServiceRequestId,
            RequestNumber = entity.RequestNumber,
            TypeMaintenanceRecordId = entity.TypeMaintenanceRecordId,
            CreatedBy = entity.CreatedBy,
            Description = entity.Description,
            ScheduledDate = entity.ScheduledDate,
            ScheduledTime = entity.ScheduledTime,
            AssignedTechnician = entity.AssignedTechnician,
            CompletionDate = entity.CompletionDate,
            Notes = entity.Notes,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        };
    }

    private async Task<string> GenerateUniqueRequestNumberAsync(CancellationToken cancellationToken)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

        for (var attempt = 0; attempt < MaxRequestNumberAttempts; attempt++)
        {
            var randomPart = RandomNumberGenerator.GetInt32(0, 1000).ToString("D3");
            var requestNumber = $"OS-{datePart}-{randomPart}";
            var alreadyExists = await _repository.ExistsByRequestNumberAsync(requestNumber, cancellationToken);
            if (!alreadyExists)
                return requestNumber;
        }

        throw new InvalidOperationException("Nao foi possivel gerar uma ordem de servico unica. Tente novamente.");
    }
}
