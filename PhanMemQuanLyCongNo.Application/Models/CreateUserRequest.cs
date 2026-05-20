namespace PhanMemQuanLyCongNo.Application.Models;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);
