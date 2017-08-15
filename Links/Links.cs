using BaseForBotExtension;
using System.Text.RegularExpressions;
using System;

namespace Links
{
    public class Links : BotExtension
    {
        public override string Name => "FA/dA Links";

        public override string Description => "Отправляет ссылку на ФА/ДА";

        public override Priority Priority => Priority.Low;

        public override bool StopAfterProcessed => false;

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (Regex.IsMatch(text, @"(\W|^)(fa|фа)(\W|$)"))
            {
                vk.SendMessage(userid, SkidFA);
                return ProcessResult.Processed;
            }

            if (Regex.IsMatch(text, @"(\W|^)(da|да)(\W|$)"))
            {
                vk.SendMessage(userid, SkidDA);
                return ProcessResult.Processed;
            }

            return ProcessResult.Skipped;
        }

        public override void Load()
        {
            return;
        }

        public override void Save()
        {
            return;
        }

        private string SkidFA => "http://www.furaffinity.net/user/aseniyaaa";
        private string SkidDA => "http://mr-skid.deviantart.com/";

        public override Version Version => new Version(1, 0);
    }
}
