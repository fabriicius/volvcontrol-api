using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Request;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface IClientRepository
{
    Task<Client> CreateAsync(ClientCreateRequest request, CancellationToken cancellationToken = default);
    Task<Client?> UpdateAsync(ClientUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Client?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
    Task<Client?> GetAnotherClientByEmailAsync(string email, int excludeClientId, CancellationToken cancellationToken = default);
    Task<Client?> GetAnotherClientByDocumentAsync(string document, int excludeClientId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ClientWithDetails?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ClientListItem> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Address> CreateAddressForClientAsync(AddressCreateRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Plan>> GetPlansAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusClient>> GetStatusClientsAsync(CancellationToken cancellationToken = default);
}
