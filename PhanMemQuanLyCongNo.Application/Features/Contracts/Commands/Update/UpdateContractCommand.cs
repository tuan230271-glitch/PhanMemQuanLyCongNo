namespace PhanMemQuanLyCongNo.Application.Features.Contracts.Commands.Update;

public sealed record UpdateContractCommand(
    Guid TenantId,
    Guid ContractId,
    UpdateContractRequest Request);
