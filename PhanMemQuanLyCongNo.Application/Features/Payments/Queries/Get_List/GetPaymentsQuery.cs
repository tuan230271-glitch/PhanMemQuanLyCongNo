namespace PhanMemQuanLyCongNo.Application.Features.Payments.Queries.Get_List;

public sealed record GetPaymentsQuery(
    Guid TenantId,
    Guid? DebtId);
