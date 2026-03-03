using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Request;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface IEquipmentRepository
{
    Task<Equipment> CreateAsync(EquipmentCreateRequest request, string photoUrl, CancellationToken cancellationToken = default);
    Task<Equipment?> UpdateAsync(EquipmentUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
