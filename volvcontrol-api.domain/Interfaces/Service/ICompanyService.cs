using volvcontrol_api.domain.Model;

namespace volvcontrol_api.domain.Interfaces.Service;

public interface ICompanyService
{
    Task<CompanyResponse> CreateAsync(CompanyCreateRequest request, CancellationToken cancellationToken = default);
    Task<CompanyResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CompanyResponse> UpdateAsync(CompanyUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
