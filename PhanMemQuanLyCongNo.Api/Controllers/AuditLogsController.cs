using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public sealed class AuditLogsController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAuditLogs()
    {
        var tenantId = service.ResolveTenant(
            Request.Headers["X-Tenant-Id"].FirstOrDefault()
        );

        return Ok(service.GetAuditLogs(tenantId));
    }
}