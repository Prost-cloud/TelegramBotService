using System.Collections.Generic;

namespace MessageParcer
{
    public interface IParcer
    {
        string Message { get; }
        string Command { get;  }
        List<string> Args { get;  }

        bool TryParceCommand(string command, bool isUpdate);
    }
}