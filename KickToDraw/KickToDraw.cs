using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KickToDraw
{
    public class KickToDraw : BotExtension
    {
        public override string Name => "Kick";

        public override string Description => "Пинок";

        public override Priority Priority => Priority.High;

        public override bool StopAfterProcessed => false;

        public override Version Version => new Version(1, 0);

        public override void Load()
        {
        }

        public override ProcessResult ProcessMessage(int userid, string text)
        {
            if (text.ToLower() == "рисуй" || text.ToLower() == "*пнул рисовать*")
            {
                Console.Beep();
                var res = MessageBox.Show("Рисуй!", "Пинок рисовать", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (res == DialogResult.OK)
                    vk.SendMessage(userid, "Окей");
                else
                    vk.SendMessage(userid, "Не");

                return ProcessResult.Processed;
            }
            return ProcessResult.Skipped;
        }

        public override void Save()
        {
        }
    }
}
