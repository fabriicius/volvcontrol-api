using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using volvcontrol_api.Authorization;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboradController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboradController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardSummaryResponse>> Get(CancellationToken cancellationToken)
    {
        if (!TryGetUserIdFromToken(out var userId))
            return Unauthorized("Token inválido: claim de usuário não encontrada.");

        var result = await _dashboardService.GetSummaryByUserIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    private bool TryGetUserIdFromToken(out int userId)
    {
        var userIdClaim = User.FindFirstValue(AuthConstants.UserIdClaim)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");

        return int.TryParse(userIdClaim, out userId) && userId > 0;
    }
}
