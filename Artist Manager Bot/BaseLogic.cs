using BaseForBotExtension;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VKInteraction;

namespace Artist_Manager_Bot
{
    class Controller
    {
        VK vk = VK.Interaction;
        BotExtension[] Extensions;
        public CustomColumn[] CustomColumns { get; private set; }
        public System.Windows.Forms.Form[] CustomTabs { get; private set; }
        
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> MessageSended;
        public event EventHandler<MessageProcessedEventArgs> MessageProcessed;

        public UsersData Users { get; } = new UsersData();
        public MessagesDataBase Messages { get; } = new MessagesDataBase();

        public string[] GetExtensions() => Extensions.Select(e => e.Name).ToArray();

        public IBotExtensionInfo GetExtension(int id) => Extensions[id];

        public static string AssemblyDirectory
        {
            get
            {
                /*string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);*/
                return Directory.GetCurrentDirectory();
            }
        }

        private bool _Receiving = false;
        public bool Receiving
        {
            set
            {
                if (_Receiving == value)
                    return;

                if (value == true && Autorized == false)
                    return;

                _Receiving = value;
                if (_Receiving)
                    vk.StartReceiving();
                else
                    vk.StopReceiving();
            }
            get
            {
                return _Receiving;
            }
        }
        public string GroupName => vk.GroupName;
        public bool Autorized => vk.Autorized;

        public Controller()
        {
            vk.MessageReceived += Vk_MessageReceived;
            vk.MessageSended += (s, e) => MessageSended?.Invoke(s, e);
        }

        private void Vk_MessageReceived(object sender, MessageEventArgs e)
        {
            if (!Users.Contains(e.Message.Id))
            {
                Task.Delay(300).Wait();
                var name = vk.GetUsername(e.Message.Id);
                if (name != null)
                    Users.AddUser(e.Message.Id, name, null);
            }

            MessageReceived?.Invoke(this, e);

            for (int i = 0; i < Extensions.Length; i++)
            {
                var extension = Extensions[i];

                if (!extension.Enabled)
                    return;

                var res = extension.ProcessMessage(e.Message.Id, e.Message.Text);
                if (res == ProcessResult.Processed)
                {
                    MessageProcessed?.Invoke(this, new MessageProcessedEventArgs(extension));
                    if (extension.StopAfterProcessed)
                        return;
                }
            }
        }

        public void Autorize(string token, string group)
        {
            vk.Autorize(token, group);
        }

        public void LoadDLLs()
        {
            var dir  = Directory.GetCurrentDirectory();
            var extensions = Directory.GetFiles(Path.Combine(AssemblyDirectory, "Modules"), "*.dll")
                .Select(fname => Assembly.LoadFrom(fname))
                .SelectMany(asm => asm.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(BotExtension)))
                .Select(et => Activator.CreateInstance(et))
                .Cast<BotExtension>()
                .ToArray();
            foreach (var ext in extensions)
            {
                ext.vk = vk;
                ext.UsersGridUpdate += (s, e) => Users[e.UserId].SetParam(e.ColumnName, e.Value);
            }

            Extensions = extensions.OrderByDescending(e => e.Priority).ToArray();
            CustomColumns = Extensions.SelectMany(e => e.GetUsersGridColumns()).ToArray();
            CustomTabs = Extensions.SelectMany(e => e.GetCustomTabs()).ToArray();

            foreach (var cc in CustomColumns)
                Users.SetDefaultValue(cc.Name, cc.DefaultValue);
        }

        public void SaveAll()
        {
            using(var fs = new FileStream("msgs.bin", FileMode.Create))
                Messages.Save(fs);

            using (var fs = new FileStream("usrs.bin", FileMode.Create))
                Users.Save(fs);

            foreach (var ext in Extensions)
                ext.Save();
        }

