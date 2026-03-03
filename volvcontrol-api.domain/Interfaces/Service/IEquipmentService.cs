using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.domain.Interfaces.Service;

public interface IEquipmentService
{
    Task<EquipmentResponse> CreateAsync(EquipmentCreateRequest request, CancellationToken cancellationToken = default);
    Task<EquipmentResponse?> UpdateAsync(EquipmentUpdateRequest request, CancellationToken cancellationToken = default);
    Task<EquipmentResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
