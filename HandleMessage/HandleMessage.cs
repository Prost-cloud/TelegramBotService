using System;
using System.Collections.Generic;
using System.Text;
using CommandParcer;
using MethodProcessor;
using Telegram.Bot.Types;

namespace HandleMessage
{
    public class HandleMessage : IHandleMessage
    {
        public Message Message { get; private set; }
        public string Result { get; private set; }
        public bool IsUpdate { get; private set; }
        private IParcer _parser;
        private IMethodProvider _methodProvider;
        private IMethodProcessor _methodProcessor;

        public HandleMessage(IParcer parcer, IMethodProvider methodProvider, IMethodProcessor iMethodProcessor)
        {
            _parser = parcer;
            _methodProvider = methodProvider;
            _methodProcessor = iMethodProcessor;
        }

        public void Invoke(Message message, bool isUpdate)
        {
            Message = message;
            IsUpdate = isUpdate;  

            if(!_parser.TryParceCommand(Message.Text))
            {
                Result = "Add class to get return for command";
            }

            if(!_methodProvider.TryGetCommandDelegate(_parser.Command, _parser.Args.ToArray(), out var @delegate))
            {
                Result = "i don't know that command";
            }

            @delegate.DynamicInvoke(_parser.Args);

        }
    }
}
