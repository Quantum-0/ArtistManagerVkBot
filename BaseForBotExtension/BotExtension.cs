using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VKInteraction;

namespace BaseForBotExtension
{
    public interface IBotExtensionInfo
    {
        string Name { get; }
        string Author { get; }
        string Link { get; }
        Version Version { get; }
        string Filename { get; }
        string Description { get; }
        Priority Priority { get; }
        bool Enabled { get; set; }
    }
    public abstract class BotExtension : IBotExtensionInfo
    {
        public VK vk { get; set; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract Priority Priority { get; }
        public abstract bool StopAfterProcessed { get; }
        public string Filename => this.GetType().Assembly.GetName().Name + ".dll";
        public string ConfigFile => "Modules\\" + this.GetType().Assembly.GetName().Name + ".cfg";
        public bool Enabled { get; set; } = true;
        public virtual string Author => "Quantum0";
        public virtual string Link => "https://vk.com/id20108853";
        public abstract Version Version { get; }
        public abstract void Load();
        public abstract void Save();

        public abstract ProcessResult ProcessMessage(int userid, string text);
        public virtual IEnumerable<CustomColumn> GetUsersGridColumns() => Enumerable.Empty<CustomColumn>();
        public virtual IEnumerable<Form> GetCustomTabs() => Enumerable.Empty<Form>();
        // custom column info - default value + name + title

        public event EventHandler<UsersGridUpdateEventArgs> UsersGridUpdate;
        protected void CallUsersGridUpdate(UsersGridUpdateEventArgs e)
        {
            UsersGridUpdate?.Invoke(this, e);
        }
    }

    public enum Priority
    {
        Lowest,
        Low,
        Medium,
        High,
        Highest
    }
    public enum ProcessResult
    {
        Skipped,
        Processed,
        StopProcessingNext
    }
    public struct CustomColumn
    {
        public string Name;
        public string Header;
        public string DefaultValue;
    }
}
