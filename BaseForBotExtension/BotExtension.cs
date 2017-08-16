using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VKInteraction;

namespace BaseForBotExtension
{
    /// <summary> Модуль расширения бота </summary>
    public abstract class BotExtension : IBotExtensionInfo
    {
        /// <summary> Точка взаимодействия с ВК </summary>
        public VK vk { get; set; }
        /// <summary> Название </summary>
        public abstract string Name { get; }
        /// <summary> Описание </summary>
        public abstract string Description { get; }
        /// <summary> Приоритет </summary>
        public abstract Priority Priority { get; }
        /// <summary> Необходимо остановить обработку сообщений после успешной обработки модулем </summary>
        public abstract bool StopAfterProcessed { get; }
        /// <summary> Файл сборки </summary>
        public string Filename => this.GetType().Assembly.GetName().Name + ".dll";
        /// <summary> Файл конфигурации бота для сохранения данных и настроек </summary>
        public string ConfigFile => "Modules\\" + this.GetType().Assembly.GetName().Name + ".cfg";
        /// <summary> Модуль активен </summary>
        public bool Enabled { get; set; } = true;
        /// <summary> Автор модуля </summary>
        public virtual string Author => "Quantum0";
        /// <summary> Ссылка на автора модуля или страницу загрузки </summary>
        public virtual string Link => "https://vk.com/id20108853";
        /// <summary> Версия модуля </summary>
        public abstract Version Version { get; }
        /// <summary> Загрузка данных </summary>
        public abstract void Load();
        /// <summary> Сохранение данных </summary>
        public abstract void Save();

        /// <summary> Обработка сообщения </summary>
        public abstract ProcessResult ProcessMessage(int userid, string text);
        /// <summary> Список дополнительных колонок добавляемых модулем </summary>
        public virtual IEnumerable<CustomColumn> GetUsersGridColumns() => Enumerable.Empty<CustomColumn>();
        /// <summary> Список дополнительных вкладок добавляемых модулем </summary>
        public virtual IEnumerable<Form> GetCustomTabs() => Enumerable.Empty<Form>();

        /// <summary> Событе обновления данных о пользователе в дополнительной колонке, добавленной модулем </summary>
        public event EventHandler<UsersGridUpdateEventArgs> UsersGridUpdate;
        /// <summary> Вызов события обновления колонки для наследников класса </summary>
        protected void CallUsersGridUpdate(UsersGridUpdateEventArgs e) => UsersGridUpdate?.Invoke(this, e); 
    }
}
