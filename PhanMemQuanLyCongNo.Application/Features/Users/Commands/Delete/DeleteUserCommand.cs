namespace PhanMemQuanLyCongNo.Application.Features.Users.Commands.Delete;

public sealed record DeleteUserCommand(
    Guid TenantId,
    Guid UserId);
