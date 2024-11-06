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
        [SqlInput(commandText: "SELECT [ChatId] FROM [dbo].[NotificationBot_Webhook] WHERE Id = @Id;",
            commandType: System.Data.CommandType.Text,
            parameters: "@Id={id}",
            connectionStringSetting: "SqlConnectionString")]
        IEnumerable<string> chatIdList)
    {
        using (logger.BeginScope(FunctionName))
        {
            logger.LogInformation("Receive webhook request.");

            var chatId = chatIdList.FirstOrDefault();

            if (chatId is null)
                return new NotFoundObjectResult("Webhook id not found.");

            logger.LogInformation("Notify to chat id {id}.", chatId);
            using var reader = new StreamReader(req.Body);
            var message = await reader.ReadToEndAsync();

            try
            {
                await botClient.SendMessage(chatId, message, ParseMode.MarkdownV2,
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