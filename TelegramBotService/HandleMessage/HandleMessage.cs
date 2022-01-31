using System;
using System.Collections.Generic;
using System.Text;
using CommandParcer;

namespace HandleMessage
{
    class HandleMessage
    {
        public string Message { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        public string Name { get; set; }
        public string Result { get; set; }

        public HandleMessage(string messageText, long chatId, int messageId, string name)
        {
            Message = messageText;
            ChatId = chatId;
            MessageId = messageId;
            Name = name;
        }

        public void Invoke()
        {
            Parcer parcer = new Parcer(ChatId.ToString(), MessageId.ToString(), Name);
            Result = parcer.ParceCommand(Message);

        }

    }
}
