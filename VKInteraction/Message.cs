using System;
using ProtoBuf;
using System.ComponentModel;

namespace VKInteraction
{
    [ProtoContract]
    public sealed class Message
    {
        [ProtoMember(1)]
        public int Id { get; private set; }
        [ProtoMember(2)]
        public string Text { get; private set; }
        [ProtoMember(3)]
        public string Attachments { get; private set; }
        [ProtoMember(4, IsRequired = true), DefaultValue(MessageDirection.Unknown)]
        public MessageDirection Dir { get; private set; }
        [ProtoMember(5)]
        public DateTime DateTime { get; private set; }

        private Message()
        {

        }

        public Message(int id, DateTime date, MessageDirection dir, string text, string attachments = null)
        {
            Id = id;
            Text = text;
            Dir = dir;
            DateTime = date;
            Attachments = attachments;
        }
    }
}
