using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace References
{
    public class References : BotExtension
    {
        public override string Name => "References";

        public override string Description => "Высылание рефок на персов пользователю";

        public override Priority Priority => Priority.Medium;

        public override bool StopAfterProcessed => true;

        public override Version Version => new Version(1, 0);

        public override void Load()
        {
            return;
        }

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (Regex.IsMatch(text, @"(\W|^)((тебя|тебе) нарисов(ать|ую)|нарисов(ать|ую) (тебя|тебе))(\W|$)"))
            {
                var photos = vk.GetAlbumPictures(214672056);
                vk.SendMessage(userid, "Нарисовать? Меня? Конечно. Вот мой реф:", photos.Take(1));
                return ProcessResult.Processed;
            }

            if (Regex.IsMatch(text, @"(\W|^)(рефы|рефки|персы|персонажи)(\W|$)"))
            {
                var photos = vk.GetAlbumPictures(214672056);
                vk.SendMessage(userid, "", photos);
                return ProcessResult.Processed;
            }

            if (Regex.IsMatch(text, @"(\W|^)(реф.?|перс|персонаж)(\W|$)"))
            {
                var photos = vk.GetAlbumPictures(214672056);
                vk.SendMessage(userid, "", photos.Take(1));
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
