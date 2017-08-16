using System;

namespace Artist_Manager_Bot
{
    public sealed class UserAddedEventArgs : EventArgs
    {
        public User NewUser { get; private set; }

        public UserAddedEventArgs(User New)
        {
            NewUser = New;
        }
    }
}
