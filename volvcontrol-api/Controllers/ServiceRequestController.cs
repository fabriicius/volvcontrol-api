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
public class ServiceRequestController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

    public ServiceRequestController(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService ?? throw new ArgumentNullException(nameof(serviceRequestService));
    }

    /// <summary>Obtém um atendimento por id com todas as informações relacionadas.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceRequestDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _serviceRequestService.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Lista os atendimentos com cliente, status e tipo.</summary>
    [HttpGet("lockup")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceRequestListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ServiceRequestListResponse>>> GetAllByUserId(CancellationToken cancellationToken)
    {
        if (!TryGetUserIdFromToken(out var userId))
            return Unauthorized("Token inválido: claim de usuário não encontrada.");

        var result = await _serviceRequestService.GetAllByUserIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Lista os tipos de atendimento.</summary>
    [HttpGet("service-types")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceTypeMaintenanceRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceTypeMaintenanceRecordResponse>>> GetServiceTypes(CancellationToken cancellationToken)
    {
        var result = await _serviceRequestService.GetServiceTypeMaintenanceRecordsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Cria um novo atendimento.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestResponse>> Create([FromBody] ServiceRequestCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _serviceRequestService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    private bool TryGetUserIdFromToken(out int userId)
    {
        var userIdClaim = User.FindFirstValue(AuthConstants.UserIdClaim)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }

}
