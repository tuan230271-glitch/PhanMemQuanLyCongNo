namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Guid tenantId, Guid debtId, string channel, string recipient, string message, string status);
}
