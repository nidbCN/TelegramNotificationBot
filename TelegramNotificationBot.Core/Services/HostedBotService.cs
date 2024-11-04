using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using TelegramNotificationBot.Core.Configs;

namespace TelegramNotificationBot.Core.Services;
internal class HostedBotService(
    ILogger<HostedBotService> logger,
    IOptions<TelegramConfig> options,
    ITelegramBotClient botClient)
    : IHostedService
{

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Configure custom endpoint per Telegram API recommendations:
        // https://core.telegram.org/bots/api#setwebhook
        // If you'd like to make sure that the webhook was set by you, you can specify secret data
        // in the parameter secret_token. If specified, the request will contain a header
        // "X-Telegram-Bot-Api-Secret-Token" with the secret token as content.
        var webhookAddress = options.Value.BotWebhookUrl.ToString();
        logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
        await botClient.SetWebhook(
           url: webhookAddress + $"#{DateTime.Now}",
           allowedUpdates: Array.Empty<UpdateType>(),
           secretToken: options.Value.SecretToken,
           cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Remove webhook on app shutdown
        logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhook(cancellationToken: cancellationToken);
    }
}
