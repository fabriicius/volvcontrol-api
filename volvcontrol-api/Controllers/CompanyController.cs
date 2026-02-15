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
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>Cria uma nova empresa.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CompanyCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _companyService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Obtém empresa por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _companyService.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Lista todas as empresas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _companyService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Atualiza uma empresa.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> Update(int id, [FromBody] CompanyUpdateRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Id da rota não confere com o id do corpo.");
        var result = await _companyService.UpdateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Remove uma empresa.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _companyService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
