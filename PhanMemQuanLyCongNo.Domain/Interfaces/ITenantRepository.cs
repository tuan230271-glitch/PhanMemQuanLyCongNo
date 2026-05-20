namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface ITenantRepository
{
    Task<IReadOnlyCollection<Guid>> GetAllIdsAsync();

    Task<bool> ExistsAsync(Guid id);

    Task<Guid?> GetIdByAliasAsync(string alias);

    Task AddAsync(Guid id, string name, string plan, string status);

    Task UpdateStatusAsync(Guid id, string status);
}
