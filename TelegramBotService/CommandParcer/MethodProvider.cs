using Interfaces;
using System;
using System.Collections.Generic;

namespace CommandParcer
{
    public class MethodProvider
    {
        private readonly Dictionary<string, Delegate> _dictionaryMethod;
        private readonly IDbProvider _dBProvider;

        public MethodProvider(IDbProvider dbProvider)
        {
            _dBProvider = dbProvider;
            
            _dictionaryMethod = new Dictionary<string, Delegate>();

            _dictionaryMethod.Add("/add",                    new Func<string, string, string, string, string> (_dBProvider.AddProduct));
            _dictionaryMethod.Add("/addupdate",              new Func<string, string, string, string, string>(_dBProvider.AddUpdate));
            _dictionaryMethod.Add("/delete",                 new Func<string, string, string, string>         (_dBProvider.DeleteProduct));
            _dictionaryMethod.Add("/addpayer",               new Func<string, string, string, string>         (_dBProvider.AddPayer));
            _dictionaryMethod.Add("/deletepayer",            new Func<string, string, string, string>         (_dBProvider.DeletePayer));
            _dictionaryMethod.Add("/addfunds",               new Func<string, string, string, string, string> (_dBProvider.AddFunds));
            _dictionaryMethod.Add("/removefunds",            new Func<string, string, string, string, string> (_dBProvider.RemoveFunds));
            _dictionaryMethod.Add("/create",                 new Func<string, string, string, string>         (_dBProvider.AddShoppingList));
            _dictionaryMethod.Add("/deleteshoppinglist",     new Func<string, string, string, string>         (_dBProvider.DeleteShoppingList));
            _dictionaryMethod.Add("/select",                 new Func<string, string, string, string>         (_dBProvider.SelectShoppingList));
            _dictionaryMethod.Add("/count",                  new Func<string, string, string>                 (_dBProvider.GetCountingByPayers));
            _dictionaryMethod.Add("/info",                   new Func<string, string, string>                 (_dBProvider.GetShoppingLists));
            _dictionaryMethod.Add("/show",                   new Func<string, string, string, string>         (_dBProvider.Show));
            _dictionaryMethod.Add("/start",                  new Func<string, string, string>                 (_dBProvider.AddUser));
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
