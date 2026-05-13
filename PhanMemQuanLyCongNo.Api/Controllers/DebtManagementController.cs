using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api")]
public sealed class DebtManagementController(IDebtManagementService service) : ControllerBase
{
    [HttpGet("users")]
    public IActionResult GetUsers() => Ok(service.GetUsers(GetTenantId()));

    [HttpPost("users")]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        try
        {
            return Created("", service.CreateUser(GetTenantId(), request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("users/{userId:guid}")]
    public IActionResult UpdateUser(Guid userId, UpdateUserRequest request)
    {
        try
        {
            return Ok(service.UpdateUser(GetTenantId(), userId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("users/{userId:guid}")]
    public IActionResult DeleteUser(Guid userId)
    {
        try
        {
            service.DeleteUser(GetTenantId(), userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("customers")]
    public IActionResult GetCustomers([FromQuery] string? search) =>
        Ok(service.GetCustomers(GetTenantId(), search));

    [HttpPost("customers")]
    public IActionResult CreateCustomer(CreateCustomerRequest request) =>
        Created("", service.CreateCustomer(GetTenantId(), request));

    [HttpGet("contracts")]
    public IActionResult GetContracts() => Ok(service.GetContracts(GetTenantId()));

    [HttpGet("debts")]
    public IActionResult GetDebts(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(service.GetDebts(GetTenantId(), search, status, from, to, page, Math.Clamp(pageSize, 1, 100)));

    [HttpPost("debts")]
    public IActionResult CreateDebt(CreateDebtRequest request)
    {
        try
        {
            return Created("", service.CreateDebt(GetTenantId(), request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("payments")]
    public IActionResult GetPayments([FromQuery] Guid? debtId) => Ok(service.GetPayments(GetTenantId(), debtId));

    [HttpPost("debts/{debtId:guid}/payments")]
    public IActionResult AddPayment(Guid debtId, CreatePaymentRequest request)
    {
        try
        {
            return Created("", service.AddPayment(GetTenantId(), debtId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("tasks")]
    public IActionResult CreateTask(CreateTaskRequest request)
    {
        try
        {
            return Created("", service.CreateTask(GetTenantId(), request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("debts/{debtId:guid}/reminders")]
    public IActionResult SendReminder(Guid debtId, SendReminderRequest request)
    {
        try
        {
            return Created("", service.SendReminder(GetTenantId(), debtId, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("notifications")]
    public IActionResult GetNotifications() => Ok(service.GetNotifications(GetTenantId()));

    [HttpGet("audit-logs")]
    public IActionResult GetAuditLogs() => Ok(service.GetAuditLogs(GetTenantId()));

    [HttpGet("dashboard")]
    public IActionResult GetDashboard() => Ok(service.GetDashboard(GetTenantId()));

    private Guid GetTenantId() => service.ResolveTenant(Request.Headers["X-Tenant-Id"].FirstOrDefault());
}
