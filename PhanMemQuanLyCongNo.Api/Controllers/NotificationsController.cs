using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "TenantAdmin,Operator,Customer")]
public sealed class NotificationsController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetNotifications()
    {
        var tenantId = GetTenantId();

        var notifications = service.GetNotifications(tenantId);

        return Ok(notifications);
    }

    private Guid GetTenantId()
    {
        return service.ResolveTenant(
            Request.Headers["X-Tenant-Id"].FirstOrDefault()
        );
    }
}
