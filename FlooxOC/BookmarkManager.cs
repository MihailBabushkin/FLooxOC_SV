using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace FlooxOC
{
    public static class BookmarkManager
    {
        private static string DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Floox OC. Home version", "Bookmarks"
        );
        private static string BookmarksFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Floox OC. Home version", "bookmarks.json"
        );
        private static string IconsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Floox OC. Home version", "BookmarkIcons"
        );

        private static List<WebBookmark> bookmarks = new List<WebBookmark>();

        static BookmarkManager()
        {
            Directory.CreateDirectory(DataPath);
            Directory.CreateDirectory(IconsFolder);
            LoadBookmarks();
        }

        public static List<WebBookmark> GetBookmarks()
        {
            return bookmarks;
        }

        public static void AddBookmark(WebBookmark bookmark)
        {
            if (!string.IsNullOrEmpty(bookmark.IconPath) && File.Exists(bookmark.IconPath))
            {
                string iconName = $"{bookmark.Id}.png";
                string destPath = Path.Combine(IconsFolder, iconName);
                File.Copy(bookmark.IconPath, destPath, true);
                bookmark.IconPath = destPath;
            }

            bookmarks.Add(bookmark);
            SaveBookmarks();
        }

        public static void RemoveBookmark(string bookmarkId)
        {
            var bookmark = bookmarks.Find(b => b.Id == bookmarkId);
            if (bookmark != null)
            {
                if (File.Exists(bookmark.IconPath))
                    File.Delete(bookmark.IconPath);

                bookmarks.Remove(bookmark);
                SaveBookmarks();
            }
        }

        public static void UpdateBookmark(WebBookmark updated)
        {
            var existing = bookmarks.Find(b => b.Id == updated.Id);
            if (existing != null)
            {
                existing.Name = updated.Name;
                existing.Url = updated.Url;

                // Обновляем иконку
                if (!string.IsNullOrEmpty(updated.IconPath) && File.Exists(updated.IconPath))
                {
                    // Удаляем старую иконку
                    if (File.Exists(existing.IconPath))
                        File.Delete(existing.IconPath);

                    string iconName = $"{existing.Id}.png";
                    string destPath = Path.Combine(IconsFolder, iconName);
                    File.Copy(updated.IconPath, destPath, true);
                    existing.IconPath = destPath;
                }

                SaveBookmarks();
            }
        }

        public static Image LoadBookmarkIcon(WebBookmark bookmark)
        {
            if (!string.IsNullOrEmpty(bookmark.IconPath) && File.Exists(bookmark.IconPath))
            {
                try
                {
                    return Image.FromFile(bookmark.IconPath);
                }
                catch { return null; }
            }
            return null;
        }

        public static Image GenerateFaviconFromUrl(string url)
        {
            try
            {
                // Пытаемся загрузить favicon с сайта
                Uri uri = new Uri(url);
                string faviconUrl = $"https://www.google.com/s2/favicons?domain={uri.Host}&sz=64";

                using (var client = new System.Net.WebClient())
                {
                    byte[] data = client.DownloadData(faviconUrl);
                    using (var ms = new System.IO.MemoryStream(data))
                    {
                        return Image.FromStream(ms);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static void ClearAll()
        {
            bookmarks.Clear();
            SaveBookmarks();
        }

        private static void LoadBookmarks()
        {
            if (File.Exists(BookmarksFile))
            {
                try
                {
                    string json = File.ReadAllText(BookmarksFile);
                    bookmarks = JsonSerializer.Deserialize<List<WebBookmark>>(json) ?? new List<WebBookmark>();
                }
                catch
                {
                    bookmarks = new List<WebBookmark>();
                }
            }
            else
            {
                // Добавляем стандартные закладки
                bookmarks = new List<WebBookmark>();

                // Google
                var google = new WebBookmark
                {
                    Name = "Google",
                    Url = "https://www.google.com"
                };
                bookmarks.Add(google);

                // YouTube
                var youtube = new WebBookmark
                {
                    Name = "YouTube",
                    Url = "https://www.youtube.com"
                };
                bookmarks.Add(youtube);

                // GitHub
                var github = new WebBookmark
                {
                    Name = "GitHub",
                    Url = "https://www.github.com"
                };
                bookmarks.Add(github);

                SaveBookmarks();
            }
        }

        private static void SaveBookmarks()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(BookmarksFile));
                string json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(BookmarksFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения закладок: {ex.Message}");
            }
        }
    }
}