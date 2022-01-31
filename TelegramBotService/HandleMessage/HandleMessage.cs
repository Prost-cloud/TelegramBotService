﻿using System;
using System.Collections.Generic;
using System.Text;
using CommandParcer;

namespace HandleMessage
{
    class HandleMessage
    {
        public string Message { get; private set; }
        public long ChatId { get; private set; }
        public int MessageId { get; private set; }
        public string Name { get; private set; }
        public string Result { get; private set; }
        public bool IsUpdate { get; private set; }

        public HandleMessage(string messageText, long chatId, int messageId, string name) : this(messageText, chatId, messageId, name, false) { }
        
        public HandleMessage(string messageText, long chatId, int messageId, string name, bool isUpdate)
        {
            Message = messageText;
            ChatId = chatId;
            MessageId = messageId;
            Name = name;
            IsUpdate = isUpdate;
        }

        public void Invoke()
        {
            Parcer parcer = new Parcer(ChatId.ToString(), MessageId.ToString(), Name, IsUpdate);
            Result = parcer.ParceCommand(Message);

        }

    }
}
