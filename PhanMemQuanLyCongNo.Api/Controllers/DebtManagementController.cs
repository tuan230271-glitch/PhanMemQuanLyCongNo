using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Features.Auth.Commands.Login;
using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class DebtManagementController(IDebtManagementService service) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login(LoginRequest request)
    {
        try
        {
            return Ok(service.Login(request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    private Guid GetTenantId()
    {
        return service.ResolveTenant(
            Request.Headers["X-Tenant-Id"].FirstOrDefault()
        );
    }
}