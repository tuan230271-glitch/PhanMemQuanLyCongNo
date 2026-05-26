using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Application.Features.Users.Commands.Create;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);
