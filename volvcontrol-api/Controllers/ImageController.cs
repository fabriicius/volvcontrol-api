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
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImageController(IImageService imageService)
    {
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
    }

    /// <summary>Cria uma nova imagem.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ImageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImageResponse>> Create([FromBody] ImageCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _imageService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Obtém imagem por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _imageService.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Atualiza uma imagem.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageResponse>> Update(int id, [FromBody] ImageUpdateRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Id da rota não confere com o id do corpo.");
        var result = await _imageService.UpdateAsync(request, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Remove uma imagem.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _imageService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
