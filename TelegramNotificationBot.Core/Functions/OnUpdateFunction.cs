// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramNotificationBot.Core.Configs;
using TelegramNotificationBot.Core.Utils;

namespace TelegramNotificationBot.Core.Functions;

public class OnUpdateFunction(ILogger<OnUpdateFunction> logger,
    ITelegramBotClient botClient,
    IOptions<TelegramConfig> options)
{
    [Function(nameof(OnUpdateFunction))]
    public async Task Run([EventGridTrigger] string eventData, CancellationToken cancellationToken)
    {
        var cloudEvent = EventGridEvent.Parse(BinaryData.FromString(eventData));

        logger.LogInformation("Event type: {type}, Event subject: {subject}, Topic: {topic}", cloudEvent.EventType, cloudEvent.Subject, cloudEvent.Topic);

        await TelegramWebhookHelper.UpdateWebhook(botClient, options.Value.BotWebhookUrl, options.Value.SecretToken,
            cancellationToken);
    }
}