namespace PhanMemQuanLyCongNo.Application.Features.Tasks.Queries.Get_By_Id;

public sealed record GetTaskByIdQuery(
    Guid TenantId,
    Guid TaskId);
