namespace PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Create;

public sealed record CreateTaskRequest(
    Guid DebtId,
    Guid AssignedTo,
    DateOnly DueDate,
    string Note);
