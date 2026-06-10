using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhanMemQuanLyCongNo.Application.Abstractions;
using PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Update;

namespace PhanMemQuanLyCongNo.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "TenantAdmin,Operator")]
public sealed class CustomersController(IDebtManagementService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetCustomers([FromQuery] string? search)
    {
        var tenantId = GetTenantId();

        var customers = service.GetCustomers(tenantId, search);

        return Ok(customers);
    }

    [HttpGet("{customerId:guid}")]
    public IActionResult GetById(Guid customerId)
    {
        var tenantId = GetTenantId();

        var customer = service.GetCustomerById(tenantId, customerId);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy khách hàng."
            });
        }

        return Ok(customer);
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

    [HttpPut("{customerId:guid}")]
    public IActionResult UpdateCustomer(Guid customerId, UpdateCustomerRequest request)
    {
        try
        {
            var tenantId = GetTenantId();

            var customer = service.UpdateCustomer(tenantId, customerId, request);

            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpDelete("{customerId:guid}")]
    public IActionResult DeleteCustomer(Guid customerId)
    {
        try
        {
            var tenantId = GetTenantId();

            service.DeleteCustomer(tenantId, customerId);

            return Ok(new
            {
                message = "Xóa khách hàng thành công."
            });
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
