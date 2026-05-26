namespace PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Create;

public sealed record CreateDebtRequest(
    Guid ContractId,
    decimal PrincipalAmount,
    decimal PenaltyRate,
    decimal ReminderFee,
    DateOnly IssuedDate,
    DateOnly DueDate,
    string? Note);
