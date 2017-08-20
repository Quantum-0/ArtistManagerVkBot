using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Examples
{
    public class Examples : BotExtension
    {
        private Random rnd = new Random();

        public override string Name => "Examples";

        public override string Description => "Присылает примеры артов";

        public override Priority Priority => Priority.Medium;

        public override bool StopAfterProcessed => true;

        public override Version Version => new Version(1, 0);

        public override void Load()
        {
            return;
        }

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (Regex.IsMatch(text, @"(\W|^)примеры?(\W|$)"))
            {
                var imgs = vk.GetAlbumPictures(239672159);
                vk.SendMessage(userid, "", imgs.OrderBy(a => rnd.Next()).Take(3));
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
