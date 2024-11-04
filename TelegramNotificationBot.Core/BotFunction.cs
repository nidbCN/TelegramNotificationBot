using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramNotificationBot.Core.Configs;
using TelegramNotificationBot.Core.Services;

namespace TelegramNotificationBot.Core;

public class BotFunction(
    ILogger<BotFunction> logger,
    IOptions<TelegramConfig> options,
    ITelegramBotClient botClient,
    UpdateHandler handler)
{
    [Function("Bot")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        if (!req.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var token))
        {
            return new UnauthorizedResult();
        }

        if (token != options.Value.SecretToken)
            return new ForbidResult();

        logger.LogInformation("C# HTTP trigger function processed a request.");

        var update = JsonSerializer.Deserialize<Update>(req.Body);

        try
        {
            handler.HandleUpdateAsync(botClient, update, CancellationToken.None).Wait();
        }
        catch (Exception exception)
        {
            handler.HandleErrorAsync(botClient, exception, Telegram.Bot.Polling.HandleErrorSource.HandleUpdateError, CancellationToken.None).Wait();
        }

        return new OkResult();
    }

}
