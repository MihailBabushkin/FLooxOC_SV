using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace FlooxOC
{
    public static class AppManager
    {
        private static string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Floox OC. Home version", "Apps"
        );
        private static string AppsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Floox OC. Home version", "apps.json"
        );
        private static string IconsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Floox OC. Home version", "Icons"
        );

        private static List<AppInfo> apps = new List<AppInfo>();

        static AppManager()
        {
            // Создаём папки
            Directory.CreateDirectory(AppDataPath);
            Directory.CreateDirectory(IconsFolder);
            LoadApps();
        }

        public static List<AppInfo> GetApps()
        {
            return apps;
        }

        public static void AddApp(AppInfo app)
        {
            // Копируем иконку в папку приложения
            if (!string.IsNullOrEmpty(app.IconPath) && File.Exists(app.IconPath))
            {
                string iconName = $"{app.Id}.png";
                string destPath = Path.Combine(IconsFolder, iconName);
                File.Copy(app.IconPath, destPath, true);
                app.IconPath = destPath;
            }

            apps.Add(app);
            SaveApps();
        }

        public static void RemoveApp(string appId)
        {
            var app = apps.Find(a => a.Id == appId);
            if (app != null)
            {
                // Удаляем иконку
                if (File.Exists(app.IconPath))
                    File.Delete(app.IconPath);

                apps.Remove(app);
                SaveApps();
            }
        }

        public static bool LaunchApp(AppInfo app)
        {
            if (string.IsNullOrEmpty(app.ExecutablePath) || !File.Exists(app.ExecutablePath))
            {
                MessageBox.Show($"Файл не найден:\n{app.ExecutablePath}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = app.ExecutablePath,
                    Arguments = app.Arguments,
                    WorkingDirectory = string.IsNullOrEmpty(app.WorkingDirectory) ?
                        Path.GetDirectoryName(app.ExecutablePath) : app.WorkingDirectory,
                    UseShellExecute = true
                };

                System.Diagnostics.Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static void LoadApps()
        {
            if (File.Exists(AppsFile))
            {
                try
                {
                    string json = File.ReadAllText(AppsFile);
                    apps = JsonSerializer.Deserialize<List<AppInfo>>(json) ?? new List<AppInfo>();
                }
                catch
                {
                    apps = new List<AppInfo>();
                }
            }
            else
            {
                // Добавляем демо-приложение
                apps = new List<AppInfo>();
                SaveApps();
            }
        }

        private static void SaveApps()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(AppsFile));
                string json = JsonSerializer.Serialize(apps, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(AppsFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        public static void ClearAll()
        {
            apps.Clear();
            SaveApps();
        }

        public static Image LoadAppIcon(AppInfo app)
        {
            if (!string.IsNullOrEmpty(app.IconPath) && File.Exists(app.IconPath))
            {
                try
                {
                    return Image.FromFile(app.IconPath);
                }
                catch { return null; }
            }
            return null;
        }

        // Получаем иконку из файла .exe
        public static Image ExtractIconFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                    if (icon != null)
                        return icon.ToBitmap();
                }
            }
            catch { }
            return null;
        }
    }
}