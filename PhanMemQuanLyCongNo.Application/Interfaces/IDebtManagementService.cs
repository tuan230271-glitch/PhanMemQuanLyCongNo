using PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Customers.Commands.Update;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Send_Reminder;
using PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Update_Status;
using PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Payments.Commands.Update;
using PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Tasks.Commands.Update_Status;
using PhanMemQuanLyCongNo.Application.Features.Users.Commands.Create;
using PhanMemQuanLyCongNo.Application.Features.Users.Commands.Update;
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
    KhachHang? GetCustomerById(Guid tenantId, Guid customerId);
    KhachHang CreateCustomer(Guid tenantId, CreateCustomerRequest request);
    KhachHang UpdateCustomer(Guid tenantId, Guid customerId, UpdateCustomerRequest request);
    void DeleteCustomer(Guid tenantId, Guid customerId);
    IReadOnlyCollection<Contract> GetContracts(Guid tenantId);
    IReadOnlyCollection<object> GetDebts(Guid tenantId, string? search, string? status, DateOnly? from, DateOnly? to, int page, int pageSize);
    object? GetDebtById(Guid tenantId, Guid debtId);
    CongNo CreateDebt(Guid tenantId, CreateDebtRequest request);
    CongNo UpdateDebtStatus(Guid tenantId, Guid debtId, UpdateDebtStatusRequest request);
    ThanhToan AddPayment(Guid tenantId, Guid debtId, CreatePaymentRequest request);
    ThanhToan UpdatePayment(Guid tenantId, Guid paymentId, UpdatePaymentRequest request);
    CollectionTask CreateTask(Guid tenantId, CreateTaskRequest request);
    IReadOnlyCollection<CollectionTask> GetTasks(Guid tenantId);
    CollectionTask? GetTaskById(Guid tenantId, Guid taskId);
    CollectionTask UpdateTaskStatus(Guid tenantId, Guid taskId, UpdateTaskStatusRequest request);
    NotificationLog SendReminder(Guid tenantId, Guid debtId, SendReminderRequest request);
    IReadOnlyCollection<ThanhToan> GetPayments(Guid tenantId, Guid? debtId);
    IReadOnlyCollection<NotificationLog> GetNotifications(Guid tenantId);
    IReadOnlyCollection<AuditLog> GetAuditLogs(Guid tenantId);
    object GetDashboard(Guid tenantId);
}
