using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKInteraction;

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
                CallUsersGridUpdate(new UsersGridUpdateEventArgs(mycol.Name, userid, Guid.NewGuid()));
                vk.SendMessage(userid, "woof");
                return ProcessResult.Processed;
            }
            return ProcessResult.Skipped;
        }

        CustomColumn mycol = new CustomColumn() { DefaultValue = "0", Header = "Meows", Name = "MeowTest" };
        public override IEnumerable<CustomColumn> GetUsersGridColumns() => new [] { mycol };
    }
}
