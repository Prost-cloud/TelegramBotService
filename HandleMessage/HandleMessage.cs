using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessageParcer;
using MethodProcessor;
using MethodProvider;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HandleMessage
{
    public class HandleMessage : IHandleMessage
    {
        public Message Message { get; private set; }
        public string Result { get; private set; }
        public bool IsUpdate { get; private set; }
        private readonly IParcer _parser;
        private readonly IMethodProvider _methodProvider;
        private readonly IMethodProcessor _methodProcessor;
        private readonly IDefaultCommandReturn _defaultCommandReturn;

        public HandleMessage(IParcer parcer, IMethodProvider methodProvider, IMethodProcessor methodProcessor, IDefaultCommandReturn defaultCommandReturn)
        {
            _parser = parcer;
            _methodProvider = methodProvider;
            _methodProcessor = methodProcessor;
            _defaultCommandReturn = defaultCommandReturn;
        }

        public async Task Invoke(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

                this.Message = updateMessage;
                this.IsUpdate = false;

                //Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
#if DEBUG
                Console.WriteLine("\nget message:");
                Console.WriteLine($"ID: {updateMessage.MessageId} Text:{updateMessage.Text}");
                Console.WriteLine($"chat ID:{updateMessage.Chat.Id} Name:{updateMessage.Chat.Username}\n");
#endif
                ProcessMessage();

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: updateMessage.Chat.Id,
                    text: Result,
                    cancellationToken: cancellationToken);
            }


            else if (update.Type == UpdateType.EditedMessage)
            {
                var updateMessage = update.EditedMessage;

                this.Message = updateMessage;
                this.IsUpdate = true;

                ProcessMessage();
#if DEBUG
                Console.WriteLine($"\nEditet a '{updateMessage.Text}' message in chat {updateMessage.Chat.Id}. Message id:{updateMessage.Chat.Id}\n");
#endif
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: updateMessage.Chat.Id,
                    text: Result,
                    cancellationToken: cancellationToken);
            }
        }



        private void ProcessMessage()
        {
            if (!_parser.TryParceCommand(Message.Text, IsUpdate))
            {
                Result = _defaultCommandReturn.GetDefaultReturnByCommandName(Message.Text.Split(" ")[0]);
                return;
            }

            if (!_methodProvider.TryGetCommandDelegate(_parser.Command, _parser.Args.ToArray(), out var method))
            {
                Result = "I don't know that command";
                return;
            }

            _methodProcessor.SetArgumentsByChatIdAndMessageId(Message.Chat.Id, Message.MessageId);

            Result = (string)method.DynamicInvoke(_parser.Args.ToArray());

            try
            {
                _methodProcessor.SaveChanges();
            }
            catch
            {
                Result = "Something went wrong, try anoither time later";
            }
        }
    }
}
