using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Send_Reminder;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Update_Status;
using PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Create;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/debts")]
[Authorize(Roles = "TenantAdmin,Operator,Customer")]
public sealed class DebtsController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetDebts(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var tenantId = GetTenantId();

        var debts = service.GetDebts(
            tenantId,
            search,
            status,
            from,
            to,
            page,
            Math.Clamp(pageSize, 1, 100)
        );

        return Ok(debts);
    }

    [HttpGet("{debtId:guid}")]
    public IActionResult GetDebtById(Guid debtId)
    {
        var tenantId = GetTenantId();

        var debt = service.GetDebtById(tenantId, debtId);

        if (debt == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy khoản công nợ."
            });
        }

        return Ok(debt);
    }
    [HttpPost]
    [Authorize(Roles = "TenantAdmin,Operator")]
    public IActionResult CreateDebt(CreateDebtRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var debt = service.CreateDebt(tenantId, request);

            return Created("", debt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpPost("{debtId:guid}/payments")]
    [Authorize(Roles = "TenantAdmin,Operator")]
    public IActionResult AddPayment(Guid debtId, CreatePaymentRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var payment = service.AddPayment(tenantId, debtId, request);

            return Created("", payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpPost("{debtId:guid}/reminders")]
    [Authorize(Roles = "TenantAdmin,Operator")]
    public IActionResult SendReminder(Guid debtId, SendReminderRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var notification = service.SendReminder(tenantId, debtId, request);

            return Created("", notification);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpPatch("{debtId:guid}/status")]
    [Authorize(Roles = "TenantAdmin,Operator")]
    public IActionResult UpdateDebtStatus(Guid debtId, UpdateDebtStatusRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var debt = service.UpdateDebtStatus(tenantId, debtId, request);

            return Ok(debt);
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
