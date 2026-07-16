using FlooxOC;
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
        private static string DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FlooxOC. Home Version", "Apps"
        );
        private static string AppsFile = Path.Combine(DataPath, "apps.json");
        private static string IconsFolder = Path.Combine(DataPath, "Icons");
        private static List<AppInfo> apps = new List<AppInfo>();

        static AppManager()
        {
            Directory.CreateDirectory(DataPath);
            Directory.CreateDirectory(IconsFolder);
            LoadApps();
        }

        public static List<AppInfo> GetApps()
        {
            return apps;
        }

        public static void AddApp(AppInfo app)
        {
            // === ИЗВЛЕКАЕМ ИКОНКУ ИЗ .EXE ===
            if (!string.IsNullOrEmpty(app.ExecutablePath) && File.Exists(app.ExecutablePath))
            {
                try
                {
                    // Извлекаем иконку из exe
                    Icon icon = Icon.ExtractAssociatedIcon(app.ExecutablePath);
                    if (icon != null)
                    {
                        // Конвертируем в Bitmap
                        Bitmap bitmap = icon.ToBitmap();

                        // Сохраняем в папку
                        string iconName = $"{app.Id}.png";
                        string destPath = Path.Combine(IconsFolder, iconName);
                        bitmap.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
                        app.IconPath = destPath;

                        bitmap.Dispose();
                        icon.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    // Если не удалось извлечь иконку, используем стандартную
                    Console.WriteLine($"Ошибка извлечения иконки: {ex.Message}");
                }
            }

            // Если иконка не найдена — используем стандартную
            if (string.IsNullOrEmpty(app.IconPath))
            {
                string iconName = $"{app.Id}.png";
                string destPath = Path.Combine(IconsFolder, iconName);

                // Создаём стандартную иконку
                Bitmap defaultIcon = new Bitmap(40, 40);
                using (Graphics g = Graphics.FromImage(defaultIcon))
                {
                    g.Clear(Color.FromArgb(220, 220, 220));
                    using (Font font = new Font("Segoe UI", 20))
                    {
                        g.DrawString("📦", font, Brushes.Black, 5, 5);
                    }
                }
                defaultIcon.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
                defaultIcon.Dispose();
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
                // === БЕЗОПАСНОЕ УДАЛЕНИЕ ===
                try
                {
                    if (!string.IsNullOrEmpty(app.IconPath) && File.Exists(app.IconPath))
                    {
                        File.Delete(app.IconPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка удаления иконки: {ex.Message}");
                }

                apps.Remove(app);
                SaveApps();
            }
        }

        public static bool LaunchApp(AppInfo app)
        {
            MessageBox.Show($"1. LaunchApp вызван для: {app.Name}", "Отладка");

            if (string.IsNullOrEmpty(app.ExecutablePath) || !File.Exists(app.ExecutablePath))
            {
                MessageBox.Show($"2. Файл не найден:\n{app.ExecutablePath}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            MessageBox.Show($"3. Файл существует: {app.ExecutablePath}", "Отладка");

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

                MessageBox.Show($"4. Запускаем процесс...", "Отладка");
                System.Diagnostics.Process.Start(startInfo);
                MessageBox.Show($"5. Процесс запущен!", "Отладка");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"6. Ошибка запуска:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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

        // ====== ИЗВЛЕЧЕНИЕ ИКОНКИ ИЗ ЛЮБОГО ФАЙЛА ======
        public static Image ExtractIconFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Проверяем, является ли файл ярлыком (.lnk)
                    if (filePath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        // Извлекаем путь из ярлыка
                        string targetPath = ResolveShortcut(filePath);
                        if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                        {
                            // Извлекаем иконку из целевого файла
                            Icon icon = Icon.ExtractAssociatedIcon(targetPath);
                            if (icon != null)
                                return icon.ToBitmap();
                        }
                    }

                    // Обычный файл .exe
                    Icon exeIcon = Icon.ExtractAssociatedIcon(filePath);
                    if (exeIcon != null)
                        return exeIcon.ToBitmap();
                }
            }
            catch { }
            return null;
        }

        // ====== РАЗРЕШЕНИЕ ЯРЛЫКА (.lnk) ======
        public static string ResolveShortcut(string shortcutPath)
        {
            try
            {
                // Создаём объект Shell для работы с ярлыками
                dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                string target = shortcut.TargetPath;
                return target;
            }
            catch
            {
                return null;
            }
        }

        // ====== ПОЛУЧЕНИЕ ИКОНКИ ДЛЯ ДИАЛОГА ======
        public static Image GetIconForFile(string filePath)
        {
            // Пытаемся извлечь иконку
            Image icon = ExtractIconFromFile(filePath);

            // Если не удалось — создаём заглушку с первой буквой
            if (icon == null)
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                Bitmap bmp = new Bitmap(40, 40);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(220, 220, 220));
                    using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                    {
                        string firstChar = name.Length > 0 ? name[0].ToString().ToUpper() : "?";
                        g.DrawString(firstChar, font, Brushes.DarkGray, 10, 8);
                    }
                }
                icon = bmp;
            }

            return icon;
        }

        public static void SaveApps()
        {
            try
            {
                string json = JsonSerializer.Serialize(apps, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(AppsFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
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
                apps = new List<AppInfo>();
                SaveApps();
            }
        }
    }
}