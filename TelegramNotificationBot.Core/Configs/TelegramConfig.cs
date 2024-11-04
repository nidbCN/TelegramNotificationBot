namespace TelegramNotificationBot.Core.Configs;

public record TelegramConfig
{
    public required string BotToken { get; set; }
    public required Uri BotWebhookUrl { get; init; }
    public string SecretToken { get; init; } = Guid.NewGuid().ToString();
}
