namespace PhanMemQuanLyCongNo.Application.Features.Notifications.Commands.Update;

public sealed record UpdateNotificationRequest(
    string Channel,
    string Recipient,
    string Message,
    string Status);
