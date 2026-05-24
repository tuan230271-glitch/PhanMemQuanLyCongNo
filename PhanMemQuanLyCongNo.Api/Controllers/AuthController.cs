using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IDebtManagementService service) : ControllerBase
{
    [HttpPost("login")]
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
}
