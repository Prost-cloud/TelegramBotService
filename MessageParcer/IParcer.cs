using System.Collections.Generic;

namespace CommandParcer
{
    public interface IParcer
    {
        bool IsUpdate { get; }
        string Message { get; }
        string Command { get;  }
        List<string> Args { get;  }

        bool TryParceCommand(string command);
    }
}