using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "SuperAdmin,TenantAdmin,Operator")]
public sealed class DashboardController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetDashboard()
    {
        var tenantId = service.ResolveTenant(
            Request.Headers["X-Tenant-Id"].FirstOrDefault()
        );

        return Ok(service.GetDashboard(tenantId));
    }
}
