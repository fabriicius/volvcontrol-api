using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Exceptions;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.service.services;

public class ClientService : IClientService
{
    private const string DefaultClientPassword = "mudar123";

    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;

    public ClientService(IClientRepository clientRepository, IUserRepository userRepository)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<ClientResponse> CreateAsync(ClientCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0)
            throw new ArgumentException("userId é obrigatório e deve ser maior que zero.");

        if (await _clientRepository.GetByEmailAsync(request.Email, cancellationToken) != null)
            throw new ConflictException("Email já cadastrado para um cliente.");

        if (await _clientRepository.GetByDocumentAsync(request.Document, cancellationToken) != null)
            throw new ConflictException("Documento já cadastrado.");

        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) != null)
            throw new ConflictException("Email já cadastrado para um usuário.");

        var entity = await _clientRepository.CreateAsync(request, cancellationToken);

        try
        {
            var userRequest = new UserCreateRequest
            {
                CompanyId = 1,
                UsersPositionId = 2,
                Name = request.Name,
                Email = request.Email,
                Password = DefaultClientPassword
            };
            await _userRepository.CreateAsync(userRequest, cancellationToken);
        }
        catch (Exception)
        {
            await _clientRepository.DeleteAsync(entity.Id, cancellationToken);
            throw new ConflictException("Erro ao cadastrar o cliente. O email já está em uso e o cadastro foi cancelado.");
        }

        return ToResponse(entity);
    }

    public async Task<ClientResponse?> UpdateAsync(ClientUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var current = await _clientRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (current is null)
            return null;

        if (await _clientRepository.GetAnotherClientByEmailAsync(request.Email, request.Id, cancellationToken) != null)
            throw new ConflictException("Email já cadastrado para um cliente.");

        if (await _clientRepository.GetAnotherClientByDocumentAsync(request.Document, request.Id, cancellationToken) != null)
            throw new ConflictException("Documento já cadastrado.");

        if (!string.Equals(current.Email, request.Email, StringComparison.OrdinalIgnoreCase) && await _userRepository.GetByEmailAsync(request.Email, cancellationToken) != null)
            throw new ConflictException("Email já cadastrado para um usuário.");

        var entity = await _clientRepository.UpdateAsync(request, cancellationToken);
        if (entity is null)
            return null;

        if (!string.Equals(current.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            await _userRepository.UpdateEmailAsync(current.Email, request.Email, cancellationToken);

        return ToResponse(entity);
    }

    public async Task<ClientDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _clientRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return entity is null ? null : ToDetailResponse(entity);
    }

    public async Task<PagedResponse<ClientListResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _clientRepository.GetPagedAsync(page, pageSize, cancellationToken);
        var list = items.Select(i => new ClientListResponse
        {
            Id = i.Id,
            Name = i.Name,
            Document = i.Document,
            StatusDescription = i.StatusDescription ?? string.Empty,
            PlanDescription = i.PlanDescription ?? string.Empty
        }).ToList();
        return new PagedResponse<ClientListResponse>
        {
            Items = list,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static ClientDetailResponse ToDetailResponse(ClientWithDetails entity)
    {
        return new ClientDetailResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            PlansId = entity.PlansId,
            PlanDescription = entity.PlanDescription ?? string.Empty,
            StatusClientId = entity.StatusClientId,
            StatusClientDescription = entity.StatusClientDescription ?? string.Empty,
            Name = entity.Name,
            Document = entity.Document,
            Email = entity.Email,
            Phone = entity.Phone,
            Notes = entity.Notes,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate,
            EquipmentCount = entity.EquipmentCount,
            Equipments = entity.Equipments.Select(e => new ClientEquipmentResponse
            {
                Name = e.Name,
                QrCode = e.QrCode,
                Status = e.Status
            }).ToList(),
            Addresses = entity.Addresses.Select(a => new AddressResponse
            {
                Id = a.Id,
                Street = a.Street,
                Neighborhood = a.Neighborhood,
                Number = a.Number,
                ZipCode = a.ZipCode,
                Sigla = a.Sigla,
                City = a.City,
                Complement = a.Complement,
                CreatedBy = a.CreatedBy,
                CreatedDate = a.CreatedDate,
                UpdatedDate = a.UpdatedDate
            }).ToList()
        };
    }

    private static ClientResponse ToResponse(Client entity)
    {
        return new ClientResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            PlansId = entity.PlansId,
            StatusClientId = entity.StatusClientId,
            Name = entity.Name,
            Document = entity.Document,
            Email = entity.Email,
            Phone = entity.Phone,
            Notes = entity.Notes,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        };
    }

    public async Task<IReadOnlyList<PlanResponse>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _clientRepository.GetPlansAsync(cancellationToken);
        return entities.Select(p => new PlanResponse { Id = p.Id, Description = p.Description }).ToList();
    }

    public async Task<IReadOnlyList<StatusClientResponse>> GetStatusClientsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _clientRepository.GetStatusClientsAsync(cancellationToken);
        return entities.Select(s => new StatusClientResponse { Id = s.Id, Description = s.Description }).ToList();
    }
}
