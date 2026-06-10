namespace PhanMemQuanLyCongNo.Application.Features.Notifications.Commands.Create;

public sealed record CreateNotificationCommand(
    Guid TenantId,
    CreateNotificationRequest Request);
