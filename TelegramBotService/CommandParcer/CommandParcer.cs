using DBContext;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;
using TelegramBotService.DBContext;

namespace CommandParcer
{
    public class Parcer : IDisposable
    {
        private readonly Dictionary<string, string> _comandDefaultReturn;
        private readonly Dictionary<string, int> _comandCountOfArgs;
        public Message Message { get; private set; }
        public bool IsUpdate { get; private set; }

        private readonly MethodProvider _methodProvider;

        public Parcer(Message message, bool isUpdate)
        {            
            Message = message;
            IsUpdate = isUpdate;
            this._methodProvider = new MethodProvider(new MethodProcessor(message));

            _comandDefaultReturn = new Dictionary<string, string>
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

            _comandCountOfArgs = new Dictionary<string, int>
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

        public string ParceCommand(string command)
        {
            List<string> args = command.Split(' ').ToList();

            if (IsUpdate)
            {
                command = args[0];
                args.RemoveAt(0);

                command = command.ToLower();

                if (command == "/add")
                {
                    args = CreateArgs(command, args);

                    command = "/addupdate";

                    return _methodProvider.Invoke(command, args.ToArray());
                }
            }

            command = args[0];
            args.RemoveAt(0);

            command = command.ToLower();           

            args = CreateArgs(command, args);

            if (args is null)  
                return DefaultOfCommand(command);               
                       
            return _methodProvider.Invoke(command, args.ToArray());
        }

        private string DefaultOfCommand(string command)
        {
            if (!_comandDefaultReturn.ContainsKey(command))
            {
                return "I don't know that command yet :(";
            }
            return _comandDefaultReturn[command];
        }

        private List<string> CreateArgs(string name, List<string> args)
        {




            if (!_comandCountOfArgs.ContainsKey(name))
            {
                return null;
            }

            if (args.Count == _comandCountOfArgs[name])
            {
                return args;
            }
            if(args.Count < _comandCountOfArgs[name])
            {
                return null;
            }

            if(_comandCountOfArgs[name] == 0)
            {
                return new List<string>();
            }

            List<string> newArgs = new List<string>();

            int difference = args.Count() - _comandCountOfArgs[name];

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

        public void Dispose()
        {
            _methodProvider.Dispose();
        }
    }
}