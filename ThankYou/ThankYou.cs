using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThankYou
{
    public class ThankYou : BotExtension
    {
        private Random rnd = new Random();

        public override string Name => "ThankYou";

        public override string Description => "Ответ на благодарность";

        public override Priority Priority => Priority.Low;

        public override bool StopAfterProcessed => false;

        public override Version Version => new Version(1, 0);

        public override void Load()
        {
            return;
        }

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (Regex.IsMatch(text, @"(\W|^)(с?пасиб(о|ки)?|благодар(ю|ствую))(\W|$)"))
            {
                var r = rnd.NextDouble();
                if (r < 0.3)
                    vk.SendMessage(userid, "Пжалста :з");
                else if (r < 0.6)
                    vk.SendMessage(userid, "Не за что");
                else if (r < 0.9)
                    vk.SendMessage(userid, "Обращайся~");
                else
                    vk.SendMessage(userid, "Не стоит благодарности ^-^");
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
