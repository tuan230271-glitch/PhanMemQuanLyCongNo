using PhanMemQuanLyCongNo.Domain.Enums;

namespace PhanMemQuanLyCongNo.Application.Models;

public enum TenantStatus
{
    Trial,
    Active,
    Suspended
}

public enum UserRole
{
    SuperAdmin,
    TenantAdmin,
    Operator,
    FieldCollector,
    Customer
}

public enum CollectionTaskStatus
{
    Assigned,
    Traveling,
    MetCustomer,
    Collected,
    Unreachable,
    Completed
}

public sealed record Tenant(Guid Id, string Name, string Plan, TenantStatus Status, DateTime CreatedAt);

public sealed record AppUser(
    Guid Id,
    Guid TenantId,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);

public sealed record Contract(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    string Code,
    decimal Amount,
    decimal InterestRate,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsClosed);

public sealed record CollectionTask(
    Guid Id,
    Guid TenantId,
    Guid DebtId,
    Guid AssignedTo,
    CollectionTaskStatus Status,
    DateOnly DueDate,
    string Note);

public sealed record NotificationLog(
    Guid Id,
    Guid TenantId,
    Guid DebtId,
    string Channel,
    string Recipient,
    string Message,
    string Status,
    DateTime SentAt);

public sealed record AuditLog(
    Guid Id,
    Guid TenantId,
    string UserName,
    string Action,
    string Entity,
    DateTime Timestamp,
    string IpAddress);

public sealed record CreateCustomerRequest(string Name, string Phone, string Address, string CitizenId);
public sealed record CreateDebtRequest(Guid ContractId, decimal PrincipalAmount, decimal PenaltyRate, decimal ReminderFee, DateOnly IssuedDate, DateOnly DueDate, string? Note);
public sealed record CreatePaymentRequest(decimal Amount, string Method, string ReceivedBy);
public sealed record CreateTaskRequest(Guid DebtId, Guid AssignedTo, DateOnly DueDate, string Note);
public sealed record SendReminderRequest(string Channel);
public sealed record LoginRequest(string Email, string Password);
