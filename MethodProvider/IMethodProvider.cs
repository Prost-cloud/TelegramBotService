using System;

namespace MethodProvider
{
    public interface IMethodProvider
    {
        bool TryGetCommandDelegate(string method, string[] args, out Delegate returnDelegate);
    }
}