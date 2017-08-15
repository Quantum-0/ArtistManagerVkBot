using System;

namespace BaseForBotExtension
{
    public class UsersGridUpdateEventArgs : EventArgs
    {
        public string ColumnName;
        public int UserId;
        public object Value;

        public UsersGridUpdateEventArgs(string column, int userid, object value)
        {
            ColumnName = column;
            UserId = userid;
            Value = value;
        }
    }
}