using System;

namespace VKInteraction
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; }

        public MessageEventArgs(Message message)
        {
            Message = message;
        }
        /*public string Message { get; }
        public int UserId { get; }
        public MessageEventArgs(string message, int userId)
        {
            Message = message;
            UserId = userId;
        }*/
    }
}