using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using VKInteraction;

namespace Artist_Manager_Bot
{
    public partial class Form1 : Form
    {
        Controller Controller = new Controller();
        string hiddenToken;

        public void SubscribeAllForLogging()
        {
            Controller.MessageReceived += (s, e) => Log("Пришло сообщение от " + e.Message.Id + " с текстом: " + e.Message.Text);
            Controller.MessageSended += (s, e) => Log("Отправлен ответ " + e.Message.Id + " c текстом:" + e.Message.Text);
            Controller.MessageProcessed += (s, e) => Log("Сообщение обработано модулем " + e.ProcessedBy.Name);
            Controller.MessageProcessing += (s, e) => Log("Сообщение передано в обработку модулю " + e.ProcessedBy.Name);
            Controller.Users.UserAdded += (s, e) => Log($"Добавлен пользователь id{e.NewUser.Id} ({e.NewUser.Name})");
            Controller.Users.UserChanged += (s, e) => Log($"Данные пользователя id{e.Id} изменены");
            Controller.MessageSkipped += (s, e) => Log($"Модуль {e.ProcessedBy.Name} пропустил сообщение");
            Controller.ExtensionStopProcessing += (s, e) => Log($"Модуль {e.ProcessedBy.Name} прервал дальнейшую обработку сообщения");
        }

        public Form1()
        {
            InitializeComponent();
            Controller.LoadDLLs();
            Controller.Users.UserAdded += (s, e) => Invoke((Action)(() => { Users_UserAdded(s, e); }));
            Controller.Users.UserRemoved += (s, e) => Invoke((Action)(() => { Users_UserRemoved(s, e); }));
            Controller.Users.UserChanged += (s, e) => Invoke((Action)(() => { Users_UserChanged(s, e); })); 
            Controller.MessageReceived += (s, e) => Invoke((Action)(() => { Messages_Received(s, e); }));
            Controller.MessageSended += (s, e) => Invoke((Action)(() => { Messages_Sended(s, e); }));
            SubscribeAllForLogging();
            listBox1.SelectedIndex = 0;
            //Controller.Users.UserChanged += Users_UserChanged;

            if (File.Exists("SavedAuth.txt"))
            {
                var authdata = File.ReadAllLines("SavedAuth.txt");
                textBoxGroupId.Text = authdata[1];
                hiddenToken = authdata[0];
                tokenTextBox.Text = new string('*', hiddenToken.Length);
            }
            listBoxModules.Items.AddRange(Controller.GetExtensions());
            foreach (var customcolumn in Controller.CustomColumns)
            {
                dataGridUsers.Columns.Add(customcolumn.Name, customcolumn.Header);
            }
        }

        private void Messages_Received(object sender, MessageEventArgs e)
        {
            if (listBox1.SelectedItem?.ToString() == Controller.Users[e.Message.Id].Name || listBox1.SelectedIndex == 0)
                AddMessageToRichEditMessages(e.Message);
        }

        private void Messages_Sended(object sender, MessageEventArgs e)
        {
            if (listBox1.SelectedItem?.ToString() == Controller.Users[e.Message.Id].Name || listBox1.SelectedIndex == 0)
                AddMessageToRichEditMessages(e.Message);
        }

        private void Users_UserChanged(object sender, UserChangedEventArgs e)
        {
            for (int i = 0; i < dataGridUsers.RowCount; i++)
                if (dataGridUsers[0, i].Value.ToString() == e.Id.ToString())
                {
                    dataGridUsers[e.Param, i].Value = Controller.Users[e.Id].GetParam(e.Param);
                    return;
                }
        }

        private void Users_UserRemoved(object sender, UserRemovedEventArgs e)
        {
            for (int i = 0; i < dataGridUsers.RowCount; i++)
                if (dataGridUsers[0, i].Value.ToString() == e.Id.ToString())
                {
                    dataGridUsers.Rows.RemoveAt(i);
                    return;
                }
        }

        private void Users_UserAdded(object sender, UserAddedEventArgs e)
        {
            dataGridUsers.RowCount++;
            dataGridUsers[0, dataGridUsers.RowCount-1].Value = e.NewUser.Id;
            dataGridUsers[1, dataGridUsers.RowCount-1].Value = e.NewUser.Name;
            for (int i = 2; i < dataGridUsers.ColumnCount; i++)
                dataGridUsers[i, dataGridUsers.RowCount - 1].Value = e.NewUser.GetParam(dataGridUsers.Columns[i].Name);

            listBox1.Items.Add(e.NewUser.Name);
        }

