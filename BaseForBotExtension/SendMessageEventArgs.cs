using System;

namespace BaseForBotExtension
{
    public class SendMessageEventArgs : EventArgs
    {
        public string Text;
        public string Attachments;

        public SendMessageEventArgs(string text)
        {
            Text = text;
        }
    }
}