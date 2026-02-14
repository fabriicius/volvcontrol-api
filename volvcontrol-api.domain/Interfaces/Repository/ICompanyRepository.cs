using volvcontrol_api.domain.Entities;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface ICompanyRepository
{
    Task<Company> CreateAsync(Company company, CancellationToken cancellationToken = default);
    Task<Company?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Company> UpdateAsync(Company company, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
