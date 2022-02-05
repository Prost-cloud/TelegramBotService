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
            string[] file;

            if (System.IO.File.Exists("TelegramApi.txt"))
            {
                file = System.IO.File.ReadAllLines("TelegramApi.txt");
            }
            else
            {
                throw new System.IO.FileNotFoundException("Create TelegramApi.txt with token in program root");
            }
            string _token = file[0];

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
                var updateMessage = update.Message;

                //Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
#if DEBUG
                Console.WriteLine("\nget message:");
                Console.WriteLine($"ID: {updateMessage.MessageId} Text:{updateMessage.Text}");
                Console.WriteLine($"chat ID:{updateMessage.Chat.Id} Name:{updateMessage.Chat.Username}\n");
#endif
                using (HandleMessage.HandleMessage handleMessage = new HandleMessage.HandleMessage(updateMessage))
                {

                    handleMessage.Invoke();

                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: updateMessage.Chat.Id,
                        text: handleMessage.Result,
                        cancellationToken: cancellationToken);
                }              
            }
            else if (update.Type == UpdateType.EditedMessage)
            {

                var updateMessage = update.Message;

                HandleMessage.HandleMessage handleMessage = new HandleMessage.HandleMessage(updateMessage, true);

                handleMessage.Invoke();
#if DEBUG
                Console.WriteLine($"\nEdidet a '{updateMessage.Text}' message in chat {updateMessage.Chat.Id}. Message id:{updateMessage.Chat.Id}\n");
#endif
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: updateMessage.Chat.Id,
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
