namespace PhanMemQuanLyCongNo.Application.Features.Debts.Queries.Get_By_Id;

public sealed record GetDebtByIdQuery(
    Guid TenantId,
    Guid DebtId);
