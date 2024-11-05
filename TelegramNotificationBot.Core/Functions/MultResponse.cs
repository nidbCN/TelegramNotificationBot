using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;

namespace TelegramNotificationBot.Core.Functions;

public record MultiResponse
{
    [SqlOutput("dbo.NotificationBot_Webhook", connectionStringSetting: "SqlConnectionString")]
    public WebhookBind? Webhook { get; set; }

    public required IActionResult HttpResult { get; set; }
}

public record WebhookBind
{
    public required Guid Id { get; set; }
    public required long ChatId { get; set; }
}