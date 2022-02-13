using System;
using System.Collections.Generic;
using System.Text;

namespace MessageParcer
{
    public class DefaultCommandReturn : IDefaultCommandReturn
    {
        private readonly Dictionary<string, string> _commandDefaultReturns;

        public DefaultCommandReturn()
        {
            _commandDefaultReturns = new Dictionary<string, string>()
            {
                { "/add", "Use /add (name) (cost)" },
                { "/delete", "Use /delete (product ID)" },
                { "/addpayer", "Use /addpayer (name)" },
                { "/deletepayer", "Use /deletepayer (payer ID)" },
                { "/addfunds", "Use /addfunds (payer ID) (count)" },
                { "/removefunds", "Use /removefunds (payer ID) (count)" },
                { "/create", "Use /create (name)" },
                { "/deleteshoppinglist", "Use /deleteshoppinglist (shopping list ID)" },
                { "/select", "Use /select (shopping list ID)" },
                { "/show", "Use /show (shopping list ID)" }
            };

        }

        public string GetDefaultReturnByCommandName(string commandName)
        {
            if (_commandDefaultReturns.ContainsKey(commandName))
            {
                return _commandDefaultReturns[commandName];
            }
            return "I dont know that command yet!";
        }
    }
}
