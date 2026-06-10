namespace PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Create;

public sealed record CreateTaskCommand(
    Guid TenantId,
    CreateTaskRequest Request);
