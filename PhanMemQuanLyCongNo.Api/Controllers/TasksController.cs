using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Update_Status;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize(Roles = "TenantAdmin,Operator,FieldCollector")]
public sealed class TasksController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetTasks()
    {
        var tenantId = GetTenantId();

        var tasks = service.GetTasks(tenantId);

        return Ok(tasks);
    }

    [HttpGet("{taskId:guid}")]
    public IActionResult GetTaskById(Guid taskId)
    {
        var tenantId = GetTenantId();

        var task = service.GetTaskById(tenantId, taskId);

        if (task == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy công việc thu hồi nợ."
            });
        }

        return Ok(task);
    }

    [HttpPost]
    [Authorize(Roles = "TenantAdmin,Operator")]
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

    [HttpPatch("{taskId:guid}/status")]
    [Authorize(Roles = "Operator,FieldCollector")]
    public IActionResult UpdateTaskStatus(Guid taskId, UpdateTaskStatusRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var task = service.UpdateTaskStatus(tenantId, taskId, request);

            return Ok(task);
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