        private void listBoxModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxModules.SelectedIndex == -1)
            {
                textBoxModuleInfo.Text = "";
                checkBoxModuleEnabled.Enabled = false;
            }
            else
            {
                var extinfo = Controller.GetExtension(listBoxModules.SelectedIndex);
                textBoxModuleInfo.Text = $"Модуль:   {extinfo.Name}\r\nСборка:   {extinfo.Filename}\r\nПриоритет:   {extinfo.Priority}\r\nОписание:\r\n  {extinfo.Description}";
                checkBoxModuleEnabled.Enabled = true;
                checkBoxModuleEnabled.Checked = extinfo.Enabled;
            }
        }

        private void Log(string text)
        {
            Invoke((Action)(() =>
                textBoxLog.Text += $"[{DateTime.Now.ToLongTimeString()}] {text}\r\n"
            ));
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Controller.Autorize(hiddenToken, textBoxGroupId.Text);

            if (!Controller.Autorized)
            {
                textBoxGroupId.ForeColor = Color.Red;
                tokenTextBox.ForeColor = Color.Red;
            }

            Controller.Receiving = true;

            if (Controller.Receiving)
            {
                this.Text = "Artist Manager Bot - " + Controller.GroupName;
                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                textBoxGroupId.ReadOnly = true;
                tokenTextBox.ReadOnly = true;
                this.Focus();
                File.WriteAllLines("SavedAuth.txt", new[] { hiddenToken, textBoxGroupId.Text });
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Controller.Receiving = false;

            if (!Controller.Receiving)
            {
                this.Text = "Artist Manager Bot";
                buttonStart.Enabled = true;
                buttonStop.Enabled = false;
                textBoxGroupId.ReadOnly = false;
                tokenTextBox.ReadOnly = false;
            }
        }

        private void tokenAndGidTextBox_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            textBox.ForeColor = Color.Black;

            if (textBox.Text == "Token" || textBox.Text == "Group ID")
                textBox.Text = "";

            if (textBox == tokenTextBox)
            {
                textBox.Text = hiddenToken;
            }
        }

        private void tokenTextBox_Leave(object sender, EventArgs e)
        {
            hiddenToken = tokenTextBox.Text;
            tokenTextBox.Text = new string('*', hiddenToken.Length);
        }

        private void checkBoxModuleEnabled_CheckedChanged(object sender, EventArgs e)
        {
            //dataGridUsers
            //dataGridUsers.Columns[]
            Controller.GetExtension(listBoxModules.SelectedIndex).Enabled = checkBoxModuleEnabled.Checked;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            if (listBox1.SelectedIndex == 0)
            {
                var msgs = Controller.Messages.GetAllMessages(3);
                foreach (var msg in msgs)
                {
                    AddMessageToRichEditMessages(msg);
                }
            }
            else
            {
                var usr = Controller.Users[listBox1.SelectedItem.ToString()];
                var msgs = Controller.Messages.GetMessagesFrom(usr);
                if (msgs != null)
                foreach (var msg in msgs)
                {
                    AddMessageToRichEditMessages(msg);
                }
            }
        }

        private void AddMessageToRichEditMessages(VKInteraction.Message message)
        {
            richTextBox1.AppendText(Controller.Users[message.Id].Name + " (" + message.DateTime.ToShortTimeString() + ") :\r\n", message.Dir == MessageDirection.In ? Color.DarkRed : Color.DarkBlue, new Font(richTextBox1.Font, FontStyle.Bold));
            richTextBox1.AppendText(message.Text + "\r\n");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Controller.SaveAll();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Controller.LoadAll();

            foreach (var customtab in Controller.CustomTabs)
                AddNewTab(customtab);
        }
        
        public void AddNewTab(Form frm)
        {
            TabPage tab = new TabPage(frm.Text);
            tab.Size = tabControl.TabPages[0].Size;
            tab.UseVisualStyleBackColor = true;
            frm.TopLevel = false;
            frm.Parent = tab;
            frm.Visible = true;
            //tabControl.TabPages.Add(tab);
            tabControl.Controls.Add(tab);
            frm.Location = new Point(4, 22);
            frm.Size = tab.Size;
            tab.BackColor = this.BackColor;
            frm.Dock = DockStyle.Fill;
        }
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color, Font font)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.SelectionFont = font;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
