using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citrina;
using System.Threading;
using ModernDev.InTouch;
using System.Text.RegularExpressions;

namespace TestVKAPINuGet
{
    public class PriceItem
    {
        public string Name { get; }
        public string Price { get; }
        public string AddChar { get; }
        public string Format { get; }
        public string Size { get; }
        public IEnumerable<PhotosPhoto> Examples { get; }

        public PriceItem(string name, string price, string addChar, string format, string size, IEnumerable<PhotosPhoto> examples)
        {
            Name = name;
            Price = price;
            AddChar = addChar;
            Format = format;
            Size = size;
            Examples = examples;
        }
    }

    public class VkMessage<TUserStates> where TUserStates : struct, IComparable, IFormattable, IConvertible
    {
        public int User;
        public string Text;
        private VkClient<TUserStates> Client;
        public int messageId { get; }
        public TUserStates UserState { get; }

        public VkMessage(VkClient<TUserStates> client, int user, string text, int messageId)
        {
            User = user;
            Text = text;
            Client = client;
            this.messageId = messageId;
            UserState = client.GetUserState(user);
        }

        public void MarkAsRead()
        {
            Client.MarkAsRead(this);
        }

        public void Reply(string text)
        {
            Client.SendMessage(User, text);
        }
    }

    public class MessageEventArgs<TUserStates> : EventArgs where TUserStates : struct, IComparable, IFormattable, IConvertible
    {
        public VkMessage<TUserStates> Message { get; private set; }

        public MessageEventArgs(VkClient<TUserStates> client, MessagesMessage message)
        {
            Message = new VkMessage<TUserStates>(client, message.UserId.Value, message.Body, message.Id.Value);
        }

        public MessageEventArgs(VkMessage<TUserStates> vkmessage)
        {
            Message = vkmessage;
        }
    }

    public class VkClient<TUserStates> where TUserStates : struct, IComparable, IFormattable, IConvertible
    {
        private IAccessToken serviceToken = new ServiceAccessToken("8daa69f48daa69f48daa69f4c98dfb9b2188daa8daa69f4d48b067705a13f838097203d", 5370581);
        private IAccessToken token;
        private int groupId = 92499884;
        private static CitrinaClient cit = new CitrinaClient();
        private static InTouch intouch = new InTouch();
        public bool IsReceiving { get; private set; }
        private CancellationTokenSource ctsReceiving;
        static Dictionary<int, TUserStates> _userstates = new Dictionary<int, TUserStates>();
        private Dictionary<int, int> LastMessageFromUser = new Dictionary<int, int>();
        public List<PriceItem> PriceList { get; private set; } = new List<PriceItem>();
        public string AdditionalInfoAboutCommissions { get; private set; }

        public event EventHandler<MessageEventArgs<TUserStates>> MessageReceived;
        public event EventHandler<MessageEventArgs<TUserStates>> MessageSended;
        public event EventHandler<MessageEventArgs<TUserStates>> CommandProcessed; // доденлать!!!!!!!!!!!!!!!!!!!!!
        public event EventHandler<EventArgs> UpdateMessagesError;

        public void SetUserState(int user, TUserStates state)
        {
            //var a = Convert.ToInt32(state); - передалать
            if (state.Equals(default(TUserStates)))
                _userstates.Remove(user);
            else
                _userstates[user] = state;
        }

        public TUserStates GetUserState(int user)
        {
            if (!_userstates.ContainsKey(user))
                _userstates[user] = default(TUserStates);
            return _userstates[user];
        }

        public TUserStates OffState { get; set; }

