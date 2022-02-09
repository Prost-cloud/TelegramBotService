using MethodProcessor;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandParcer
{
    public class Parcer : IParcer
    {
        //private readonly Dictionary<string, string> _commandDefaultReturn;
        private readonly Dictionary<string, int> _commandCountOfArgs;
        public string Message { get; private set; }
        public bool IsUpdate { get; private set; }
        public string Command { get; private set; }
        public List<string> Args { get; private set; }


        public Parcer()
        {

            //_commandDefaultReturn = new Dictionary<string, string>
            //{
            //    { "/add", "Use /add (name) (cost)" },
            //    { "/delete", "Use /delete (product ID)" },
            //    { "/addpayer", "Use /addpayer (name)" },
            //    { "/deletepayer", "Use /deletepayer (payer ID)" },
            //    { "/addfunds", "Use /addfunds (payer ID) (count)" },
            //    { "/removefunds", "Use /removefunds (payer ID) (count)" },
            //    { "/create", "Use /create (name)" },
            //    { "/deleteshoppinglist", "Use /deleteshoppinglist (shopping list ID)" },
            //    { "/select", "Use /select (shopping list ID)" },
            //    { "/show", "Use /show (shopping list ID)" }
            //};

            _commandCountOfArgs = new Dictionary<string, int>
            {
                { "/add", 2 },
                { "/delete", 1 },
                { "/addpayer", 1 },
                { "/deletepayer", 1 },
                { "/addfunds", 2 },
                { "/removefunds", 2 },
                { "/create", 1 },
                { "/deleteshoppinglist", 1 },
                { "/select", 1 },
                { "/start", 0 },
                { "/show", 1 },
                { "/info", 0 },
                { "/count", 0 }
            };
        }

        public bool TryParceCommand(string command)
        {
            List<string> args = command.Split(' ').ToList();

            if (IsUpdate)
            {
                command = args[0];
                args.RemoveAt(0);

                command = command.ToLower();

                if (command == "/add")
                {
                    Args = CreateArgs(command, args);

                    Command = "/addupdate";
                    return true;
                }
            }

            Command = args[0];
            args.RemoveAt(0);

            Command = command.ToLower();

            Args = CreateArgs(command, args);

            if (Args is null)
            {
                return false;
            }
            return true;
        }

       //private string DefaultOfCommand(string command)
       //{
       //    if (!_commandDefaultReturn.ContainsKey(command))
       //    {
       //        return "I don't know that command yet :(";
       //    }
       //    return _commandDefaultReturn[command];
       //}

        private List<string> CreateArgs(string name, List<string> args)
        {

            if (!_commandCountOfArgs.ContainsKey(name))
            {
                return null;
            }

            if (args.Count == _commandCountOfArgs[name])
            {
                return args;
            }
            if (args.Count < _commandCountOfArgs[name])
            {
                return null;
            }

            if (_commandCountOfArgs[name] == 0)
            {
                return new List<string>();
            }

            List<string> newArgs = new List<string>();

            int difference = args.Count() - _commandCountOfArgs[name];

            string newName = string.Empty;

            for (int i = 0; i <= difference; i++)
            {
                newName += args[i] + " ";
            }

            newArgs.Add(newName);
            for (int i = difference + 1; i < args.Count(); i++)
            {
                newArgs.Add(args[i]);
            }
            return newArgs;
        }

    }
}