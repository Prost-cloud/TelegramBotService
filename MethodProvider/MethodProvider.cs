using Interfaces;
using MethodProcessor;
using System;
using System.Collections.Generic;

namespace CommandParcer
{
    public class MethodProvider : IMethodProvider
    {
        private readonly Dictionary<string, Delegate> _dictionaryMethod;
        private readonly IMethodProcessor _methodProcessor;


        public MethodProvider(IMethodProcessor methodProcessor)
        {
            _methodProcessor = methodProcessor;

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

        public bool TryGetCommandDelegate(string method, string[] args, out Delegate returnDelegate)
        {
            if (_dictionaryMethod.TryGetValue(method, out var command))
            {
                returnDelegate = command;
                return true;
            }
            returnDelegate = null;
            return false;
        }
    }
}
