namespace PhanMemQuanLyCongNo.Application.Features.Debts.Commands.Send_Reminder;

public sealed record SendReminderCommand(
    Guid TenantId,
    Guid DebtId,
    SendReminderRequest Request);
