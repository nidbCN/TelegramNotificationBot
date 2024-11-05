using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Telegram.Bot.Types;

namespace TelegramNotificationBot.Core.Functions;

public record MultiResponse
{
    [CosmosDBOutput("nidb", "database",
        Connection = "CosmosDbConnectionSetting", CreateIfNotExists = true)]
    public WebhookBind? Webhook { get; set; }

    public required IActionResult HttpResult { get; set; }
}

public record WebhookBind
{
    public required Guid Id { get; set; }
    public required Chat Chat { get; set; }
}