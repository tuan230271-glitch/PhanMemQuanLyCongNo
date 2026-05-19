using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetPayments([FromQuery] Guid? debtId)
    {
        var tenantId = GetTenantId();

        var payments = service.GetPayments(tenantId, debtId);

        return Ok(payments);
    }

    private Guid GetTenantId()
    {
        return service.ResolveTenant(
            Request.Headers["X-Tenant-Id"].FirstOrDefault()
        );
    }
}