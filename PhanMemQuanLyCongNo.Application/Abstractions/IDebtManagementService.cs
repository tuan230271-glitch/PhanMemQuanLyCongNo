using PhanMemQuanLyCongNo.Application.Models;
using PhanMemQuanLyCongNo.Domain.Entities;
using PhanMemQuanLyCongNo.Domain.Enums;

namespace PhanMemQuanLyCongNo.Application.Abstractions;

public interface IDebtManagementService
{
    Guid DefaultTenantId { get; }
    Guid ResolveTenant(string? tenantId);
    IReadOnlyCollection<Tenant> GetTenants();
    IReadOnlyCollection<AppUser> GetUsers(Guid tenantId);
    AppUser CreateUser(Guid tenantId, CreateUserRequest request);
    AppUser UpdateUser(Guid tenantId, Guid userId, UpdateUserRequest request);
    void DeleteUser(Guid tenantId, Guid userId);
    object Login(LoginRequest request);
    IReadOnlyCollection<KhachHang> GetCustomers(Guid tenantId, string? search);
    KhachHang CreateCustomer(Guid tenantId, CreateCustomerRequest request);
    IReadOnlyCollection<Contract> GetContracts(Guid tenantId);
    IReadOnlyCollection<object> GetDebts(Guid tenantId, string? search, string? status, DateOnly? from, DateOnly? to, int page, int pageSize);
    CongNo CreateDebt(Guid tenantId, CreateDebtRequest request);
    ThanhToan AddPayment(Guid tenantId, Guid debtId, CreatePaymentRequest request);
    CollectionTask CreateTask(Guid tenantId, CreateTaskRequest request);
    NotificationLog SendReminder(Guid tenantId, Guid debtId, SendReminderRequest request);
    IReadOnlyCollection<ThanhToan> GetPayments(Guid tenantId, Guid? debtId);
    IReadOnlyCollection<NotificationLog> GetNotifications(Guid tenantId);
    IReadOnlyCollection<AuditLog> GetAuditLogs(Guid tenantId);
    object GetDashboard(Guid tenantId);
}
