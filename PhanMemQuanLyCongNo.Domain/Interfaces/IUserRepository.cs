namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyCollection<Guid>> GetAllIdsAsync(Guid tenantId);

    Task<bool> ExistsAsync(Guid tenantId, Guid id);

    Task<Guid?> GetIdByEmailAsync(string email);

    Task AddAsync(Guid id, Guid tenantId, string fullName, string email, string role, bool isActive);

    Task UpdateAsync(Guid id, string fullName, string email, string role, bool isActive);

    Task DeleteAsync(Guid tenantId, Guid id);
}
