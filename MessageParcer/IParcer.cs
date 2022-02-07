using Telegram.Bot.Types;

namespace CommandParcer
{
    public interface IParcer
    {
        bool IsUpdate { get; }
        Message Message { get; }

        string ParceCommand(string command);
    }
}