using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramNotificationBot.Core.Functions;

public class WebhookFunction(
    ILogger<WebhookFunction> logger,
    Dictionary<Guid, long> webhookTable,
    ITelegramBotClient botClient)
{

    [Function("Notifications")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
        [CosmosDBInput(databaseName: "nidb", containerName: "database", Connection = "CosmosDBConnection")] IList<WebhookBind> webhookList)
    {
        var token = req.Query["token"];
        logger.LogInformation("Temp debug: path: {path}, base: {pb}", req.Path, req.PathBase);
        logger.LogInformation("Received notify request, to {id}", token);

        if (Guid.TryParse(token, out var guid))
        {
            var webhook = webhookList.FirstOrDefault(x => x.Id == guid);

            if (webhook is null)
            {
                return new NotFoundResult();
            }

            logger.LogInformation("Notify to chat id {id}.", webhook.Chat.Id);
            using var reader = new StreamReader(req.Body);
            var message = await reader.ReadToEndAsync();
            await botClient.SendMessage(webhook.Chat, message, ParseMode.MarkdownV2, replyMarkup: new ReplyKeyboardRemove());
            return new CreatedResult();
        }
        logger.LogWarning("Can not parse token param: `{str}`", token!);
        return new BadRequestResult();
    }
}