using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnOff
{
    public class TurnOff : BotExtension
    {
        private CustomColumn TurnedOffColumn = new CustomColumn() { Name = "TurnedOff", Header = "Turned off", DefaultValue = "On" };

        public override string Name => "Turn Off";

        public override string Description => "Возможность отключения соощений от бота";

        public override Priority Priority => Priority.Highest;

        public override bool StopAfterProcessed => true;

        public override Version Version => new Version(1, 0);

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (text.ToLower() == "выкл")
            {
                if (!TurnedOff.Contains(userid))
                {
                    TurnedOff.Add(userid);
                    CallUsersGridUpdate(new UsersGridUpdateEventArgs("TurnedOff", userid, "Off"));
                    //vk.SendMessage(userid, "Бот отключен. Если хочешь чтоб бот снова отвечал пользователям, напиши ВКЛ");
                    return ProcessResult.Processed;
                }
            }

            if (text.ToLower() == "вкл")
            {
                if (TurnedOff.Contains(userid))
                {
                    TurnedOff.Remove(userid);
                    CallUsersGridUpdate(new UsersGridUpdateEventArgs("TurnedOff", userid, "On"));
                    vk.SendMessage(userid, "Бот включён и готов тебе ответить :з");
                    return ProcessResult.Processed;
                }
            }

            if (TurnedOff.Contains(userid))
                return ProcessResult.Processed;
            else
                return ProcessResult.Skipped;
        }

        public override IEnumerable<CustomColumn> GetUsersGridColumns() => new[] { TurnedOffColumn };

        List<int> TurnedOff = new List<int>();
    }
}
