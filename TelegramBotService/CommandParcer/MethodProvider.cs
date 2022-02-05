using Interfaces;
using System;
using System.Collections.Generic;

namespace CommandParcer
{
    public class MethodProvider : IDisposable
    {
        private readonly Dictionary<string, Delegate> _dictionaryMethod;
        private readonly IMethodProcessor _methodProcessor;

        public MethodProvider(IMethodProcessor methodProvider)
        {
            _methodProcessor = methodProvider;

            _dictionaryMethod = new Dictionary<string, Delegate>
            {
                { "/add", new Func<string, string, string>(_methodProcessor.AddProduct) },
                { "/addupdate", new Func<string, string, string>(_methodProcessor.UpdateProduct) },
                { "/delete", new Func<string, string>(_methodProcessor.DeleteProduct) },
                { "/addpayer", new Func<string, string>(_methodProcessor.AddPayer) },
                { "/deletepayer", new Func<string, string>(_methodProcessor.DeletePayer) },
                { "/addfunds", new Func<string, string, string>(_methodProcessor.AddFunds) },
                { "/removefunds", new Func<string, string, string>(_methodProcessor.RemoveFunds) },
                { "/create", new Func<string, string>(_methodProcessor.AddShoppingList) },
                { "/deleteshoppinglist", new Func<string, string>(_methodProcessor.DeleteShoppingList) },
                { "/select", new Func<string, string>(_methodProcessor.SelectShoppingList) },
                { "/count", new Func<string>(_methodProcessor.GetCountingByPayers) },
                { "/info", new Func<string>(_methodProcessor.GetShoppingLists) },
                { "/show", new Func<string, string>(_methodProcessor.Show) },
                { "/start", new Func<string>(_methodProcessor.Start) }
            };
        }

        public void Dispose()
        {
            _methodProcessor.Dispose();
        }

        public string Invoke(string method, params string[] args)
        {
            if (_dictionaryMethod.TryGetValue(method, out var command))
            {
                return (string)command.DynamicInvoke(args);
            }
            return "I don't know that command";
        }
    }
}
