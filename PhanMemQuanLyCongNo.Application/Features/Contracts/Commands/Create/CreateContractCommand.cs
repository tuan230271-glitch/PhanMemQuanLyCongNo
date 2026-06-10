namespace PhanMemQuanLyCongNo.Application.Features.Contracts.Commands.Create;

public sealed record CreateContractCommand(
    Guid TenantId,
    CreateContractRequest Request);
