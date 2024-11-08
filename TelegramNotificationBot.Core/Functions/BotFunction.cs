using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramNotificationBot.Core.Configs;

namespace TelegramNotificationBot.Core.Functions;

public class BotFunction(
    ILogger<BotFunction> logger,
    IOptions<TelegramConfig> options,
    JsonSerializerOptions jsonOptions,
    ITelegramBotClient botClient)
{
    private const string TelegramBotTokenHeader = "X-Telegram-Bot-Api-Secret-Token";
    public const string Name = "TelegramMessages";

    [Function(Name)]
    public async Task<MultiResponse> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, CancellationToken cancellationToken)
    {
        using (logger.BeginScope(Name))
        {
            logger.LogInformation("Received telegram update request, start Authorize.");

            if (!req.Headers.TryGetValue(TelegramBotTokenHeader, out var token))
            {
                logger.LogWarning("Authorize failed, reason: no token was provided.");
                return new(new UnauthorizedResult());
            }

            if (token != options.Value.SecretToken)
            {
                logger.LogWarning("Authorize failed, reason: token {token} doesn't match.", token!);
                return new(new ForbidResult());
            }

            logger.LogInformation("Authorization completed.");

            var update = await JsonSerializer.DeserializeAsync<Update>(req.Body, jsonOptions, cancellationToken);
            if (update is null)
            {
                using var reader = new StreamReader(req.Body);
                var content = await reader.ReadToEndAsync(cancellationToken);
                logger.LogWarning("Deserialize request payload failed, original data: `{content}`.", content);
                return new(new BadRequestResult());
            }

            var message = update.Message;
            if (message is null)
            {
                logger.LogWarning("Received unknown update type {type}, ignore.", update.Type);
                return new(new NoContentResult());
            }

            logger.LogInformation("Received message from {name}, chat:{id}, content: {content}"
                , message.From?.Username, message.Chat.Id, message.Text);

            var resp = new MultiResponse(new OkResult());

            try
            {
                if (message.Text?.StartsWith('/') ?? false)
                {
                    var commandText = message.Text!;
                    var commandParts = commandText.Split();
                    var command = commandParts[0];
                    logger.LogInformation("Received command {text}", command);

                    if (command.Equals("/new", StringComparison.OrdinalIgnoreCase))
                    {
                        var guid = Guid.NewGuid();

                        resp.Webhook = new()
                        {
                            Id = guid,
                            ChatId = message.Chat.Id
                        };

                        var link = new Uri($"https://tg-notification-bot.azurewebsites.net/api/Notifications/{guid}");

                        var replyMessage = @$"Now you have a Webhook to this chat, send HTTP POST to `{link}` to send message to this chat\.";
                        await botClient.SendMessage(message.Chat, replyMessage, parseMode: ParseMode.MarkdownV2, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    logger.LogWarning("Unknown command, send a prompt.");
                    await botClient.SendMessage(message.Chat, @$"Unknown command `{message.Text}`\.", parseMode: ParseMode.MarkdownV2, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occured. {msg}, {trace}", e.Message, e.StackTrace);
            }

            return resp;
        }
    }
}
