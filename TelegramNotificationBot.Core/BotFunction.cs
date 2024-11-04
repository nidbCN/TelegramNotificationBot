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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var update = JsonSerializer.Deserialize<Update>(content, options);

        logger.LogInformation("Deserialize update, id: {id}", update?.Message?.Chat.Id);

        try
        {
            var chat = update?.Message?.Chat;
            if (chat is null)
            {
                logger.LogWarning("Chat is null!");
            }

            var chatId = new ChatId(chat.Id);
            if (chatId is null)
            {
                logger.LogWarning("Chat id is null.");
            }

            await botClient.SendMessage(chatId!, "here", parseMode: ParseMode.MarkdownV2, replyMarkup: new ReplyKeyboardRemove());
            await handler.HandleUpdateAsync(botClient, update, CancellationToken.None);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured. {msg}, {trace}", e.Message, e.StackTrace);
        }

        return new OkResult();
    }
}
