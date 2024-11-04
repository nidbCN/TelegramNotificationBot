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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        logger.LogInformation("Received request, start Authorize.");

        if (!req.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var token))
        {
            return new UnauthorizedResult();
        }

        if (token != options.Value.SecretToken)
            return new ForbidResult();

        using var reader = new StreamReader(req.Body);
        var content = await reader.ReadToEndAsync();

        logger.LogInformation("Received with content: {content}", content);

        var update = JsonSerializer.Deserialize<Update>(content);

        try
        {
            await botClient.SendMessage(update.Message.Chat.Id, "text");
            await handler.HandleUpdateAsync(botClient, update, CancellationToken.None);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured.");
        }
        
        return new OkResult();
    }
}
