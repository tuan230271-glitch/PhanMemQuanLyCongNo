namespace PhanMemQuanLyCongNo.Application.Features.Contracts.Commands.Create;

public sealed record CreateContractRequest(
    Guid CustomerId,
    string Code,
    decimal Amount,
    decimal InterestRate,
    DateOnly StartDate,
    DateOnly EndDate);
