using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(IDebtManagementService service) : ControllerBase
{
    [HttpPost]
    public IActionResult CreateTask(CreateTaskRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var task = service.CreateTask(tenantId, request);

            return Created("", task);
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