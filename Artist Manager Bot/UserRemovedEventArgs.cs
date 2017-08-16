using System;

namespace Artist_Manager_Bot
{
    public sealed class UserRemovedEventArgs : EventArgs
    {
        public int Id { get; private set; }
        public UserRemovedEventArgs(int id)
        {
            Id = id;
        }
    }
}
