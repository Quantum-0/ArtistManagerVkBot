using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Info
{
    public class Info : BotExtension
    {
        public override string Name => "BotInfo";

        public override string Description => "Информация о боте";

        public override Priority Priority => Priority.Medium;

        public override bool StopAfterProcessed => true;

        public override Version Version => new Version(1, 0);

        public override void Load()
        {
            return;
        }

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (text.ToLower() == "бот")
            {
                vk.SendMessage(userid, "Твелв-бот\nБот разработан для группы https://vk.com/skid_gallery \nХудожник - https://vk.com/id87324758 (Skid)\nСоздатель бота - https://vk.com/id20108853 (Тш)\nЕсли хотите себе такого же бота в группу, можете написать мне :з (https://vk.com/id20108853)");
                return ProcessResult.Processed;
            }
            return ProcessResult.Skipped;
        }

        public override void Save()
        {
            return;
        }
    }
}
