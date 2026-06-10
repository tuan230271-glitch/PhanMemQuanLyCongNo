namespace PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Create;

public sealed record CreateCustomerCommand(
    Guid TenantId,
    CreateCustomerRequest Request);
