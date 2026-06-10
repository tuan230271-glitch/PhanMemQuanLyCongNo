namespace PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Create;

public sealed record CreateDebtCommand(
    Guid TenantId,
    CreateDebtRequest Request);
