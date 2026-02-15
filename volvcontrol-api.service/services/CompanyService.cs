using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Map;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.service.services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
    }

    public async Task<CompanyResponse> CreateAsync(CompanyCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = CompanyMap.ToEntity(request);
        var created = await _companyRepository.CreateAsync(entity, cancellationToken);
        return CompanyMap.ToResponse(created);
    }

    public async Task<CompanyResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _companyRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : CompanyMap.ToResponse(entity);
    }

    public async Task<IReadOnlyList<CompanyResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _companyRepository.GetAllAsync(cancellationToken);
        return entities.Select(CompanyMap.ToResponse).ToList();
    }

    public async Task<CompanyResponse> UpdateAsync(CompanyUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = CompanyMap.ToEntity(request);
        var updated = await _companyRepository.UpdateAsync(entity, cancellationToken);
        return CompanyMap.ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _companyRepository.DeleteAsync(id, cancellationToken);
    }
}
