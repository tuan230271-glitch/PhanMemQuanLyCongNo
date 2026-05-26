using PhanMemQuanLyCongNo.Domain.Enums;

namespace PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Update_Status;

public sealed record UpdateDebtStatusRequest(
    string Status
);
