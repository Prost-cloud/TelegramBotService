using Interfaces;
using System;
using System.Collections.Generic;

namespace CommandParcer
{
    public class MethodProvider : IMethodProvider
    {
        private readonly Dictionary<string, Delegate> _dictionaryMethod;
        private IMethodProcessor _methodProcessor;


        public MethodProvider(IMethodProcessor methodProcessor)
        {
            _methodProcessor = methodProcessor;

            _dictionaryMethod = new Dictionary<string, Delegate>();

            _dictionaryMethod.Add("/add", new Func<string, string, string>(_methodProcessor.AddProduct));
            _dictionaryMethod.Add("/addupdate", new Func<string, string, string>(_methodProcessor.UpdateProduct));
            _dictionaryMethod.Add("/delete", new Func<string, string>(_methodProcessor.DeleteProduct));
            _dictionaryMethod.Add("/addpayer", new Func<string, string>(_methodProcessor.AddPayer));
            _dictionaryMethod.Add("/deletepayer", new Func<string, string>(_methodProcessor.DeletePayer));
            _dictionaryMethod.Add("/addfunds", new Func<string, string, string>(_methodProcessor.AddFunds));
            _dictionaryMethod.Add("/removefunds", new Func<string, string, string>(_methodProcessor.RemoveFunds));
            _dictionaryMethod.Add("/create", new Func<string, string>(_methodProcessor.AddShoppingList));
            _dictionaryMethod.Add("/deleteshoppinglist", new Func<string, string>(_methodProcessor.DeleteShoppingList));
            _dictionaryMethod.Add("/select", new Func<string, string>(_methodProcessor.SelectShoppingList));
            _dictionaryMethod.Add("/count", new Func<string>(_methodProcessor.GetCountingByPayers));
            _dictionaryMethod.Add("/info", new Func<string>(_methodProcessor.GetShoppingLists));
            _dictionaryMethod.Add("/show", new Func<string, string>(_methodProcessor.Show));
            _dictionaryMethod.Add("/start", new Func<string>(_methodProcessor.Start));
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
