namespace PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Update_Status;

public sealed record UpdateTaskStatusCommand(
    Guid TenantId,
    Guid TaskId,
    UpdateTaskStatusRequest Request);
