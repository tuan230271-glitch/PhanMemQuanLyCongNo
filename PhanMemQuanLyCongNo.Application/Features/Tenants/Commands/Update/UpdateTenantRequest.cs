using PhanMemQuanLyCongNo.Application.Models;

namespace PhanMemQuanLyCongNo.Application.Features.Tenants.Commands.Update;

public sealed record UpdateTenantRequest(
    string Name,
    string Plan,
    TenantStatus Status);
