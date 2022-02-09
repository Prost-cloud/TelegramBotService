using System;

namespace CommandParcer
{
    public interface IMethodProvider
    {
        bool TryGetCommandDelegate(string method, string[] args, out Delegate returnDelegate);
    }
}