namespace PhanMemQuanLyCongNo.Application.Models;

public sealed record CreateTaskRequest(
    Guid DebtId,
    Guid AssignedTo,
    DateOnly DueDate,
    string Note);
