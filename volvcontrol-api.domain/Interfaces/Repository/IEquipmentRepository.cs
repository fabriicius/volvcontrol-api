using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Request;

namespace volvcontrol_api.domain.Interfaces.Repository;

public interface IEquipmentRepository
{
    Task<Equipment> CreateAsync(EquipmentCreateRequest request, string photoUrl, CancellationToken cancellationToken = default);
    Task<Equipment?> UpdateAsync(EquipmentUpdateRequest request, string photoUrl, CancellationToken cancellationToken = default);
    Task<bool> UpdateQrCodeAsync(int equipmentId, string qrCodeUrl, CancellationToken cancellationToken = default);
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Equipment>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentServiceRequestItem>> GetServiceRequestsByEquipmentIdsAsync(IReadOnlyCollection<int> equipmentIds, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EquipmentType>> GetTypesAsync(CancellationToken cancellationToken = default);
}
