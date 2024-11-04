using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TelegramNotificationBot.Core.Configs;
using TelegramNotificationBot.Core.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((ctx, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var configs = ctx.Configuration;
        var telegramConfig = configs
            .GetRequiredSection(nameof(TelegramConfig));

        services.Configure<TelegramConfig>(telegramConfig);
        services
            .AddHttpClient("telegram_webhook")
            .AddTypedClient<ITelegramBotClient>(httpClient =>
                new TelegramBotClient(telegramConfig.Get<TelegramConfig>()!.BotToken, httpClient));
        services.ConfigureTelegramBotMvc();

        services.AddHostedService<HostedBotService>();
        services.AddSingleton<UpdateHandler>();
    })
 .Build();

host.Run();
