using System;

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
}
