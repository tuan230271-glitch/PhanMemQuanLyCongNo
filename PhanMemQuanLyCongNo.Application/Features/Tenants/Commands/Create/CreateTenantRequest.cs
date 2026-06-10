using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Application.Features.Tenants.Commands.Create;

public sealed record CreateTenantRequest(
    string Name,
    string Plan,
    TenantStatus Status);
