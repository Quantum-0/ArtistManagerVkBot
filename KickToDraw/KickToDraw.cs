using BaseForBotExtension;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KickToDraw
{
    public class KickToDraw : BotExtension
    {
        private const bool SCREENSHOT_ALLOWED = false;
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

            if (SCREENSHOT_ALLOWED && text.ToLower() == "чоделаеш, пакажи")
            {
                MakeScreenshot().Save("test.png");
                vk.SendImage(userid, "test.png");
            }

            return ProcessResult.Skipped;
        }

        private Bitmap MakeScreenshot()
        {
            Graphics graph = null;
            var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            graph = Graphics.FromImage(bmp);
            graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            return bmp;
        }

        public override void Save()
        {
        }
    }
}
