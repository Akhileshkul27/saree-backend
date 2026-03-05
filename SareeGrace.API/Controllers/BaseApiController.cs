using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SareeGrace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userIdClaim is null ? Guid.Empty : Guid.Parse(userIdClaim);
    }

    protected string GetUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? "Customer";
    }

    protected IActionResult ApiResult<T>(SareeGrace.Application.DTOs.ApiResponse<T> response)
    {
        if (response.Success)
            return Ok(response);
        return BadRequest(response);
    }
}
