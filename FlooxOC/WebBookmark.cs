using System;
using System.Drawing;
using System.Text.Json.Serialization;

namespace FlooxOC
{
    [Serializable]
    public class WebBookmark
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Новый сайт";
        public string Url { get; set; } = "https://www.google.com";
        public string IconPath { get; set; } = "";
        public string Category { get; set; } = "Закладки";
        public DateTime AddedDate { get; set; } = DateTime.Now;

        public WebBookmark()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string GetDisplayName()
        {
            return Name;
        }
    }
}