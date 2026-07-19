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
            "Floox OC. Home Version", "Apps"
        );
        private static string AppsFile = Path.Combine(DataPath, "apps.json");
        private static string IconsFolder = Path.Combine(DataPath, "Icons");
        private static List<AppInfo> apps = new List<AppInfo>();
        private static Form1 mainForm; // Ссылка на главную форму

        static AppManager()
        {
            Directory.CreateDirectory(DataPath);
            Directory.CreateDirectory(IconsFolder);
            LoadApps();
        }

        // ===== УСТАНОВКА ГЛАВНОЙ ФОРМЫ =====
        public static void SetMainForm(Form1 form)
        {
            mainForm = form;
        }

        public static List<AppInfo> GetApps()
        {
            return apps;
        }

        public static void AddApp(AppInfo app)
        {
            if (!string.IsNullOrEmpty(app.ExecutablePath) && File.Exists(app.ExecutablePath))
            {
                try
                {
                    Icon icon = Icon.ExtractAssociatedIcon(app.ExecutablePath);
                    if (icon != null)
                    {
                        Bitmap bitmap = icon.ToBitmap();
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
                    Console.WriteLine($"Ошибка извлечения иконки: {ex.Message}");
                }
            }

            if (string.IsNullOrEmpty(app.IconPath))
            {
                string iconName = $"{app.Id}.png";
                string destPath = Path.Combine(IconsFolder, iconName);

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

        // ===== ЗАПУСК ПРИЛОЖЕНИЯ ВО ВНУТРЕННЕМ ОКНЕ =====
        public static bool LaunchApp(AppInfo app)
        {
            if (mainForm == null)
            {
                MessageBox.Show("Главное окно не найдено!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(app.ExecutablePath) || !File.Exists(app.ExecutablePath))
            {
                MessageBox.Show($"Файл не найден:\n{app.ExecutablePath}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                // Создаём внутреннее окно для приложения
                CustomWindow window = new CustomWindow(app.Name);
                window.Size = new Size(800, 600);
                window.MinimumSize = new Size(400, 300);

                // Создаём панель для встраивания приложения
                Panel hostPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = CustomWindow.DefaultBackground
                };

                // Запускаем процесс и встраиваем его в панель
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = app.ExecutablePath,
                    Arguments = app.Arguments,
                    WorkingDirectory = string.IsNullOrEmpty(app.WorkingDirectory) ?
                        Path.GetDirectoryName(app.ExecutablePath) : app.WorkingDirectory,
                    UseShellExecute = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                };

                System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);

                if (process != null)
                {
                    // Даём процессу время на запуск
                    System.Threading.Thread.Sleep(500);

                    // Встраиваем окно процесса в наш hostPanel
                    IntPtr handle = process.MainWindowHandle;
                    if (handle != IntPtr.Zero)
                    {
                        // Устанавливаем родительское окно
                        SetParent(handle, hostPanel.Handle);

                        // Разворачиваем на весь hostPanel
                        ShowWindow(handle, 3); // SW_MAXIMIZE
                    }
                    else
                    {
                        // Если не удалось получить handle, ждём ещё
                        System.Threading.Thread.Sleep(1000);
                        handle = process.MainWindowHandle;
                        if (handle != IntPtr.Zero)
                        {
                            SetParent(handle, hostPanel.Handle);
                            ShowWindow(handle, 3);
                        }
                    }
                }

                window.ContentControl = hostPanel;
                mainForm.Controls.Add(window);
                window.BringToFront();

                // Обработка закрытия окна
                window.Closed += (s, e) =>
                {
                    try
                    {
                        if (process != null && !process.HasExited)
                        {
                            process.Kill();
                            process.Dispose();
                        }
                    }
                    catch { }
                };

                window.Minimized += (s, e) => mainForm.AddTaskbarButton(app.Name, window);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ===== ИМПОРТЫ WINAPI ДЛЯ ВСТРАИВАНИЯ ОКОН =====
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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

        public static Image ExtractIconFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    if (filePath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        string targetPath = ResolveShortcut(filePath);
                        if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                        {
                            Icon icon = Icon.ExtractAssociatedIcon(targetPath);
                            if (icon != null)
                                return icon.ToBitmap();
                        }
                    }

                    Icon exeIcon = Icon.ExtractAssociatedIcon(filePath);
                    if (exeIcon != null)
                        return exeIcon.ToBitmap();
                }
            }
            catch { }
            return null;
        }

        public static string ResolveShortcut(string shortcutPath)
        {
            try
            {
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

        public static Image GetIconForFile(string filePath)
        {
            Image icon = ExtractIconFromFile(filePath);

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