namespace PhanMemQuanLyCongNo.Application.Features.Users.Commands.Update;

public sealed record UpdateUserCommand(
    Guid TenantId,
    Guid UserId,
    UpdateUserRequest Request);
