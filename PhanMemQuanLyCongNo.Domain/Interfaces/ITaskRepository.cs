namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface ITaskRepository
{
    Task<IReadOnlyCollection<Guid>> GetAllIdsAsync(Guid tenantId);

    Task<bool> ExistsAsync(Guid tenantId, Guid id);

    Task AddAsync(Guid id, Guid tenantId, Guid debtId, Guid assignedTo, string status, DateOnly dueDate, string note);

    Task UpdateStatusAsync(Guid tenantId, Guid id, string status);
}