        public void LoadAll()
        {
            if (File.Exists("usrs.bin"))
                using (var fs = new FileStream("usrs.bin", FileMode.Open))
                    Users.Load(fs);

            if (File.Exists("msgs.bin"))
                using (var fs = new FileStream("msgs.bin", FileMode.Open))
                    Messages.Load(fs);

            foreach (var ext in Extensions)
                try
                {
                    ext.Load();
                }
                catch
                { }
        }
    }

    public class MessageProcessedEventArgs : EventArgs
    {
        BotExtension ProcessedBy { get; }
        public MessageProcessedEventArgs(BotExtension extension)
        {
            ProcessedBy = extension;
        }
    }

    public class UsersData
    {
        [ProtoContract]
        public class User
        {
            private User() { }

            [ProtoMember(1)]
            public int Id { get; private set; }
            [ProtoMember(2)]
            public string Name { get; private set; }
            [ProtoMember(3)]
            private Dictionary<string, string> Params { get; set; }
            private Action<User, string> RaiseEvent { get; set; }
            private UsersData UsersData { get; set; }

            public object GetParam(string Key)
            {
                return Params.ContainsKey(Key) ? Params[Key] : UsersData.GetDefaultValue(Key);
            }
            public void SetParam(string Key, object Value)
            {
                Params[Key] = Value.ToString();
                RaiseEvent?.Invoke(this, Key);
            }

            public void SetUserDataAndRaiseEvent(Action<User, string> re, UsersData ud)
            {
                RaiseEvent = re;
                UsersData = ud;
            }

            public User(int id, string name, Action<User, string> raiseEvent, UsersData ud, Dictionary<string, string> prms = null)
            {
                Id = id;
                Name = name;
                UsersData = ud;
                Params = prms ?? new Dictionary<string, string>();
                RaiseEvent = raiseEvent;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        //public static UsersData Users;

        private Dictionary<string, object> DefaultValues = new Dictionary<string, object>();
        private HashSet<User> _Users = new HashSet<User>();

        public void AddUser(int id, string name, Dictionary<string, string> prms = null)
        {
            if (prms == null)
                prms = new Dictionary<string, string>();

            foreach (var kvp in DefaultValues)
                if (!prms.ContainsKey(kvp.Key))
                    prms.Add(kvp.Key, kvp.Value.ToString());

            var newuser = new User(id, name, RaiseChangedEvent, this, prms);
            _Users.Add(newuser);
            UserAdded?.Invoke(this, new UserAddedEventArgs(newuser));
        }

        public void RemoveUser(int id)
        {
            _Users.RemoveWhere(u => u.Id == id);
            UserRemoved?.Invoke(this, new UserRemovedEventArgs(id));
        }

        public bool Contains(int id)
        {
             return _Users.Any(u => u.Id == id);
        }

        public void SetDefaultValue(string Key, object Value)
        {
            DefaultValues[Key] = Value;
        }

        public object GetDefaultValue(string Key)
        {
            return DefaultValues.ContainsKey(Key) ? DefaultValues[Key] : null;
        }

        public event EventHandler<UserAddedEventArgs> UserAdded;
        public event EventHandler<UserRemovedEventArgs> UserRemoved;
        public event EventHandler<UserChangedEventArgs> UserChanged;

        public int Count => _Users.Count;

        private void RaiseChangedEvent(User user, string field)
        {
            UserChanged?.Invoke(this, new UserChangedEventArgs(user.Id, field));
        }

        public User this[int id] => _Users.FirstOrDefault(u => u.Id == id);

        public User this[string name] => _Users.FirstOrDefault(u => u.Name == name);

        public void Save(Stream stream)
        {
            /*IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, Users);*/
            Serializer.Serialize(stream, _Users);
        }

        public void Load(Stream stream)
        {
            /* if (stream.Length == 0)
                 return;

             IFormatter formatter = new BinaryFormatter();
             Users = (HashSet<User>)formatter.Deserialize(stream);*/

            _Users = Serializer.Deserialize<HashSet<User>>(stream);

            foreach (var user in _Users)
            {
                user.SetUserDataAndRaiseEvent(RaiseChangedEvent, this);
                UserAdded?.Invoke(this, new UserAddedEventArgs(user));
            }
        }

        public UsersData()
        {
            //Users = this;
        }
    }
    
    public class MessagesDataBase
    {
        private Dictionary<int, LinkedList<Message>> messages = new Dictionary<int, LinkedList<Message>>();

        public void Save(Stream stream)
        {
            /*
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, messages);
            */
            Serializer.Serialize(stream, messages);
        }

        public void Load(Stream stream)
        {
            /*
            IFormatter formatter = new BinaryFormatter();
            messages = (Dictionary<int, LinkedList<Message>>)formatter.Deserialize(stream);*
            */

            messages = Serializer.Deserialize<Dictionary<int, LinkedList<Message>>>(stream);
        }

        public Message[] GetMessagesFrom(UsersData.User user)
        {
            return messages.ContainsKey(user.Id) ? messages[user.Id].ToArray() : null;
        }

        public Message[] GetAllMessages(int maxforeach = 10)
        {
            return messages.SelectMany(m => m.Value.Reverse().Take(maxforeach).Reverse()).OrderBy(m => m.DateTime).ToArray();
        }

        public void NewMessage(int id, DateTime date, MessageDirection dir, string text, string attachments = null)
        {
            NewMessage(new Message(id, date, dir, text, attachments));
        }

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

    public class UserRemovedEventArgs : EventArgs
    {
        public int Id { get; private set; }
        public UserRemovedEventArgs(int id)
        {
            Id = id;
        }
    }

    public class UserAddedEventArgs : EventArgs
    {
        public UsersData.User NewUser { get; private set; }

        public UserAddedEventArgs(UsersData.User New)
        {
            NewUser = New;
        }
    }

    public class UserChangedEventArgs : EventArgs
    {
        public int Id { get; private set; }
        public string Param { get; private set; }

        public UserChangedEventArgs(int id, string param)
        {
            Id = id;
            Param = param;
        }
    }
}
