using Citrina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using Citrina.StandardApi.Models;

namespace VKInteraction
{

    public class VK
    {
        IAccessToken token, stoken;
        CitrinaClient client = new CitrinaClient();
        public bool IsReceiving { get; private set; }
        public string GroupName { get; private set; }
        public int GroupId { get; private set; }
        public bool Autorized { get; set; }

        // Внутренняя кухня приёма сообщенек
        CancellationTokenSource ctsReceiving;
        private Dictionary<int, int> LastMessageFromUser = new Dictionary<int, int>();

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

        /// <summary> Авторизация ВК </summary>
        public void Autorize(string GroupToken, string Group)
        {
            // Помечаем что не авторизовано и создаём токен
            Autorized = false;
            GroupId = 0;
            token = new CommunityAccessToken(GroupToken, 0, 0);

            // Пытаемся обратится с токеном к АПИ и получить информацио о своей группе
            var groupAndTest = client.Groups.GetById(new GroupsGetByIdRequest() { AccessToken = token, GroupId = Group });
            groupAndTest.Wait();
            // Если не вылетело исключение и не произошла ошибка в АПИ - токен верный
            if (groupAndTest.IsCompleted && groupAndTest.Result.IsError == false)
            {
                // Если не админ - значит группа не наша, выходим с Autorized == false
                if (!groupAndTest.Result.Response.First().IsAdmin == true)
                    return;

                // Иначе - успешно, сохраняем GroupID и GroupName
                Autorized = true;
                this.GroupId = GroupId;
                GroupName = groupAndTest.Result.Response.First().Name;
            }
        }

        /// <summary> Установка сервисного токена </summary>
        public void SetServiceToken(string token, int appid)
        {
            stoken = new ServiceAccessToken(token, appid);
        }

        /// <summary> Получение имени пользователя по ID </summary>
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

        /// <summary> Получение сообщеней </summary>
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

        /// <summary> Отправка сообщения </summary>
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

        /// <summary> Получение списка диалогов </summary>
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

        /// <summary> Отправка изображения </summary>
        public void SendImage(int userId, string fname)
        {
            var call = client.Photos.GetMessagesUploadServer(new PhotosGetMessagesUploadServerRequest() { AccessToken = token }).Result;
            var upload = client.Uploader.Photos.UploadMessagePhotoAsync(call.Response, fname).Result;
            if (upload.IsError)
                return;
            var saved = client.Photos.SaveMessagesPhoto(new PhotosSaveMessagesPhotoRequest() { AccessToken = token, Photo = upload.Data.Photo, Hash = upload.Data.Hash, Server = upload.Data.Server }).Result;
            var photo = "photo" + saved.Response.First().OwnerId + "_" + saved.Response.First().Id;
            var msg = client.Messages.Send(new MessagesSendRequest() { AccessToken = token, UserId = userId, Attachment = photo }).Result;
        }

        /// <summary> Получение изображений из альбома </summary>
        public string[] GetAlbumPictures(int albumId, int ownerId = 0)
        {
            if (ownerId == 0)
                ownerId = -GroupId;

            var photos = client.Photos.Get(new PhotosGetRequest() { AccessToken = stoken, AlbumId = albumId.ToString(), OwnerId = -ownerId }).Result;
            return photos.Response.Items.Select(p => $"photo{p.OwnerId}_{p.Id}").ToArray();
        }

        /// <summary> Преобразование сообщения из пакета Citrina в собственный тип сообщения </summary>
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
