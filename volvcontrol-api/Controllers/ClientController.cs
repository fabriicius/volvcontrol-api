using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using volvcontrol_api.Authorization;
using volvcontrol_api.domain.Exceptions;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthConstants.AdmOnlyPolicy)]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientController(IClientService clientService)
    {
        _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
    }

    /// <summary>Cria um novo cliente com um endereço. Apenas ADM. Não cadastra se email ou documento já existirem.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> Create([FromBody] ClientCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _clientService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ConflictException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Lista clientes paginado (nome, documento, status e plano).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ClientListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ClientListResponse>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _clientService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>Atualiza um cliente. Valida email e documento únicos.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> Update(int id, [FromBody] ClientUpdateRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Id da rota não confere com o id do corpo.");
        try
        {
            var result = await _clientService.UpdateAsync(request, cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
        catch (ConflictException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Obtém cliente por id (com descrição do plano, do status e lista de endereços).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _clientService.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Lista todos os planos (id e descrição).</summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanResponse>>> GetPlans(CancellationToken cancellationToken)
    {
        var result = await _clientService.GetPlansAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Lista todos os status de cliente (id e descrição).</summary>
    [HttpGet("status-clients")]
    [ProducesResponseType(typeof(IReadOnlyList<StatusClientResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StatusClientResponse>>> GetStatusClients(CancellationToken cancellationToken)
    {
        var result = await _clientService.GetStatusClientsAsync(cancellationToken);
        return Ok(result);
    }
}
