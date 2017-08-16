using BaseForBotExtension;
using System;

namespace Artist_Manager_Bot
{
    public class MessageProcessedEventArgs : EventArgs
    {
        BotExtension ProcessedBy { get; }
        public MessageProcessedEventArgs(BotExtension extension)
        {
            ProcessedBy = extension;
        }
    }
}
