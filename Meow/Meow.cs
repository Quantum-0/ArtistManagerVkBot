using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Meow
{
    public class Meow : BotExtension
    {
        public override string Name => "meow";

        public override string Description => "мяу\r\nМодуль от Шш :D";

        public override Priority Priority => Priority.Low;

        public override bool StopAfterProcessed => false;

        public override Version Version => new Version(1, 0);

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (text == "meow" || text == "мяу")
            {
                if (!meows.ContainsKey(userid))
                    meows.Add(userid, 0);

                meows[userid]++;
                CallUsersGridUpdate(new UsersGridUpdateEventArgs(mycol.Name, userid, meows[userid]));
                vk.SendMessage(userid, "woof " + meows[userid]);
                return ProcessResult.Processed;
            }
            return ProcessResult.Skipped;
        }

        public Dictionary<int, int> meows = new Dictionary<int, int>();

        CustomColumn mycol = new CustomColumn() { DefaultValue = "0", Header = "Meows", Name = "MeowTest" };
        public override IEnumerable<CustomColumn> GetUsersGridColumns() => new [] { mycol };

        public override void Load()
        {
            using (var fs = new FileStream(ConfigFile, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine().Split(':');
                        meows.Add(int.Parse(line.First()), int.Parse(line.Last()));
                    }
                }
            }
        }

        public override void Save()
        {
            using (var fs = new FileStream(ConfigFile, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    foreach (var meow in meows.ToList())
                    {
                        var line = meow.Key.ToString() + ':' + meow.Value.ToString();
                        sw.WriteLine(line);
                    }
                }
            }
        }
    }
}
