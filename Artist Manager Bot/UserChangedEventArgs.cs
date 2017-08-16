using System;

namespace Artist_Manager_Bot
{
    public sealed class UserChangedEventArgs : EventArgs
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
