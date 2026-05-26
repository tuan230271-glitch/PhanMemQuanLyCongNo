namespace PhanMemQuanLyCongNo.Application.Features.Auth.Commands.Login;

public sealed record LoginRequest(
    string Email,
    string Password);
