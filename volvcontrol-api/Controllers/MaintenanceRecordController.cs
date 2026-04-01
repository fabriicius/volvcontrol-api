using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using volvcontrol_api.Authorization;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceRecordController : ControllerBase
{
    private readonly IMaintenanceRecordService _maintenanceRecordService;
    private readonly IServiceRequestService _serviceRequestService;

    public MaintenanceRecordController(IMaintenanceRecordService maintenanceRecordService, IServiceRequestService serviceRequestService)
    {
        _maintenanceRecordService = maintenanceRecordService ?? throw new ArgumentNullException(nameof(maintenanceRecordService));
        _serviceRequestService = serviceRequestService ?? throw new ArgumentNullException(nameof(serviceRequestService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(MaintenanceRecordDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MaintenanceRecordDetailResponse>> Create([FromBody] MaintenanceRecordCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _maintenanceRecordService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("lockup/{serviceRequestId:int}")]
    [ProducesResponseType(typeof(MaintenanceRecordServiceRequestContextResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaintenanceRecordServiceRequestContextResponse>> GetLockupByServiceRequestId(int serviceRequestId, CancellationToken cancellationToken)
    {
        if (serviceRequestId <= 0)
            return BadRequest("serviceRequestId deve ser maior que zero.");

        var serviceRequest = await _serviceRequestService.GetByIdAsync(serviceRequestId, cancellationToken);

        if (serviceRequest is null)
            return NotFound();

        var response = new MaintenanceRecordServiceRequestContextResponse
        {
            ClientId = serviceRequest.ClientId,
            ClientName = serviceRequest.ClientName,
            EquipmentId = serviceRequest.EquipmentId,
            EquipmentName = serviceRequest.EquipmentName
        };

        return Ok(response);
    }

    [HttpGet("statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<StatusMaintenanceRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StatusMaintenanceRecordResponse>>> GetStatuses(CancellationToken cancellationToken)
    {
        var result = await _maintenanceRecordService.GetStatusesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("service-types")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceTypeMaintenanceRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceTypeMaintenanceRecordResponse>>> GetServiceTypes(CancellationToken cancellationToken)
    {
        var result = await _maintenanceRecordService.GetServiceTypesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("info")]
    [ProducesResponseType(typeof(IReadOnlyList<MaintenanceRecordInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MaintenanceRecordInfoResponse>>> GetInfo(CancellationToken cancellationToken)
    {
        if (!TryGetUserIdFromToken(out var userId))
            return Unauthorized("Token inválido: claim de usuário não encontrada.");

        var result = await _maintenanceRecordService.GetInfoByUserIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MaintenanceRecordDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaintenanceRecordDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _maintenanceRecordService.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MaintenanceRecordDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaintenanceRecordDetailResponse>> Update(int id, [FromBody] MaintenanceRecordUpdateRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Id da rota nao confere com o id do corpo.");
        var result = await _maintenanceRecordService.UpdateAsync(request, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _maintenanceRecordService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    private bool TryGetUserIdFromToken(out int userId)
    {
        var userIdClaim = User.FindFirstValue(AuthConstants.UserIdClaim)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
