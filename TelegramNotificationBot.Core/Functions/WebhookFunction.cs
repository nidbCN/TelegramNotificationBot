using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramNotificationBot.Core.Functions;

public class WebhookFunction(
    ILogger<WebhookFunction> logger,
    ITelegramBotClient botClient)
{
    private const string FunctionName = "Notifications";

    [Function(FunctionName)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = FunctionName + "/{id}")] HttpRequest req,
        [SqlInput(commandText: "select * from dbo.NotificationBot_Webhook where Id = @Id",
            commandType: System.Data.CommandType.Text,
            parameters: "@Id={id}",
            connectionStringSetting: "SqlConnectionString")]
        IEnumerable<WebhookBind> webhookList)
    {
        using (logger.BeginScope(FunctionName))
        {
            logger.LogInformation("Receive webhook request.");
            var webhook = webhookList.FirstOrDefault();

            if (webhook is null)
                return new NotFoundObjectResult("Webhook id not found.");

            logger.LogInformation("Notify to chat id {id}.", webhook.ChatId);
            using var reader = new StreamReader(req.Body);
            var message = await reader.ReadToEndAsync();

            try
            {
                await botClient.SendMessage(webhook.ChatId, message, ParseMode.MarkdownV2,
                    replyMarkup: new ReplyKeyboardRemove());
            }
            catch (ApiRequestException apiException)
            {
                return new BadRequestObjectResult(apiException);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occured during send message to telegram.");
            }

            return new CreatedResult();
        }
    }
}