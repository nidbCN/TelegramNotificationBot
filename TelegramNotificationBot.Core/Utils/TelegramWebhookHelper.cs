using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramNotificationBot.Core.Utils;

public static class TelegramWebhookHelper
{
    public static async Task UpdateWebhook(ITelegramBotClient bot, Uri webhookUrl, string token, CancellationToken cancellationToken)
        => await bot.SetWebhook(url: webhookUrl.ToString(),
            allowedUpdates: [UpdateType.Message],
            secretToken: token,
            cancellationToken: cancellationToken);
}