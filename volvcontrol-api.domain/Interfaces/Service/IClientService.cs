using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.domain.Interfaces.Service;

public interface IClientService
{
    Task<ClientResponse> CreateAsync(ClientCreateRequest request, CancellationToken cancellationToken = default);
    Task<ClientResponse?> UpdateAsync(ClientUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ClientDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ClientListResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanResponse>> GetPlansAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusClientResponse>> GetStatusClientsAsync(CancellationToken cancellationToken = default);
}
