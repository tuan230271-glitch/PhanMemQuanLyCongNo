namespace PhanMemQuanLyCongNo.Application.Models;

public sealed record UpdateUserRequest(
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);
