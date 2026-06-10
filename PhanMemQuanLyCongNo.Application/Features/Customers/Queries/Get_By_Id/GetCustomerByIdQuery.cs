namespace PhanMemQuanLyCongNo.Application.Features.Customers.Queries.Get_By_Id;

public sealed record GetCustomerByIdQuery(
    Guid TenantId,
    Guid CustomerId);
