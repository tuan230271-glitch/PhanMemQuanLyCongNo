using PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Create;

namespace PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Add_Payment;

public sealed record AddPaymentToDebtCommand(
    Guid TenantId,
    Guid DebtId,
    CreatePaymentRequest Request);
