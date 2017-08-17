using BaseForBotExtension;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VKInteraction;

namespace Artist_Manager_Bot
{
    /// <summary> Контроллер, выполняющий базовую логику и взаимодействующий с модулями </summary>
    class Controller
    {
        /// <summary> Взаимодействие с ВК </summary>
        private VK vk = VK.Interaction;
        /// <summary> Взаимодействие со списком пользователей </summary>
        public UsersData Users { get; } = new UsersData();
        /// <summary> База данных сообщеней </summary>
        public MessagesDataBase Messages { get; } = new MessagesDataBase();

        /// <summary> Название группы </summary>
        public string GroupName => vk.GroupName;
        /// <summary> Успешна ли авторизация </summary>
        public bool Autorized => vk.Autorized;

        /// <summary> Загруженные модули </summary>
        private BotExtension[] Extensions;
        /// <summary> Собственные колонки, добавленные подгруженными модулями для таблицы пользователей </summary>
        public CustomColumn[] CustomColumns { get; private set; }
        /// <summary> Собственные вкладки с интерфейсом, подгруженные из модулей </summary>
        public System.Windows.Forms.Form[] CustomTabs { get; private set; }
        
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> MessageSended;
        public event EventHandler<MessageProcessedEventArgs> MessageProcessed;
        public event EventHandler<MessageProcessedEventArgs> MessageProcessing;
        public event EventHandler<MessageProcessedEventArgs> MessageSkipped;
        public event EventHandler<MessageProcessedEventArgs> ExtensionStopProcessing;

        /// <summary> Получение списка названий подгруженных модулей </summary>
        public string[] GetExtensions() => Extensions.Select(e => e.Name).ToArray();
        /// <summary> Получение информации о модуле </summary>
        public IBotExtensionInfo GetExtension(int id) => Extensions[id];

        private static string AssemblyDirectory
        {
            get
            {
                /*string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);*/
                return Directory.GetCurrentDirectory();
            }
        }

        private bool _Receiving = false;
        public bool Receiving
        {
            set
            {
                if (_Receiving == value)
                    return;

                if (value == true && Autorized == false)
                    return;

                _Receiving = value;
                if (_Receiving)
                    vk.StartReceiving();
                else
                    vk.StopReceiving();
            }
            get
            {
                return _Receiving;
            }
        }

        public Controller()
        {
            vk.MessageReceived += Vk_MessageReceived;
            vk.MessageSended += (s, e) => MessageSended?.Invoke(s, e);
        }

        // Обработка приходящих сообщений
        private void Vk_MessageReceived(object sender, MessageEventArgs e)
        {
            // Добавление в Users, если отсутствует
            if (!Users.Contains(e.Message.Id))
            {
                Task.Delay(300).Wait();
                var name = vk.GetUsername(e.Message.Id);
                if (name != null)
                    Users.AddUser(e.Message.Id, name, null);
            }
            
            // Вызов события о новом сообщении
            MessageReceived?.Invoke(this, e);

            // Пинаем каждый модуль чтоб он обработал сообщения
            for (int i = 0; i < Extensions.Length; i++)
            {
                var extension = Extensions[i];

                // Скипаем выключенные модули
                if (!extension.Enabled)
                    continue;

                // Передаём модулям сообщения для обработки
                MessageProcessing?.Invoke(this, new MessageProcessedEventArgs(extension));
                var res = extension.ProcessMessage(e.Message.Id, e.Message.Text);
                // Логирование и остановка обработки
                if (res == ProcessResult.Processed)
                {
                    MessageProcessed?.Invoke(this, new MessageProcessedEventArgs(extension));
                    if (extension.StopAfterProcessed)
                    {
                        ExtensionStopProcessing?.Invoke(this, new MessageProcessedEventArgs(extension));
                        return;
                    }
                }
                else if (res == ProcessResult.Skipped)
                    MessageSkipped?.Invoke(this, new MessageProcessedEventArgs(extension));
            }
        }
        
        /// <summary> Авторизация ВК </summary>
        public void Autorize(string token, string group) => vk.Autorize(token, group);

        /// <summary> Загрузка DLL модулей </summary>
        public void LoadDLLs()
        {
            var dir  = Directory.GetCurrentDirectory();
            // Находим все dll файлы в папке Modules, загружаем сборки, 
            //      вытаскиваем из них все типы, из них выделяем наследуемые от BotExtension,
            //      для каждого из них вызываем конструктор, приводим всё к базовому классу
            //      и сохраняем полученные экземпляры модулей в массив
            var extensions = Directory.GetFiles(Path.Combine(AssemblyDirectory, "Modules"), "*.dll")
                .Select(fname => Assembly.LoadFrom(fname))
                .SelectMany(asm => asm.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(BotExtension)))
                .Select(et => Activator.CreateInstance(et))
                .Cast<BotExtension>()
                .ToArray();

            // Передаём каждому модулю экземпляр VK чтоб они могли взаимодействовать вк напрямую
            //  и подписываемся на событие изменения данных в кастомных колонках таблицы пользователей
            foreach (var ext in extensions)
            {
                ext.vk = vk;
                ext.UsersGridUpdate += (s, e) => Users[e.UserId].SetParam(e.ColumnName, e.Value);
            }
            
            // Сортируем по убыванию приоритета, вытаскиваем кастомные колонки и вкладки
            Extensions = extensions.OrderByDescending(e => e.Priority).ToArray();
            CustomColumns = Extensions.SelectMany(e => e.GetUsersGridColumns()).ToArray();
            CustomTabs = Extensions.SelectMany(e => e.GetCustomTabs()).ToArray();

            // Устанавливаем дефолтные значения в кастомных колонках
            foreach (var cc in CustomColumns)
                Users.SetDefaultValue(cc.Name, cc.DefaultValue);
        }

        /// <summary> Сохранение всего </summary>
        public void SaveAll()
        {
            // Сохранение сообщений
            using(var fs = new FileStream("msgs.bin", FileMode.Create))
                Messages.Save(fs);

            // Сохранение пользователей
            using (var fs = new FileStream("usrs.bin", FileMode.Create))
                Users.Save(fs);

            // Вызов сохранения для всех модулей
            foreach (var ext in Extensions)
                ext.Save();
        }

        /// <summary> Загрузка всего </summary>
        public void LoadAll()
        {
            // Загрузка пользователей
            if (File.Exists("usrs.bin"))
                using (var fs = new FileStream("usrs.bin", FileMode.Open))
                    Users.Load(fs);

            // Загрузка сообщеней
            if (File.Exists("msgs.bin"))
                using (var fs = new FileStream("msgs.bin", FileMode.Open))
                    Messages.Load(fs);

            // Вызов загрузки данных для всех модулей
            foreach (var ext in Extensions)
                try
                {
                    ext.Load();
                }
                catch
                { }
        }
    }
}
