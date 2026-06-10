namespace PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Update;

public sealed record UpdatePaymentRequest(
    decimal Amount,
    string Method,
    string ReceivedBy);
