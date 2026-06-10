namespace PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Delete;

public sealed record DeleteCustomerCommand(
    Guid TenantId,
    Guid CustomerId);
