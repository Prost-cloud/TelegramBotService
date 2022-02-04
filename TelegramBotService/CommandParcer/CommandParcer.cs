using DBContext;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBotService.DBContext;

namespace CommandParcer
{
    public class Parcer : IDisposable
    {
        public string Name { get; private set; }
        public bool IsUpdate { get; private set; }

        private MethodProvider _methodProvider;

        public Parcer(long chatId, int messageId, string name, bool isUpdate)
        {            
            Name = name;
            IsUpdate = isUpdate;
            this._methodProvider = new MethodProvider(new MethodProcessor(chatId, messageId, name));
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
            Dictionary<string, string> comandCountOfArgs = new Dictionary<string, string>();

            comandCountOfArgs.Add("/add", "Use /add (name) (cost)");                        
            comandCountOfArgs.Add("/delete", "Use /delete (product ID)");                     
            comandCountOfArgs.Add("/addpayer", "Use /addpayer (name)");                   
            comandCountOfArgs.Add("/deletepayer", "Use /deletepayer (payer ID)");                
            comandCountOfArgs.Add("/addfunds", "Use /addfunds (payer ID) (count)");                   
            comandCountOfArgs.Add("/removefunds", "Use /removefunds (payer ID) (count)");                
            comandCountOfArgs.Add("/create", "Use /create (name)");                     
            comandCountOfArgs.Add("/deleteshoppinglist", "Use /deleteshoppinglist (shopping list ID)");         
            comandCountOfArgs.Add("/select", "Use /select (shopping list ID)");                     
            comandCountOfArgs.Add("/show", "Use /show (shopping list ID)");                       

            if (!comandCountOfArgs.ContainsKey(command))
            {
                return "I don't know that command yet :(";
            }
            return comandCountOfArgs[command];
        }

        private List<string> CreateArgs(string name, List<string> args)
        {
            Dictionary<string, int> comandCountOfArgs = new Dictionary<string, int>();

            comandCountOfArgs.Add("/add", 2);
            comandCountOfArgs.Add("/delete", 1);
            comandCountOfArgs.Add("/addpayer", 1);
            comandCountOfArgs.Add("/deletepayer", 1);
            comandCountOfArgs.Add("/addfunds", 2);
            comandCountOfArgs.Add("/removefunds", 2);
            comandCountOfArgs.Add("/create", 1);
            comandCountOfArgs.Add("/deleteshoppinglist", 1);
            comandCountOfArgs.Add("/select", 1);
            comandCountOfArgs.Add("/start", 0);
            comandCountOfArgs.Add("/show", 1);
            comandCountOfArgs.Add("/info", 0);
            comandCountOfArgs.Add("/count", 0);



            if (!comandCountOfArgs.ContainsKey(name))
            {
                return null;
            }

            if (args.Count == comandCountOfArgs[name])
            {
                return args;
            }
            if(args.Count < comandCountOfArgs[name])
            {
                return null;
            }

            if(comandCountOfArgs[name] == 0)
            {
                return new List<string>();
            }

            List<string> newArgs = new List<string>();

            int difference = args.Count() - comandCountOfArgs[name];

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