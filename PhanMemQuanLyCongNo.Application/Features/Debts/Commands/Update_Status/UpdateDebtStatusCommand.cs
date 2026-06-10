namespace PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Update_Status;

public sealed record UpdateDebtStatusCommand(
    Guid TenantId,
    Guid DebtId,
    UpdateDebtStatusRequest Request);
