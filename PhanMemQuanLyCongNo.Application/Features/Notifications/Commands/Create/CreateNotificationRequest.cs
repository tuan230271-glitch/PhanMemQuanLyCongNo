namespace PhanMemQuanLyCongNo.Application.Features.Notifications.Commands.Create;

public sealed record CreateNotificationRequest(
    Guid DebtId,
    string Channel,
    string Recipient,
    string Message);
