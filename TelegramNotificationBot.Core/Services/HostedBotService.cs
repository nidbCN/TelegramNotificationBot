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
        => await UpdateWebhook(botClient, options.Value.BotWebhookUrl, options.Value.SecretToken, cancellationToken);


    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Remove webhook on app shutdown
        logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhook(cancellationToken: cancellationToken);
    }

    public static async Task UpdateWebhook(ITelegramBotClient bot, Uri webhookUrl, string token, CancellationToken cancellationToken)
        => await bot.SetWebhook(url: webhookUrl.ToString(),
            allowedUpdates: [UpdateType.Message],
            secretToken: token,
            cancellationToken: cancellationToken);
}
