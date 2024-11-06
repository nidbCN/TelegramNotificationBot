// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System.Text.Json.Serialization;
using Azure.Messaging;
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
    public async Task Run([EventGridTrigger] MyEvent eventData, CancellationToken cancellationToken)
    {
        logger.LogInformation("Event type: {type}, Event subject: {subject}, Topic: {topic}", eventData.EventType, eventData.Subject, eventData.Topic);

        await TelegramWebhookHelper.UpdateWebhook(botClient, options.Value.BotWebhookUrl, options.Value.SecretToken,
            cancellationToken);
    }
}

public record MyEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = default!;

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = default!;

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = default!;

    [JsonPropertyName("eventTime")]
    public DateTime EventTime { get; set; } = default!;

    [JsonPropertyName("data")]
    public BinaryData? Data { get; set; }
    [JsonPropertyName("dataVersion")]
    public string? DataVersion { get; set; }
}