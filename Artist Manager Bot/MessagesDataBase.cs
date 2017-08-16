using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VKInteraction;

namespace Artist_Manager_Bot
{
    public class MessagesDataBase
    {
        // База данных сообщений
        private Dictionary<int, LinkedList<Message>> messages = new Dictionary<int, LinkedList<Message>>();

        // Публичные методы для взаимодействия с БД

        public void Save(Stream stream) => Serializer.Serialize(stream, messages);

        public void Load(Stream stream) => 
            messages = Serializer.Deserialize<Dictionary<int, LinkedList<Message>>>(stream);

        public Message[] GetMessagesFrom(User user) =>
            messages.ContainsKey(user.Id) ? messages[user.Id].ToArray() : null;

        public Message[] GetAllMessages(int maxforeach = 10) =>
            messages.SelectMany(m => m.Value.Reverse()
                .Take(maxforeach).Reverse())
                .OrderBy(m => m.DateTime).ToArray();

        public void NewMessage(int id, DateTime date, MessageDirection dir, string text, string attachments = null) =>
            NewMessage(new Message(id, date, dir, text, attachments));

        public void NewMessage(Message msg)
        {
            if (!messages.ContainsKey(msg.Id))
                messages.Add(msg.Id, new LinkedList<Message>());

            messages[msg.Id].AddLast(msg);
            if (messages[msg.Id].Count > 10)
                messages[msg.Id].RemoveFirst();
        }

        public MessagesDataBase()
        {
            VK.Interaction.MessageReceived += (s, e) => NewMessage(e.Message);
            VK.Interaction.MessageSended += (s, e) => NewMessage(e.Message);
        }
    }
}
