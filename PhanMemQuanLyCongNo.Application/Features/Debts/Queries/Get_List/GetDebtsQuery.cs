namespace PhanMemQuanLyCongNo.Application.Features.Debts.Queries.Get_List;

public sealed record GetDebtsQuery(
    Guid TenantId,
    string? Search,
    string? Status,
    DateOnly? From,
    DateOnly? To,
    int Page,
    int PageSize);
