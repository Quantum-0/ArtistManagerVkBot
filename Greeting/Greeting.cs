using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Greeting
{
    public class Greeting : BotExtension
    {
        public override string Name => "Greetings";

        public override string Description => "Модуль, приветствующий новых собеседников бота";

        public override Priority Priority => Priority.High;

        public override bool StopAfterProcessed => false;

        public override Version Version => new Version(1, 0);

        public override void Load()
        {
            using (var sr = new StreamReader(ConfigFile))
            {
                var idsstr = sr.ReadToEnd();
                var ids = idsstr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var ids_int = ids.Select(id => int.Parse(id)).ToArray();
                foreach (var id in ids_int)
                {
                    Greeted.Add(id);
                }
            }
        }

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (Greeted.Contains(userid))
                return ProcessResult.Skipped;

            Greeted.Add(userid);
            vk.SendMessage(userid, "Здравствуйте.В данном сообществе на сообщения отвечает бот.Если не хотите получать сообщения от бота, напишите \"ВЫКЛ\". Чтобы получить список возможностей бота, напишите \"ПОМОЩЬ\"\nЕсли у вас есть предложения о том, какие функции необходимо добавить боту, пишите пожалуйста мне сюда - https://vk.com/id20108853");
            return ProcessResult.Processed;
        }

        public override void Save()
        {
            using (var sw = new StreamWriter(ConfigFile))
            {
                foreach (var id in Greeted)
                {
                    sw.Write(id.ToString() + ';');
                }
            }
        }

        private HashSet<int> Greeted = new HashSet<int>();
    }
}
