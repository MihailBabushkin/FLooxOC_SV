using FlooxOC;
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
            "Floox OC. Home Version", "Bookmarks"
        );
        private static string BookmarksFile = Path.Combine(DataPath, "bookmarks.json");
        private static string IconsFolder = Path.Combine(DataPath, "Icons");
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
                try
                {
                    string iconName = $"{bookmark.Id}.png";
                    string destPath = Path.Combine(IconsFolder, iconName);
                    File.Copy(bookmark.IconPath, destPath, true);
                    bookmark.IconPath = destPath;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка копирования иконки: {ex.Message}");
                    bookmark.IconPath = "";
                }
            }

            bookmarks.Add(bookmark);
            SaveBookmarks();
        }

        public static void RemoveBookmark(string bookmarkId)
        {
            var bookmark = bookmarks.Find(b => b.Id == bookmarkId);
            if (bookmark != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(bookmark.IconPath) && File.Exists(bookmark.IconPath))
                    {
                        File.Delete(bookmark.IconPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка удаления иконки: {ex.Message}");
                }

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

                if (!string.IsNullOrEmpty(updated.IconPath) && File.Exists(updated.IconPath))
                {
                    try
                    {
                        if (File.Exists(existing.IconPath))
                            File.Delete(existing.IconPath);

                        string iconName = $"{existing.Id}.png";
                        string destPath = Path.Combine(IconsFolder, iconName);
                        File.Copy(updated.IconPath, destPath, true);
                        existing.IconPath = destPath;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обновления иконки: {ex.Message}");
                    }
                }

                SaveBookmarks();
            }
        }

        public static Image LoadBookmarkIcon(WebBookmark bookmark)
        {
            try
            {
                if (!string.IsNullOrEmpty(bookmark.IconPath) && File.Exists(bookmark.IconPath))
                {
                    return Image.FromFile(bookmark.IconPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки иконки: {ex.Message}");
            }
            return null;
        }

        public static Image GenerateFaviconFromUrl(string url)
        {
            try
            {
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
            try
            {
                if (Directory.Exists(IconsFolder))
                {
                    foreach (string file in Directory.GetFiles(IconsFolder))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }
            catch { }

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
                // ===== СТАНДАРТНЫЕ ЗАКЛАДКИ (ЗАМЕНЕНЫ) =====
                bookmarks = new List<WebBookmark>();

                // Карта сервера
                var map = new WebBookmark
                {
                    Name = "Карта сервера",
                    Url = "http://188.127.241.249:25824"
                };
                bookmarks.Add(map);

                // Информация о сервере
                var serverInfo = new WebBookmark
                {
                    Name = "RassvetCraft",
                    Url = "https://top-minecrafter.com/server/rassvetcraft/"
                };
                bookmarks.Add(serverInfo);

                // Google (оставляем для удобства)
                var google = new WebBookmark
                {
                    Name = "Google",
                    Url = "https://www.google.com"
                };
                bookmarks.Add(google);

                SaveBookmarks();
            }
        }

        public static void SaveBookmarks()
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