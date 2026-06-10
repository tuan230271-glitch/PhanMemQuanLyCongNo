using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Update;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "TenantAdmin,Operator,Customer")]
public sealed class PaymentsController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetPayments([FromQuery] Guid? debtId)
    {
        var tenantId = GetTenantId();

        var payments = service.GetPayments(tenantId, debtId);

        return Ok(payments);
    }

    [HttpPut("{paymentId:guid}")]
    [Authorize(Roles = "TenantAdmin,Operator")]
    public IActionResult UpdatePayment(Guid paymentId, UpdatePaymentRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var payment = service.UpdatePayment(tenantId, paymentId, request);

            return Ok(payment);
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
