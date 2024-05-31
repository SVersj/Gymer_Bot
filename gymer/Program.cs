using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.EntityFrameworkCore;
using gymer;

class Program
{
    private static TelegramBotClient botClient;

    static void Main(string[] args)
    {
        botClient = new TelegramBotClient("TOKEN");

        using (var context = new Database())
        {
            context.Database.Migrate();
        }

        var cts = new CancellationTokenSource();

        var handler = new BotHandlers(botClient);
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // получать все типы обновлений
        };
        botClient.StartReceiving(
            handler.HandleUpdateAsync,
            handler.HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token);

        Console.WriteLine("Bot is up and running. Press any key to exit.");
        Console.ReadKey();
        cts.Cancel();
    }
}
