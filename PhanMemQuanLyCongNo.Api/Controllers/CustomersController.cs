using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public sealed class CustomersController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetCustomers([FromQuery] string? search)
    {
        var tenantId = GetTenantId();

        var customers = service.GetCustomers(tenantId, search);

        return Ok(customers);
    }

    [HttpPost]
    public IActionResult CreateCustomer(CreateCustomerRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var customer = service.CreateCustomer(tenantId, request);

            return Created("", customer);
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