using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Artist_Manager_Bot
{
    [ProtoContract]
    public sealed class User
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
            return Id % 128;
        }
    }
}
