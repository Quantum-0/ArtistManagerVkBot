using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

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

        public override void Load()
        {
            using (var fs = new FileStream(ConfigFile, FileMode.Open))
            {
                //IFormatter formatter = new BinaryFormatter();
                var serializer = new XmlSerializer(typeof(List<int>));
                TurnedOff = (List<int>)serializer.Deserialize(fs);
                //TurnedOff = (List<int>)formatter.Deserialize(fs);
            }
        }

        public override void Save()
        {
            using (var fs = new FileStream(ConfigFile, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(List<int>));
                serializer.Serialize(fs, TurnedOff);
            }
        }

        //[Serializable]
        List<int> TurnedOff = new List<int>();
    }
}
