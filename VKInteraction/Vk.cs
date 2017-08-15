using Citrina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
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

    [ProtoContract]
    public enum MessageDirection
    {
        [ProtoEnum]
        Unknown = 0,
        [ProtoEnum]
        In = 1,
        [ProtoEnum]
        Out = 2
    }

    public class VK
    {
        IAccessToken token;
        CitrinaClient client = new CitrinaClient();
        public bool IsReceiving { get; private set; }
        public string GroupName { get; private set; }
        public int GroupId { get; private set; }
        public bool Autorized { get; set; }

        // Внутренняя кухня приёма сообщенек
        CancellationTokenSource ctsReceiving;
        private Dictionary<int, int> LastMessageFromUser = new Dictionary<int, int>();

        // Singleton
        private VK()
        {

        }
        private static VK _instance;
        public static VK Interaction
        {
            get
            {
                if (_instance == null)
                    _instance = new VK();

                return _instance;
            }
        }

        public void Autorize(string GroupToken, string Group)
        {
            Autorized = false;
            GroupId = 0;
            token = new CommunityAccessToken(GroupToken, 0, 0);

            var groupAndTest = client.Groups.GetById(new Citrina.StandardApi.Models.GroupsGetByIdRequest() { AccessToken = token, GroupId = Group });
            groupAndTest.Wait();
            if (groupAndTest.IsCompleted && groupAndTest.Result.IsError == false)
            {
                if (!groupAndTest.Result.Response.First().IsAdmin == true)
                    return;

                Autorized = true;
                this.GroupId = GroupId;
                GroupName = groupAndTest.Result.Response.First().Name;
            }
        }

        public string GetUsername(int userId)
        {
            try
            {
                var res = client.Users.Get(new Citrina.StandardApi.Models.UsersGetRequest() { AccessToken = token, UserIds = new[] { userId.ToString() } }).Result;
                return res.Response.First().FirstName + ' ' + res.Response.First().LastName;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<MessagesMessage> GetMessages(int userId, int count)
        {
            try
            {
                var result = client.Messages.GetHistory(new Citrina.StandardApi.Models.MessagesGetHistoryRequest() { AccessToken = token, UserId = userId.ToString(), Count = count, Rev = 0 }).Result;

                //if (result.IsError || result == null)
                    //UpdateMessagesError?.Invoke(this, EventArgs.Empty);

                return result.Response.Items.Reverse().ToArray();
            }
            catch
            {
                //UpdateMessagesError?.Invoke(this, EventArgs.Empty);
                return null;
            }
        }

        public void SendMessage(int userId, string message, IEnumerable<string> attachments = null)
        {
            try
            {
                var result = client.Messages.Send(new Citrina.StandardApi.Models.MessagesSendRequest() { AccessToken = token, UserId = userId, Message = message, Attachment = attachments == null ? null : string.Join(",", attachments/*attachments.Select(a => "photo" + a.OwnerId + '_' + a.Id)*/.ToArray()) }).Result;
                if (result.Response.HasValue)
                    MessageSended?.Invoke(this, new MessageEventArgs(new Message(userId, DateTime.Now, MessageDirection.Out, message, string.Join(",", attachments ?? Enumerable.Empty<string>()))));
                else
                    SendMessageError?.Invoke(this, new MessageEventArgs(new Message(userId, DateTime.Now, MessageDirection.Out, message, string.Join(",", attachments ?? Enumerable.Empty<string>()))));
            }
            catch
            {
                UpdateMessagesError?.Invoke(this, EventArgs.Empty);
            }
        }

        public IEnumerable<MessagesDialog> GetDialogs(int count)
        {
            try
            {
                var result = client.Messages.GetDialogs(new Citrina.StandardApi.Models.MessagesGetDialogsRequest() { AccessToken = token, Count = count, Unread = true }).Result;

                if (result.IsError || result == null)
                    UpdateMessagesError?.Invoke(this, EventArgs.Empty);

                return result.Response?.Items;
            }
            catch
            {
                UpdateMessagesError?.Invoke(this, EventArgs.Empty);
                return null;
            }
        }

        public void StartReceiving()
        {
            ctsReceiving = new CancellationTokenSource();
            Task.Factory.StartNew(() => Receiving(ctsReceiving.Token), ctsReceiving.Token);
        }

        private void Receiving(CancellationToken token)
        {
            IsReceiving = true;
            while (!token.IsCancellationRequested)
            {
                var dialogs = GetDialogs(10);
                if (dialogs == null)
                {
                    Thread.Sleep(10000);
                    continue;
                }
                foreach (var dialog in dialogs)
                {
                    if (LastMessageFromUser.ContainsKey(dialog.Message.UserId.Value))
                        if (LastMessageFromUser[dialog.Message.UserId.Value] == dialog.Message.Id.Value)
                            continue;

                    if (dialog.Unread.Value == 1)
                    {
                        MessageReceived?.Invoke(this, new MessageEventArgs(CitrinaMessageToMyMessage(dialog.Message, MessageDirection.In)));
                    }
                    else
                    {
                        Thread.Sleep(300);
                        var messages = GetMessages(dialog.Message.UserId.Value, dialog.Unread.Value);
                        if (messages == null)
                            continue;
                        if (LastMessageFromUser.ContainsKey(dialog.Message.UserId.Value))
                        {
                            messages = messages.Reverse().ToArray();
                            messages = messages.TakeWhile(m => m.Id.Value != LastMessageFromUser[dialog.Message.UserId.Value]).ToArray();
                            messages = messages.Reverse().ToArray();
                        }

                        foreach (var message in messages)
                            MessageReceived?.Invoke(this, new MessageEventArgs(CitrinaMessageToMyMessage(message, MessageDirection.In)));
                        //MessageReceived?.Invoke(this, new MessageEventArgs(dialog.Message.Body, dialog.Message.UserId.Value));
                    }

                    LastMessageFromUser[dialog.Message.UserId.Value] = dialog.Message.Id.Value;
                }
                Thread.Sleep(3000);
            }
            IsReceiving = false;
        }

        public void StopReceiving()
        {
            ctsReceiving.Cancel();
        }
        
        private Message CitrinaMessageToMyMessage(MessagesMessage message, MessageDirection dir)
        {
            return new Message(message.UserId.Value, message.Date.Value, dir, message.Body, message.Attachments != null ? string.Join(",", message.Attachments.Select(a => a.ToString())) : "");
        }

        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> MessageSended;
        public event EventHandler<MessageEventArgs> SendMessageError;
        public event EventHandler<EventArgs> UpdateMessagesError;
    }
}
