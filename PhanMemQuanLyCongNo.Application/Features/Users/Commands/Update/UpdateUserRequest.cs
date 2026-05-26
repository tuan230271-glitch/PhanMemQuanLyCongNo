using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Application.Features.Users.Commands.Update;

public sealed record UpdateUserRequest(
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);
