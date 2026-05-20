using PhanMemQuanLyCongNo.Domain.Entities;

namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface IContractRepository
{
    Task<IReadOnlyCollection<Contract>> GetAllAsync(Guid tenantId);

    Task<Contract?> GetByIdAsync(Guid tenantId, Guid id);

    Task AddAsync(Contract contract);

    Task UpdateAsync(Contract contract);
}
