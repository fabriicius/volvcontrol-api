using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using volvcontrol_api.Authorization;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthConstants.AdmOnlyPolicy)]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
    }

    /// <summary>Cria um novo equipamento.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EquipmentResponse>> Create([FromBody] EquipmentCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _equipmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Obtém equipamento por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _equipmentService.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Atualiza um equipamento.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> Update(int id, [FromBody] EquipmentUpdateRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Id da rota não confere com o id do corpo.");
        var result = await _equipmentService.UpdateAsync(request, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Remove um equipamento.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _equipmentService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
