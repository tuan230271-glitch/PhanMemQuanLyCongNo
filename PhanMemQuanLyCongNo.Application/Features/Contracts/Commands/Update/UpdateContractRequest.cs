namespace PhanMemQuanLyCongNo.Application.Features.Contracts.Commands.Update;

public sealed record UpdateContractRequest(
    Guid CustomerId,
    string Code,
    decimal Amount,
    decimal InterestRate,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsClosed);
