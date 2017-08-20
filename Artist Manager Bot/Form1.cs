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

        
        #region Общее

        /// <summary> Конструктор формы </summary>
        public Form1()
        {
            InitializeComponent();
            // Подгрузка модулей
            Controller.LoadDLLs();
            // Подписка на события изменяющие таблицу пользователей
            Controller.Users.UserAdded += (s, e) => Invoke((Action)(() => { Users_UserAdded(s, e); }));
            Controller.Users.UserRemoved += (s, e) => Invoke((Action)(() => { Users_UserRemoved(s, e); }));
            Controller.Users.UserChanged += (s, e) => Invoke((Action)(() => { Users_UserChanged(s, e); })); 
            // Подписка на события прихода и отправки сообщений для вывода во вкладке Messages
            Controller.MessageReceived += (s, e) => Invoke((Action)(() => { Messages_Received(s, e); }));
            Controller.MessageSended += (s, e) => Invoke((Action)(() => { Messages_Sended(s, e); }));
            // Подписка на всё для логирования
            SubscribeAllForLogging();
            listBoxDialogs.SelectedIndex = 0;

            // Автоматическое заполнение поля авторизации
            if (File.Exists("SavedAuth.txt"))
            {
                var authdata = File.ReadAllLines("SavedAuth.txt");
                textBoxGroupId.Text = authdata[1];
                hiddenToken = authdata[0];
                tokenTextBox.Text = new string('*', hiddenToken.Length);
            }

            // Получение списка модулей и добавление дополнительных колонок в таблицу
            listBoxModules.Items.AddRange(Controller.GetExtensions());
            foreach (var customcolumn in Controller.CustomColumns)
                dataGridUsers.Columns.Add(customcolumn.Name, customcolumn.Header);
        }

        /// <summary> Добавление формы в TabControl в виде вкладки </summary>
        private void AddNewTab(Form frm)
        {
            TabPage tab = new TabPage(frm.Text)
            {
                Size = tabControl.TabPages[0].Size,
                UseVisualStyleBackColor = true,
                BackColor = this.BackColor
            };
            frm.TopLevel = false;
            frm.Parent = tab;
            frm.Visible = true;
            //tabControl.TabPages.Add(tab);
            tabControl.Controls.Add(tab);
            frm.Location = new Point(4, 22);
            frm.Size = tab.Size;
            frm.Dock = DockStyle.Fill;
        }

        /// <summary> Сохранение всех данных при закрытии формы </summary>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Controller.SaveAll();
        }

        /// <summary> Загрузка пользователей, сообщений, данных модулей и добавление кастомных вкладок </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            Controller.LoadAll();

            foreach (var customtab in Controller.CustomTabs)
                AddNewTab(customtab);
        }

        #endregion


        #region Авторизация

        /// <summary> Запуск бота </summary>
        private void ButtonStart_Click(object sender, EventArgs e)
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

        /// <summary> Остановка бота </summary>
        private void ButtonStop_Click(object sender, EventArgs e)
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

        private void TokenAndGidTextBox_Enter(object sender, EventArgs e)
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

        private void TokenTextBox_Leave(object sender, EventArgs e)
        {
            hiddenToken = tokenTextBox.Text;
            tokenTextBox.Text = new string('*', hiddenToken.Length);
        }

        #endregion


        #region Вкладка Модули

        /// <summary> Просмотр информации о модулях </summary> 
        private void ListBoxModules_SelectedIndexChanged(object sender, EventArgs e)
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

        /// <summary> Включение и отключения модулей </summary>
        private void CheckBoxModuleEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Controller.GetExtension(listBoxModules.SelectedIndex).Enabled = checkBoxModuleEnabled.Checked;
        }

        #endregion


        #region Вкладка Messages

        /// <summary> Вывод приходящих сообщений во вкладке Messages </summary>
        private void Messages_Received(object sender, MessageEventArgs e)
        {
            if (listBoxDialogs.SelectedItem?.ToString() == Controller.Users[e.Message.Id].Name || listBoxDialogs.SelectedIndex == 0)
                AddMessageToRichEditMessages(e.Message);
        }

        /// <summary> Вывод отправленных сообщений во вкладке Messages </summary>
        private void Messages_Sended(object sender, MessageEventArgs e)
        {
            if (listBoxDialogs.SelectedItem?.ToString() == Controller.Users[e.Message.Id].Name || listBoxDialogs.SelectedIndex == 0)
                AddMessageToRichEditMessages(e.Message);
        }

        /// <summary> Вывод сообщений по выбору собеседника в списке диалогов </summary>
        private void ListBoxDialogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBoxMessages.Clear();
            VKInteraction.Message[] msgs;

            // Получение всех сообщений
            if (listBoxDialogs.SelectedIndex == 0)
            {
                msgs = Controller.Messages.GetAllMessages(5);
            }
            else // Получение сообщений конкретного пользователя
            {
                var usr = Controller.Users[listBoxDialogs.SelectedItem.ToString()];
                msgs = Controller.Messages.GetMessagesFrom(usr);
            }

            // Вывод сообщений
            foreach (var msg in msgs)
                AddMessageToRichEditMessages(msg);
        }

        /// <summary> Вывод сообщения в RichTextBoxMessages </summary>
        private void AddMessageToRichEditMessages(VKInteraction.Message message)
        {
            richTextBoxMessages.AppendText(Controller.Users[message.Id].Name + " (" + message.DateTime.ToShortTimeString() + ") :\r\n", message.Dir == MessageDirection.In ? Color.DarkRed : Color.DarkBlue, new Font(richTextBoxMessages.Font, FontStyle.Bold));
            richTextBoxMessages.AppendText(message.Text + "\r\n");
        }

        #endregion


        #region Вкладка Users

        /// <summary> Изменение пользователя </summary>
        private void Users_UserChanged(object sender, UserChangedEventArgs e)
        {
            for (int i = 0; i < dataGridUsers.RowCount; i++)
                if (dataGridUsers[0, i].Value.ToString() == e.Id.ToString())
                {
                    dataGridUsers[e.Param, i].Value = Controller.Users[e.Id].GetParam(e.Param);
                    return;
                }
        }

        /// <summary> Удаление пользователя </summary>
        private void Users_UserRemoved(object sender, UserRemovedEventArgs e)
        {
            for (int i = 0; i < dataGridUsers.RowCount; i++)
                if (dataGridUsers[0, i].Value.ToString() == e.Id.ToString())
                {
                    dataGridUsers.Rows.RemoveAt(i);
                    return;
                }
        }

        /// <summary> Добавление пользователя </summary>
        private void Users_UserAdded(object sender, UserAddedEventArgs e)
        {
            dataGridUsers.RowCount++;
            dataGridUsers[0, dataGridUsers.RowCount - 1].Value = e.NewUser.Id;
            dataGridUsers[1, dataGridUsers.RowCount - 1].Value = e.NewUser.Name;
            for (int i = 2; i < dataGridUsers.ColumnCount; i++)
                dataGridUsers[i, dataGridUsers.RowCount - 1].Value = e.NewUser.GetParam(dataGridUsers.Columns[i].Name);

            listBoxDialogs.Items.Add(e.NewUser.Name);
        }

        #endregion


        #region Логирование

        /// <summary> Подписка на события для логирования </summary>
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

        /// <summary> Логирования события </summary>
        private void Log(string text)
        {
            Invoke((Action)(() =>
                textBoxLog.Text += $"[{DateTime.Now.ToLongTimeString()}] {text}\r\n"
            ));
        }

        #endregion
    }

    public static class RichTextBoxExtensions
    {
        /// <summary>
        /// Расширенное добавление текста в текстовое поле
        /// </summary>
        /// <param name="text">Содержимое текста</param>
        /// <param name="color">Цвет текста</param>
        /// <param name="font">Шрифт</param>
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
