namespace PhanMemQuanLyCongNo.Application.Features.Notifications.Commands.Update;

public sealed record UpdateNotificationCommand(
    Guid TenantId,
    Guid NotificationId,
    UpdateNotificationRequest Request);
