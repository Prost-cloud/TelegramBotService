using Telegram.Bot.Types;

namespace HandleMessage
{
    public interface IHandleMessage
    {
        bool IsUpdate { get; }
        Message Message { get; }
        string Result { get; }

        void Invoke(Message message, bool isUpdate);
    }
}