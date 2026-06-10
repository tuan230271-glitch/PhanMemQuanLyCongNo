namespace PhanMemQuanLyCongNo.Application.Features.Customers.Queries.Get_List;

public sealed record GetCustomersQuery(
    Guid TenantId,
    string? Search);
