namespace PhanMemQuanLyCongNo.Application.Features.Users.Commands.Create;

public sealed record CreateUserCommand(
    Guid TenantId,
    CreateUserRequest Request);
