namespace CommandParcer
{
    public interface IMethodProvider
    {
        string Invoke(string method, params string[] args);
    }
}