namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(Guid tenantId, string userName, string action, string entity, string ipAddress);
}
