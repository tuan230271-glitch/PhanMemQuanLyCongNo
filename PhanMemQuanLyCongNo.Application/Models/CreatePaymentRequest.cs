namespace PhanMemQuanLyCongNo.Application.Models;

public sealed record CreatePaymentRequest(
    decimal Amount,
    string Method,
    string ReceivedBy);
