namespace PhanMemQuanLyCongNo.Application.Models;

public sealed record LoginRequest(
    string Email,
    string Password
);
