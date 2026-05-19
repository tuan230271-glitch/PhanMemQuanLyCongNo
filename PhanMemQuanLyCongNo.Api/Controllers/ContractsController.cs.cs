using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public sealed class ContractsController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetContracts()
    {
        var tenantId = GetTenantId();

        var contracts = service.GetContracts(tenantId);

        return Ok(contracts);
    }

    private Guid GetTenantId()
    {
        return service.ResolveTenant(
            Request.Headers["X-Tenant-Id"].FirstOrDefault()
        );
    }
}