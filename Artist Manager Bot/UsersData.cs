using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Artist_Manager_Bot
{
    public sealed class UsersData
    {
        private Dictionary<string, object> DefaultValues = new Dictionary<string, object>();
        private HashSet<User> _Users = new HashSet<User>();

        /// <summary> Вызов события UserChanged </summary>
        private void RaiseChangedEvent(User user, string field) =>
            UserChanged?.Invoke(this, new UserChangedEventArgs(user.Id, field));

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
        
        public User this[int id] => _Users.FirstOrDefault(u => u.Id == id);

        public User this[string name] => _Users.FirstOrDefault(u => u.Name == name);

        public void Save(Stream stream) => Serializer.Serialize(stream, _Users);

        public void Load(Stream stream)
        {
            _Users = Serializer.Deserialize<HashSet<User>>(stream);

            foreach (var user in _Users)
            {
                user.SetUserDataAndRaiseEvent(RaiseChangedEvent, this);
                UserAdded?.Invoke(this, new UserAddedEventArgs(user));
            }
        }
    }
}
