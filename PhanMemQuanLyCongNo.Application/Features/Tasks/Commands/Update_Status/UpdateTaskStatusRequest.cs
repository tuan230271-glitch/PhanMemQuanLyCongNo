using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Update_Status;

public sealed record UpdateTaskStatusRequest(
    PhanMemQuanLyCongNo.Application.Models.CollectionTaskStatus Status
);
