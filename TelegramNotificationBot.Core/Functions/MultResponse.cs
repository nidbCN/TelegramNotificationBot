using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;

namespace TelegramNotificationBot.Core.Functions;

public record MultiResponse(IActionResult ActionResult)
{
    [SqlOutput("dbo.NotificationBot_Webhook", connectionStringSetting: "SqlConnectionString")]
    public WebhookBind? Webhook { get; set; }

    public IActionResult HttpResult { get; set; } = ActionResult;
}

public record WebhookBind
{
    public required Guid Id { get; set; }
    public required long ChatId { get; set; }
}