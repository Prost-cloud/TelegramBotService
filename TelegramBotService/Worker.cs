using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            string _token = "5067208302:AAFQQhmp40pgz13G_4SRQ8sH1hMPvFPi140";

            TelegramBotClient botClient = new TelegramBotClient(_token);

            using var cts = new CancellationTokenSource();



            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };
            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(15000, stoppingToken);
            }

            cts.Cancel();

        }

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (!(update.Type == UpdateType.Message || update.Type == UpdateType.EditedMessage))
                return;
            // Only process text messages
            if (update.Type == UpdateType.Message)
            {
                if (update.Message!.Type != MessageType.Text)
                    return;
            }
            if (update.Type == UpdateType.EditedMessage)
            {
                if (update.EditedMessage.Type != MessageType.Text)
                    return;
            }


            // Echo received message text
            if (update.Type == UpdateType.Message)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                var messageId = update.Message.MessageId;
                var name = update.Message.Chat.Username;

                //Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
                HandleMessage.HandleMessage handleMessage = new HandleMessage.HandleMessage(messageText, chatId, messageId, name);

                handleMessage.Invoke();
#if DEBUG
                Console.WriteLine("get message:");
                Console.WriteLine($"ID: {update.Message.MessageId} Text:{update.Message.Text}");
                Console.WriteLine($"chat ID:{chatId} Name:{name}");
#endif
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: handleMessage.Result,
                    cancellationToken: cancellationToken);
            }
            else if (update.Type == UpdateType.EditedMessage)
            {
                var chatId = update.EditedMessage.Chat.Id;
                var messageText = update.EditedMessage.Text;
                var messageId = update.EditedMessage.MessageId;
                var name = update.EditedMessage.Chat.Username;
               
                HandleMessage.HandleMessage handleMessage = new HandleMessage.HandleMessage(messageText, chatId, messageId, name, true);

                handleMessage.Invoke();
#if DEBUG
                Console.WriteLine($"Edidet a '{messageText}' message in chat {chatId}. Message id:{messageId}");
#endif
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: handleMessage.Result,
                    cancellationToken: cancellationToken);
            }
        }
        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

       
    }
}
