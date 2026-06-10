namespace PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Update;

public sealed record UpdatePaymentCommand(
    Guid TenantId,
    Guid PaymentId,
    UpdatePaymentRequest Request);
