using System;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace FlooxOC
{
    [Serializable]
    public class AppInfo
    {
        public string Name { get; set; } = "Новое приложение";
        public string ExecutablePath { get; set; } = "";
        public string IconPath { get; set; } = "";
        public string WorkingDirectory { get; set; } = "";
        public string Arguments { get; set; } = "";
        public string Category { get; set; } = "Приложения";
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public AppInfo()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string GetDisplayName()
        {
            return System.IO.Path.GetFileNameWithoutExtension(ExecutablePath);
        }
    }
}