namespace PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Update;

public sealed record UpdateCustomerCommand(
    Guid TenantId,
    Guid CustomerId,
    UpdateCustomerRequest Request);
