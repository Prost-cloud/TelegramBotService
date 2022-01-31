using DBContext;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBotService.DBContext;

namespace CommandParcer
{
    public class Parcer
    {
        public string ChatId { get; private set; }
        public string MessageId { get; private set; }
        public string Name { get; private set; }

        public Parcer(string chatId, string messageId, string name)
        {
            ChatId = chatId;
            MessageId = messageId;
            Name = name;
        }

        public string ParceCommand(string command)
        {
            List<string> args = command.Split(' ').ToList();

            command = args[0];
            args.RemoveAt(0);

            command = command.ToLower();

            args.Add(MessageId);
            args.Add(ChatId);

            args = CreateArgs(command, args);

            if (args is null)  
                return DefaultOfCommand(command);

            if(args.Count == 0)
            {
                args.Add(ChatId);                
                args.Add(Name);
            }

            MethodProvider methodProvider = new MethodProvider(new DBProvider());

            return methodProvider.Invoke(command, args.ToArray());
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

            comandCountOfArgs.Add("/add", 4);
            comandCountOfArgs.Add("/delete", 3);
            comandCountOfArgs.Add("/addpayer", 3);
            comandCountOfArgs.Add("/deletepayer", 3);
            comandCountOfArgs.Add("/addfunds", 4);
            comandCountOfArgs.Add("/removefunds", 4);
            comandCountOfArgs.Add("/create", 3);
            comandCountOfArgs.Add("/deleteshoppinglist", 3);
            comandCountOfArgs.Add("/select", 3);
            comandCountOfArgs.Add("/start", 0);
            comandCountOfArgs.Add("/show", 3);
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
    }
}