using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/tenants")]
public sealed class TenantsController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetTenants() => Ok(service.GetTenants());
}
