using System;
using System.Collections.Generic;
using System.Text;
using CommandParcer;
using Telegram.Bot.Types;

namespace HandleMessage
{
    class HandleMessage : IDisposable
    {
        public Message Message { get; private set; }
        public string Result { get; private set; }
        public bool IsUpdate { get; private set; }
        private Parcer _parser;

        public HandleMessage(Message message) : this(message, false) { }
        
        public HandleMessage(Message message, bool isUpdate)
        {
            Message = message;
            IsUpdate = isUpdate;

            _parser = new Parcer(Message, IsUpdate);
        }

        public void Invoke()
        {
            Result = _parser.ParceCommand(Message.Text);
        }

        public void Dispose()
        {
            _parser.Dispose();
        }
    }
}
