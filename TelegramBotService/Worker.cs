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
using HandleMessage;

namespace TelegramBotService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private IHandleMessage _handleMessage;

        public Worker(ILogger<Worker> logger, IHandleMessage handleMessage)
        {
            _logger = logger;
            _handleMessage = handleMessage;
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

         async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            if (!(update.Type == UpdateType.Message || update.Type == UpdateType.EditedMessage))
                return;

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
                    _handleMessage.Invoke(updateMessage, isUpdate: false);

                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: updateMessage.Chat.Id,
                        text: _handleMessage.Result,
                        cancellationToken: cancellationToken);
            }
            else if (update.Type == UpdateType.EditedMessage)
            {

                var updateMessage = update.EditedMessage;

                   _handleMessage.Invoke(updateMessage, isUpdate: true);
#if DEBUG
                    Console.WriteLine($"\nEditet a '{updateMessage.Text}' message in chat {updateMessage.Chat.Id}. Message id:{updateMessage.Chat.Id}\n");
#endif
                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: updateMessage.Chat.Id,
                        text: _handleMessage.Result,
                        cancellationToken: cancellationToken);
            }
        }
         Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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