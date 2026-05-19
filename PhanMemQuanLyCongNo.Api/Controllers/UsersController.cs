using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetUsers()
    {
        var tenantId = GetTenantId();

        var users = service.GetUsers(tenantId);

        return Ok(users);
    }

    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var user = service.CreateUser(tenantId, request);

            return Created("", user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpPut("{userId:guid}")]
    public IActionResult UpdateUser(Guid userId, UpdateUserRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var user = service.UpdateUser(tenantId, userId, request);

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpDelete("{userId:guid}")]
    public IActionResult DeleteUser(Guid userId)
    {
        try
        {
            var tenantId = GetTenantId();

            service.DeleteUser(tenantId, userId);

            return NoContent();
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