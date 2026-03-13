using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceRequestListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ServiceRequestListResponse>>> GetAllByUserId(int userId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
            return BadRequest("userId deve ser maior que zero.");

        var result = await _serviceRequestService.GetAllByUserIdAsync(userId, cancellationToken);
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

    /// <summary>Lista tipos e status de atendimento em um único endpoint.</summary>
    [HttpGet("lookups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetLookups(CancellationToken cancellationToken)
    {
        var typesMaintenanceRecord = await _serviceRequestService.GetTypeMaintenanceRecordsAsync(cancellationToken);
        var status = await _serviceRequestService.GetStatusServiceRequestsAsync(cancellationToken);
        return Ok(new { typesMaintenanceRecord, status });
    }
}