        public VkClient(string accessToken)
        {
            token = new GroupAccessToken(accessToken, 0, 0);
            intouch.SetServiceToken("8daa69f48daa69f48daa69f4c98dfb9b2188daa8daa69f4d48b067705a13f838097203d");
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
                    /*if (GetUserState(dialog.Message.UserId.Value).Equals(OffState))
                        continue;*/
                    if (LastMessageFromUser.ContainsKey(dialog.Message.UserId.Value))
                        if (LastMessageFromUser[dialog.Message.UserId.Value] == dialog.Message.Id.Value)
                            continue;

                    if (dialog.Unread.Value == 1)
                    {
                        MessageReceived?.Invoke(this, new MessageEventArgs<TUserStates>(this, dialog.Message));
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
                            MessageReceived?.Invoke(this, new MessageEventArgs<TUserStates>(this, message));
                    }

                    LastMessageFromUser[dialog.Message.UserId.Value] = dialog.Message.Id.Value;
                }
                Thread.Sleep(5000);
            }
            IsReceiving = false;
        }

        public void StopReceiving()
        {
            ctsReceiving.Cancel();
        }

        public IEnumerable<MessagesDialog> GetDialogs(int count)
        {
            try
            {
                // Вот тут вылетает StackOverFlowException
                var result = cit.Messages.GetDialogs(new Citrina.StandardApi.Models.MessagesGetDialogsRequest() { AccessToken = token, Count = count, Unread = true }).Result;

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

        public IEnumerable<MessagesMessage> GetMessages(int userId, int count)
        {
            try
            {
                var result = cit.Messages.GetHistory(new Citrina.StandardApi.Models.MessagesGetHistoryRequest() { AccessToken = token, UserId = userId.ToString(), Count = count, Rev = 0 }).Result;

                if (result.IsError || result == null)
                    UpdateMessagesError?.Invoke(this, EventArgs.Empty);

                return result.Response.Items.Reverse().ToArray();
            }
            catch
            {
                UpdateMessagesError?.Invoke(this, EventArgs.Empty);
                return null;
            }
        }

        public void SendMessage(int userId, string message, IEnumerable<PhotosPhoto> attachments = null)
        {
            try
            { 
            var result = cit.Messages.Send(new Citrina.StandardApi.Models.MessagesSendRequest() { AccessToken = token, UserId = userId, Message = message, Attachment = attachments == null ? null : string.Join(",", attachments.Select(a => "photo" + a.OwnerId + '_' + a.Id ).ToArray())}).Result;
            if (result.Response.HasValue)
                MessageSended?.Invoke(this, new MessageEventArgs<TUserStates>(new VkMessage<TUserStates>(this, userId, message, result.Response.Value)));
            else
                UpdateMessagesError?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                UpdateMessagesError?.Invoke(this, EventArgs.Empty);
            }
        }

        public void MarkAsRead(MessagesMessage message)
        {
            cit.Messages.MarkAsRead(new Citrina.StandardApi.Models.MessagesMarkAsReadRequest() { AccessToken = token, PeerId = message.UserId.ToString(), StartMessageId = message.Id });
        }

        public void MarkAsRead(VkMessage<TUserStates> message)
        {
            cit.Messages.MarkAsRead(new Citrina.StandardApi.Models.MessagesMarkAsReadRequest() { AccessToken = token, PeerId = message.User.ToString(), StartMessageId = message.messageId });
        }

        public string GetRules()
        {
            //var topics = cit.Board.GetTopics(new Citrina.StandardApi.Models.BoardGetTopicsRequest() { AccessToken = serviceToken, GroupId = groupId, PreviewLength = 0, Preview = 0, Order = 1, Count = 10 }).Result;
            var topics = intouch.Board.GetTopics(new BoardGetTopicsParams() { GroupId = groupId, Preview = 0, PreviewLength = 0, Order = 2 }).Result;
            /*foreach(var topic in topics.Response.Items)
            {
                topic.Title.ToLower().Contains("правила");
            }*/
            var rulestopic = topics.Data.Items.FirstOrDefault(t => t.Title.ToLower().Contains("правила"));
            var rules = cit.Board.GetComments(new Citrina.StandardApi.Models.BoardGetCommentsRequest() { AccessToken = serviceToken, GroupId = groupId, TopicId = rulestopic.Id }).Result;
            return rules.Response.Items.First().Text;
        }

        public PhotosPhoto GetRandomPictureFromAlbum(int AlbumId)
        {
            //var photos = intouch.Photos.Get(new PhotosGetParams() { AlbumId = AlbumId.ToString(), OwnerId = -groupId }).Result;
            var photos = cit.Photos.Get(new Citrina.StandardApi.Models.PhotosGetRequest() { AccessToken = serviceToken, AlbumId = AlbumId.ToString(), OwnerId = -groupId }).Result;
            Random rnd = new Random();
            return photos.Response.Items.OrderBy(p => rnd.Next()).FirstOrDefault();
        }

        public IEnumerable<PhotosPhoto> GetPhotosFromAlbum(int AlbumId)
        {
            var photos = cit.Photos.Get(new Citrina.StandardApi.Models.PhotosGetRequest() { AccessToken = serviceToken, AlbumId = AlbumId.ToString(), OwnerId = -groupId }).Result;
            return photos.Response.Items;
        }

        public void UpdatePrice()
        {
            var topics = intouch.Board.GetTopics(new BoardGetTopicsParams() { GroupId = groupId, Preview = 0, PreviewLength = 0, Order = 2 }).Result;
            var pricetopic = topics.Data.Items.FirstOrDefault(t => t.Title.ToLower().Contains("commission") || t.Title.ToLower().Contains("price") || t.Title.ToLower().Contains("прайс"));
            var comments = cit.Board.GetComments(new Citrina.StandardApi.Models.BoardGetCommentsRequest() { AccessToken = serviceToken, GroupId = groupId, TopicId = pricetopic.Id }).Result;
            AdditionalInfoAboutCommissions = comments.Response.Items.First().Text;
            var prices = comments.Response.Items.Skip(1).Where(p => p.Text.StartsWith("◄")).ToArray();
            PriceList.Clear();
            foreach (var pricecomment in prices)
            {
                string name = "", cost = "", format = "", addchar = "", size = "";
                IEnumerable<PhotosPhoto> examples;
                var match = Regex.Match(pricecomment.Text, "◄(.+)\\.?\n");
                if (match.Success)
                {
                    name = match.Groups[1].Value;
                    name = name.Trim(new[] { ' ', '.' });
                    name = name[0].ToString().ToUpper() + name.Substring(1).ToLower();
                }
                match = Regex.Match(pricecomment.Text, "Стоимость: (.+)\n");
                if (match.Success)
                    cost = match.Groups[1].Value;
                match = Regex.Match(pricecomment.Text, "персонажа (.+)\n");
                if (match.Success)
                    addchar = match.Groups[1].Value;
                match = Regex.Match(pricecomment.Text, "Формат : (\\w+)");
                if (match.Success)
                    format = match.Groups[1].Value;
                match = Regex.Match(pricecomment.Text, "Размер: ?(.*)\n");
                if (match.Success)
                    size = match.Groups[1].Value;

                examples = pricecomment.Attachments.Select(a => a.Photo).ToArray();

                PriceList.Add(new PriceItem(name, cost, addchar, format, size, examples));
            }
        }

        public IEnumerable<UsersUserXtrCounters> GetUser(IEnumerable<int> Ids)
        {
            var res = cit.Users.Get(new Citrina.StandardApi.Models.UsersGetRequest() { AccessToken = serviceToken, UserIds = Ids.Select(u => u.ToString()) }).Result;
            return res.Response;
        }

        public void SaveUserStates(System.IO.Stream stream)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
            {
                foreach (var us in _userstates)
                {
                    sw.WriteLine("{0} {1}", us.Key, Convert.ChangeType(us.Value, typeof(string)));
                }
            }
        }

        public void LoadUserStates(System.IO.Stream stream)
        {
            _userstates.Clear();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(stream))
            {
                
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(' ');
                    TUserStates tus;
                    Enum.TryParse<TUserStates>(line.Last(), out tus);
                    _userstates.Add(int.Parse(line.First()), tus);
                }
            }
        }
    }
}
