using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Help
{
    public class Help : BotExtension
    {
        public override string Name => "Help";

        public override string Description => "Помощь по командам";

        public override Priority Priority => Priority.Medium;

        public override bool StopAfterProcessed => true;

        public override Version Version => new Version(1, 0);

        public override void Load()
        {
            return;
        }

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (Regex.IsMatch(text, @"(\W|^)помощь(\W|$)"))
            {
                vk.SendMessage(userid, "ПРАЙС - прайс лист художника\n" + // сделяль
                                                                   //"АРТ-СТАТУС - текущий арт-статус\n" +
                                "ПРАВИЛА - правила данной группы\n" + // сделяль
                                "ПРИМЕРЫ - примеры артов\n" + // сделяль
                                "РЕФ/РЕФКИ - персонаж(и) художника\n" + // сделяль
                                "ФА - ссылка на FurAffinity\n" + // сделяль
                                "ДА - ссылка на DeviantArt\n" + // сделаль
                                "УЧИТЬ - режим обучения бота\n" +
                                "\nБОТ - информация о боте"); // сделяль
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
