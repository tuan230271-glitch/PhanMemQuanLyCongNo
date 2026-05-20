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
