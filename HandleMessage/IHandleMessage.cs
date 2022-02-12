using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HandleMessage
{
    public interface IHandleMessage
    {
        bool IsUpdate { get; }
        Message Message { get; }
        string Result { get; }

        Task Invoke(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    }
}