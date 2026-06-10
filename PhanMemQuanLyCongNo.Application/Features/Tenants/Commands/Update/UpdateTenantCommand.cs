namespace PhanMemQuanLyCongNo.Application.Features.Tenants.Commands.Update;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    UpdateTenantRequest Request);
