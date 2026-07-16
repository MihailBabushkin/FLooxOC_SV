using FlooxOC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace FlooxOC
{
    public class DesktopLayoutManager
    {
        private static string DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Floox OC. Home Version", "DesktopLayouts"
        );
        private static string CurrentLayoutFile = Path.Combine(DataPath, "current_layout.json");
        private static string LayoutsListFile = Path.Combine(DataPath, "layouts.json");

        public class IconLayout
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string Data { get; set; }
        }

        public class DesktopLayout
        {
            public string Name { get; set; }
            public DateTime CreatedDate { get; set; }
            public List<IconLayout> Icons { get; set; } = new List<IconLayout>();
        }

        public class LayoutsCollection
        {
            public List<string> LayoutNames { get; set; } = new List<string>();
        }

        static DesktopLayoutManager()
        {
            try
            {
                Directory.CreateDirectory(DataPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания папки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SaveCurrentLayout(Panel desktopPanel)
        {
            try
            {
                if (desktopPanel == null) return;

                var layout = new DesktopLayout
                {
                    Name = "Текущий",
                    CreatedDate = DateTime.Now
                };

                foreach (Control ctrl in desktopPanel.Controls)
                {
                    if (ctrl is DesktopIcon icon)
                    {
                        var iconLayout = new IconLayout
                        {
                            Id = icon.AppId ?? "",
                            Name = icon.GetText() ?? "Иконка",
                            Type = icon.Type ?? "app",
                            X = icon.Location.X,
                            Y = icon.Location.Y
                        };

                        if (icon.Type == "bookmark")
                        {
                            var bookmark = BookmarkManager.GetBookmarks().Find(b => b.Id == icon.AppId);
                            if (bookmark != null)
                                iconLayout.Data = bookmark.Url;
                        }
                        else if (icon.Type == "app")
                        {
                            var app = AppManager.GetApps().Find(a => a.Id == icon.AppId);
                            if (app != null)
                                iconLayout.Data = app.ExecutablePath;
                        }

                        layout.Icons.Add(iconLayout);
                    }
                }

                if (layout.Icons.Count == 0) return;

                string json = JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CurrentLayoutFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения макета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void RestoreCurrentLayout(Panel desktopPanel)
        {
            if (!File.Exists(CurrentLayoutFile)) return;

            try
            {
                string json = File.ReadAllText(CurrentLayoutFile);
                var layout = JsonSerializer.Deserialize<DesktopLayout>(json);
                if (layout == null || layout.Icons.Count == 0) return;

                foreach (var iconLayout in layout.Icons)
                {
                    foreach (Control ctrl in desktopPanel.Controls)
                    {
                        if (ctrl is DesktopIcon icon && icon.AppId == iconLayout.Id)
                        {
                            icon.SetPosition(new Point(iconLayout.X, iconLayout.Y));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка восстановления макета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SaveBackup(string name, Panel desktopPanel)
        {
            try
            {
                if (desktopPanel == null) return;

                var layout = new DesktopLayout
                {
                    Name = name,
                    CreatedDate = DateTime.Now
                };

                foreach (Control ctrl in desktopPanel.Controls)
                {
                    if (ctrl is DesktopIcon icon)
                    {
                        var iconLayout = new IconLayout
                        {
                            Id = icon.AppId ?? "",
                            Name = icon.GetText() ?? "Иконка",
                            Type = icon.Type ?? "app",
                            X = icon.Location.X,
                            Y = icon.Location.Y
                        };

                        if (icon.Type == "bookmark")
                        {
                            var bookmark = BookmarkManager.GetBookmarks().Find(b => b.Id == icon.AppId);
                            if (bookmark != null)
                                iconLayout.Data = bookmark.Url;
                        }
                        else if (icon.Type == "app")
                        {
                            var app = AppManager.GetApps().Find(a => a.Id == icon.AppId);
                            if (app != null)
                                iconLayout.Data = app.ExecutablePath;
                        }

                        layout.Icons.Add(iconLayout);
                    }
                }

                if (layout.Icons.Count == 0)
                {
                    MessageBox.Show("Нет иконок для сохранения!", "Информация");
                    return;
                }

                string fileName = $"{name.Replace(" ", "_")}.json";
                string filePath = Path.Combine(DataPath, fileName);
                string json = JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);

                UpdateLayoutsList(name);

                MessageBox.Show($"Макет '{name}' сохранён! Иконок: {layout.Icons.Count}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения бэкапа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void RestoreBackup(string name, Panel desktopPanel)
        {
            try
            {
                string fileName = $"{name.Replace(" ", "_")}.json";
                string filePath = Path.Combine(DataPath, fileName);

                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Бэкап '{name}' не найден!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string json = File.ReadAllText(filePath);
                var layout = JsonSerializer.Deserialize<DesktopLayout>(json);
                if (layout == null || layout.Icons.Count == 0)
                {
                    MessageBox.Show("Макет пуст!", "Информация");
                    return;
                }

                // Удаляем все иконки (кроме системных)
                List<Control> toRemove = new List<Control>();
                foreach (Control ctrl in desktopPanel.Controls)
                {
                    if (ctrl is DesktopIcon icon && icon.Type != "system")
                    {
                        toRemove.Add(ctrl);
                    }
                }
                foreach (var ctrl in toRemove)
                {
                    desktopPanel.Controls.Remove(ctrl);
                }

                // Восстанавливаем иконки из бэкапа
                foreach (var iconLayout in layout.Icons)
                {
                    Image iconImage = null;

                    if (iconLayout.Type == "bookmark")
                    {
                        var bookmark = BookmarkManager.GetBookmarks().Find(b => b.Id == iconLayout.Id);
                        if (bookmark != null)
                        {
                            iconImage = BookmarkManager.LoadBookmarkIcon(bookmark);
                            if (iconImage == null)
                                iconImage = BookmarkManager.GenerateFaviconFromUrl(bookmark.Url);
                        }
                    }
                    else if (iconLayout.Type == "app")
                    {
                        var app = AppManager.GetApps().Find(a => a.Id == iconLayout.Id);
                        if (app != null)
                            iconImage = AppManager.LoadAppIcon(app);
                    }

                    if (iconImage == null)
                    {
                        iconImage = CreateDefaultIcon();
                    }

                    var icon = new DesktopIcon(iconLayout.Name, iconImage, iconLayout.Type);
                    icon.AppId = iconLayout.Id;
                    icon.SetPosition(new Point(iconLayout.X, iconLayout.Y));

                    // === ОТКРЫТИЕ ПО КЛИКУ ===
                    icon.Click += (s, e) =>
                    {
                        if (icon.Type == "bookmark")
                        {
                            var bookmark = BookmarkManager.GetBookmarks().Find(b => b.Id == icon.AppId);
                            if (bookmark != null)
                            {
                                Form1 form = desktopPanel.FindForm() as Form1;
                                if (form != null)
                                    form.OpenBookmark(bookmark);
                            }
                        }
                        else if (icon.Type == "app")
                        {
                            var app = AppManager.GetApps().Find(a => a.Id == icon.AppId);
                            if (app != null)
                                AppManager.LaunchApp(app);
                        }
                    };

                    // === УДАЛЕНИЕ ===
                    icon.OnDelete += (s, e) =>
                    {
                        DialogResult result = MessageBox.Show($"Удалить '{icon.GetText()}'?", "Подтверждение",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            if (icon.Type == "bookmark")
                                BookmarkManager.RemoveBookmark(icon.AppId);
                            else if (icon.Type == "app")
                                AppManager.RemoveApp(icon.AppId);
                            desktopPanel.Controls.Remove(icon);
                        }
                    };

                    desktopPanel.Controls.Add(icon);
                }

                SaveCurrentLayout(desktopPanel);

                MessageBox.Show($"Макет '{name}' восстановлен! Иконок: {layout.Icons.Count}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка восстановления бэкапа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static List<string> GetLayoutsList()
        {
            List<string> layouts = new List<string>();

            if (File.Exists(LayoutsListFile))
            {
                try
                {
                    string json = File.ReadAllText(LayoutsListFile);
                    var collection = JsonSerializer.Deserialize<LayoutsCollection>(json);
                    if (collection != null)
                        layouts = collection.LayoutNames;
                }
                catch { }
            }

            return layouts;
        }

        public static void DeleteBackup(string name)
        {
            try
            {
                string fileName = $"{name.Replace(" ", "_")}.json";
                string filePath = Path.Combine(DataPath, fileName);

                if (File.Exists(filePath))
                    File.Delete(filePath);

                List<string> layouts = GetLayoutsList();
                if (layouts.Contains(name))
                {
                    layouts.Remove(name);
                    var collection = new LayoutsCollection { LayoutNames = layouts };
                    string json = JsonSerializer.Serialize(collection, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(LayoutsListFile, json);
                }

                MessageBox.Show($"Макет '{name}' удалён!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления бэкапа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Image CreateDefaultIcon()
        {
            Bitmap bmp = new Bitmap(40, 40);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(220, 220, 220));
                using (Font font = new Font("Segoe UI", 20))
                {
                    g.DrawString("📦", font, Brushes.Black, 5, 5);
                }
            }
            return bmp;
        }

        private static void UpdateLayoutsList(string name)
        {
            try
            {
                List<string> layouts = GetLayoutsList();
                if (!layouts.Contains(name))
                {
                    layouts.Add(name);
                    var collection = new LayoutsCollection { LayoutNames = layouts };
                    string json = JsonSerializer.Serialize(collection, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(LayoutsListFile, json);
                }
            }
            catch { }
        }
    }
}