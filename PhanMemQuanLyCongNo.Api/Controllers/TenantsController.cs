using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PhanMemQuanLyCongNo.Application.Abstractions;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize(Roles = "SuperAdmin")]
public sealed class TenantsController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetTenants() => Ok(service.GetTenants());
}
