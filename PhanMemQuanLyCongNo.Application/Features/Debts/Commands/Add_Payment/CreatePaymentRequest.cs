namespace PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Add_Payment;

public sealed record CreatePaymentRequest(
    decimal Amount,
    string Method,
    string ReceivedBy);
