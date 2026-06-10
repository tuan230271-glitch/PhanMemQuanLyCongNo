namespace PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Create;

public sealed record CreatePaymentCommand(
    Guid TenantId,
    Guid DebtId,
    CreatePaymentRequest Request);
