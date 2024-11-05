using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramNotificationBot.Core.Configs;
using TelegramNotificationBot.Core.Utils;

namespace TelegramNotificationBot.Core.Services;
internal class HostedBotService(
    ILogger<HostedBotService> logger,
    IOptions<TelegramConfig> options,
    ITelegramBotClient botClient)
    : IHostedService
{

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Host start, set webhook to {url}.", options.Value.BotWebhookUrl);
        await TelegramWebhookHelper.UpdateWebhook(botClient, options.Value.BotWebhookUrl, options.Value.SecretToken, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Remove webhook on app shutdown
        logger.LogInformation("Host stop, removing webhook");
        // await botClient.DeleteWebhook(cancellationToken: cancellationToken);
        return Task.CompletedTask;
    }


}
