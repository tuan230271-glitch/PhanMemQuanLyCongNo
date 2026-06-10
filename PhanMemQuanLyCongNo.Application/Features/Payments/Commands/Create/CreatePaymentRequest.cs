namespace PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Create;

public sealed record CreatePaymentRequest(
    decimal Amount,
    string Method,
    string ReceivedBy);
