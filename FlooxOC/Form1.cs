using FlooxOC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FlooxOC
{
    public class Form1 : Form
    {
        private Panel desktopPanel;
        private Panel taskbar;
        private Label clockLabel;
        private Timer clockTimer;
        private Color savedWallpaperColor;

        public Form1()
        {
            this.Text = "MyOS 95";
            this.BackColor = Color.FromArgb(0, 128, 128);
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            LoadWallpaperColor();
            InitializeTaskbar();
            InitializeDesktop();
            InitializeDesktopContextMenu();
            LoadSavedItems();
        }

        private void InitializeTaskbar()
        {
            taskbar = new Panel();
            taskbar.Height = 55;  // ← Немного выше для информации
            taskbar.Dock = DockStyle.Bottom;
            taskbar.BackColor = Color.FromArgb(192, 192, 192);  // ← Классический серый Win95
            taskbar.BorderStyle = BorderStyle.Fixed3D;  // ← 3D рамка как в Win95
            taskbar.Name = "taskbar";
            taskbar.Padding = new Padding(3, 0, 3, 0);

            // ===== КНОПКА ПУСК (Win95 стиль) =====
            Button startBtn = new Button();
            startBtn.Text = " Пуск ";
            startBtn.Location = new Point(4, 6);
            startBtn.Size = new Size(75, 40);
            startBtn.FlatStyle = FlatStyle.Flat;
            startBtn.FlatAppearance.BorderSize = 1;
            startBtn.FlatAppearance.BorderColor = Color.FromArgb(128, 128, 128);
            startBtn.BackColor = Color.FromArgb(192, 192, 192);
            startBtn.ForeColor = Color.Black;
            startBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            startBtn.Cursor = Cursors.Hand;
            startBtn.TextAlign = ContentAlignment.MiddleCenter;

            startBtn.MouseEnter += (s, e) =>
            {
                startBtn.BackColor = Color.FromArgb(212, 208, 200);
                startBtn.FlatAppearance.BorderColor = Color.White;
            };
            startBtn.MouseLeave += (s, e) =>
            {
                startBtn.BackColor = Color.FromArgb(192, 192, 192);
                startBtn.FlatAppearance.BorderColor = Color.FromArgb(128, 128, 128);
            };
            startBtn.Click += (s, e) => ShowStartMenu();
            taskbar.Controls.Add(startBtn);

            // ===== ПАНЕЛЬ ЗАДАЧ =====
            Panel taskButtonsPanel = new Panel();
            taskButtonsPanel.Location = new Point(84, 6);
            taskButtonsPanel.Size = new Size(taskbar.Width - 240, 40);
            taskButtonsPanel.BackColor = Color.Transparent;
            taskButtonsPanel.Name = "taskButtonsPanel";
            taskbar.Controls.Add(taskButtonsPanel);

            // ===== ПАНЕЛЬ ИНФОРМАЦИИ + ЧАСЫ (Win95 стиль) =====
            Panel infoPanel = new Panel();
            infoPanel.Size = new Size(140, 40);
            infoPanel.Location = new Point(taskbar.Width - 145, 6);
            infoPanel.BackColor = Color.FromArgb(192, 192, 192);
            infoPanel.BorderStyle = BorderStyle.Fixed3D;
            infoPanel.Cursor = Cursors.Hand;

            // ДАТА (маленькая, над временем)
            Label dateLabel = new Label();
            dateLabel.Text = DateTime.Now.ToString("dd.MM.yyyy");
            dateLabel.Font = new Font("Segoe UI", 7);
            dateLabel.ForeColor = Color.Black;
            dateLabel.Location = new Point(2, 0);
            dateLabel.Size = new Size(134, 14);
            dateLabel.TextAlign = ContentAlignment.MiddleCenter;
            infoPanel.Controls.Add(dateLabel);

            // ВРЕМЯ (крупно)
            clockLabel = new Label();
            clockLabel.Text = DateTime.Now.ToString("HH:mm");
            clockLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            clockLabel.ForeColor = Color.Black;
            clockLabel.Location = new Point(2, 14);
            clockLabel.Size = new Size(134, 22);
            clockLabel.TextAlign = ContentAlignment.MiddleCenter;
            clockLabel.Name = "clockLabel";
            infoPanel.Controls.Add(clockLabel);

            // Клик по панели информации
            infoPanel.Click += (s, e) => ShowDateTimeInfo();
            dateLabel.Click += (s, e) => ShowDateTimeInfo();
            clockLabel.Click += (s, e) => ShowDateTimeInfo();

            taskbar.Controls.Add(infoPanel);

            // ===== ТАЙМЕР =====
            clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) =>
            {
                if (clockLabel != null)
                {
                    clockLabel.Text = DateTime.Now.ToString("HH:mm");
                    dateLabel.Text = DateTime.Now.ToString("dd.MM.yyyy");
                }
            };
            clockTimer.Start();

            this.Controls.Add(taskbar);

            this.Resize += (s, e) =>
            {
                if (taskbar != null)
                {
                    infoPanel.Location = new Point(taskbar.Width - 145, 6);
                    taskButtonsPanel.Width = taskbar.Width - 240;
                }
            };
        }


        private void ShowDateTimeInfo()
        {
            try
            {
                string uptime = GetSystemUptime();

                MessageBox.Show(
                    $"📅 {DateTime.Now.ToString("dd.MM.yyyy")}\n" +
                    $"📆 {GetDayOfWeek()}\n" +
                    $"⏰ {DateTime.Now.ToString("HH:mm:ss")}\n" +
                    $"\n" +
                    $"🕐 Время работы: {uptime}\n" +
                    $"💻 {Environment.MachineName}",
                    "Дата и время",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show(
                    $"📅 {DateTime.Now.ToString("dd.MM.yyyy")}\n" +
                    $"📆 {GetDayOfWeek()}\n" +
                    $"⏰ {DateTime.Now.ToString("HH:mm:ss")}",
                    "Дата и время",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }


        private string GetDayOfWeek()
        {
            string[] days = { "Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
            return days[(int)DateTime.Now.DayOfWeek];
        }


        private string GetSystemUptime()
        {
            try
            {
                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                return $"{uptime.Days}д {uptime.Hours}ч {uptime.Minutes}м";
            }
            catch
            {
                return "Недоступно";
            }
        }


        // ===== ОСТАЛЬНЫЕ МЕТОДЫ (без изменений) =====

        private void InitializeDesktop()
        {
            desktopPanel = new Panel();
            desktopPanel.Dock = DockStyle.Fill;
            desktopPanel.BackColor = Color.Transparent;
            desktopPanel.Name = "desktopPanel";
            this.Controls.Add(desktopPanel);

            int y = 20;

            DesktopIcon calcIcon = new DesktopIcon("Калькулятор", CreateIcon("🧮"), "system");
            calcIcon.Location = new Point(20, y);
            calcIcon.Click += (s, e) => OpenCalculator();
            desktopPanel.Controls.Add(calcIcon);
            y += 85;

            DesktopIcon notepadIcon = new DesktopIcon("Блокнот", CreateIcon("📝"), "system");
            notepadIcon.Location = new Point(20, y);
            notepadIcon.Click += (s, e) => OpenNotepad();
            desktopPanel.Controls.Add(notepadIcon);
            y += 85;

            DesktopIcon browserIcon = new DesktopIcon("Браузер", CreateIcon("🌐"), "system");
            browserIcon.Location = new Point(20, y);
            browserIcon.Click += (s, e) => OpenBrowser();
            desktopPanel.Controls.Add(browserIcon);
            y += 85;

            DesktopIcon settingsIcon = new DesktopIcon("Настройки", CreateIcon("⚙️"), "system");
            settingsIcon.Location = new Point(20, y);
            settingsIcon.Click += (s, e) => OpenSettings();
            desktopPanel.Controls.Add(settingsIcon);
            y += 85;

            DesktopIcon musicIcon = new DesktopIcon("Музыка", CreateIcon("🎵"), "system");
            musicIcon.Location = new Point(20, y);
            musicIcon.Click += (s, e) => OpenMusicPlayer();
            desktopPanel.Controls.Add(musicIcon);
        }

        private void InitializeDesktopContextMenu()
        {
            ContextMenuStrip desktopMenu = new ContextMenuStrip();

            ToolStripMenuItem addAppItem = new ToolStripMenuItem("➕ Добавить приложение");
            addAppItem.Click += (s, e) => AddExternalApp();
            desktopMenu.Items.Add(addAppItem);

            ToolStripMenuItem addBookmarkItem = new ToolStripMenuItem("🔖 Добавить закладку");
            addBookmarkItem.Click += (s, e) => AddBookmark();
            desktopMenu.Items.Add(addBookmarkItem);

            desktopMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem backupMenu = new ToolStripMenuItem("💾 Бэкапы рабочего стола");

            ToolStripMenuItem saveBackupItem = new ToolStripMenuItem("💾 Сохранить макет");
            saveBackupItem.Click += (s, e) => SaveDesktopLayout();
            backupMenu.DropDownItems.Add(saveBackupItem);

            ToolStripMenuItem restoreBackupItem = new ToolStripMenuItem("📂 Восстановить макет");
            restoreBackupItem.Click += (s, e) => RestoreDesktopLayout();
            backupMenu.DropDownItems.Add(restoreBackupItem);

            ToolStripMenuItem deleteBackupItem = new ToolStripMenuItem("🗑️ Удалить макет");
            deleteBackupItem.Click += (s, e) => DeleteDesktopLayout();
            backupMenu.DropDownItems.Add(deleteBackupItem);

            desktopMenu.Items.Add(backupMenu);

            desktopMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem refreshItem = new ToolStripMenuItem("⟳ Обновить");
            refreshItem.Click += (s, e) => RefreshDesktop();
            desktopMenu.Items.Add(refreshItem);

            desktopPanel.ContextMenuStrip = desktopMenu;
        }

        private void LoadSavedItems()
        {
            try
            {
                var apps = AppManager.GetApps();
                foreach (var app in apps)
                {
                    AddAppIcon(app);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки приложений: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                var bookmarks = BookmarkManager.GetBookmarks();
                foreach (var bookmark in bookmarks)
                {
                    AddBookmarkIcon(bookmark);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки закладок: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Image CreateIcon(string emoji)
        {
            Bitmap bmp = new Bitmap(40, 40);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Font font = new Font("Segoe UI", 24))
                {
                    g.DrawString(emoji, font, Brushes.Black, 2, 2);
                }
            }
            return bmp;
        }

        private int GetNextIconY()
        {
            int maxY = 20;
            foreach (Control ctrl in desktopPanel.Controls)
            {
                if (ctrl is DesktopIcon di)
                {
                    if (di.Location.Y + 85 > maxY)
                        maxY = di.Location.Y + 85;
                }
            }
            return maxY;
        }

        private void ShowStartMenu()
        {
            using (Form menu = new Form())
            {
                menu.Text = "Меню Пуск";
                menu.Size = new Size(250, 350);
                menu.StartPosition = FormStartPosition.CenterParent;
                menu.FormBorderStyle = FormBorderStyle.FixedDialog;
                menu.MaximizeBox = false;
                menu.MinimizeBox = false;
                menu.BackColor = Color.FromArgb(192, 192, 192);

                int y = 10;

                Button calcBtn = new Button
                {
                    Text = "🧮 Калькулятор",
                    Location = new Point(10, y),
                    Size = new Size(210, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 220, 220)
                };
                calcBtn.Click += (s, e) => { menu.Close(); OpenCalculator(); };
                menu.Controls.Add(calcBtn);
                y += 45;

                Button notepadBtn = new Button
                {
                    Text = "📝 Блокнот",
                    Location = new Point(10, y),
                    Size = new Size(210, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 220, 220)
                };
                notepadBtn.Click += (s, e) => { menu.Close(); OpenNotepad(); };
                menu.Controls.Add(notepadBtn);
                y += 45;

                Button browserBtn = new Button
                {
                    Text = "🌐 Браузер",
                    Location = new Point(10, y),
                    Size = new Size(210, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 220, 220)
                };
                browserBtn.Click += (s, e) => { menu.Close(); OpenBrowser(); };
                menu.Controls.Add(browserBtn);
                y += 45;

                Button settingsBtn = new Button
                {
                    Text = "⚙️ Настройки",
                    Location = new Point(10, y),
                    Size = new Size(210, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 220, 220)
                };
                settingsBtn.Click += (s, e) => { menu.Close(); OpenSettings(); };
                menu.Controls.Add(settingsBtn);
                y += 45;

                Button musicBtn = new Button
                {
                    Text = "🎵 Музыка",
                    Location = new Point(10, y),
                    Size = new Size(210, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 220, 220)
                };
                musicBtn.Click += (s, e) => { menu.Close(); OpenMusicPlayer(); };
                menu.Controls.Add(musicBtn);
                y += 45;

                Button shutdownBtn = new Button
                {
                    Text = "⏻ Выключить",
                    Location = new Point(10, y),
                    Size = new Size(210, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(200, 80, 80),
                    ForeColor = Color.White
                };
                shutdownBtn.Click += (s, e) => { menu.Close(); Application.Exit(); };
                menu.Controls.Add(shutdownBtn);

                menu.ShowDialog();
            }
        }

        private void AddExternalApp()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Исполняемые файлы (*.exe)|*.exe|Ярлыки (*.lnk)|*.lnk|Все файлы (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string name = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                        AppInfo app = new AppInfo
                        {
                            Name = name,
                            ExecutablePath = ofd.FileName
                        };
                        AppManager.AddApp(app);
                        AddAppIcon(app);
                        MessageBox.Show($"Приложение '{name}' добавлено!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AddAppIcon(AppInfo app)
        {
            Image icon = AppManager.LoadAppIcon(app);
            if (icon == null)
                icon = CreateIcon("📦");

            DesktopIcon appIcon = new DesktopIcon(app.Name, icon, "app");
            appIcon.AppId = app.Id;
            appIcon.Location = new Point(20, GetNextIconY());

            appIcon.Click += (s, e) =>
            {
                var foundApp = AppManager.GetApps().Find(a => a.Id == appIcon.AppId);
                if (foundApp != null)
                    AppManager.LaunchApp(foundApp);
            };

            appIcon.OnDelete += (s, e) =>
            {
                DialogResult result = MessageBox.Show($"Удалить приложение '{app.Name}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    AppManager.RemoveApp(appIcon.AppId);
                    desktopPanel.Controls.Remove(appIcon);
                }
            };

            desktopPanel.Controls.Add(appIcon);
        }

        private void AddBookmark()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Добавить закладку";
                dialog.Size = new Size(400, 150);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                Label labelName = new Label();
                labelName.Text = "URL сайта:";
                labelName.Location = new Point(10, 10);
                labelName.Size = new Size(80, 20);
                dialog.Controls.Add(labelName);

                TextBox textBox = new TextBox();
                textBox.Text = "https://www.google.com";
                textBox.Location = new Point(100, 10);
                textBox.Size = new Size(270, 20);
                dialog.Controls.Add(textBox);

                Button okBtn = new Button();
                okBtn.Text = "Добавить";
                okBtn.Location = new Point(200, 45);
                okBtn.Size = new Size(80, 25);
                okBtn.BackColor = Color.FromArgb(0, 120, 215);
                okBtn.ForeColor = Color.White;
                okBtn.FlatStyle = FlatStyle.Flat;
                okBtn.DialogResult = DialogResult.OK;
                dialog.Controls.Add(okBtn);

                Button cancelBtn = new Button();
                cancelBtn.Text = "Отмена";
                cancelBtn.Location = new Point(290, 45);
                cancelBtn.Size = new Size(80, 25);
                cancelBtn.FlatStyle = FlatStyle.Flat;
                cancelBtn.DialogResult = DialogResult.Cancel;
                dialog.Controls.Add(cancelBtn);

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(textBox.Text))
                {
                    string url = textBox.Text;
                    try
                    {
                        string name = "Сайт";
                        try
                        {
                            Uri uri = new Uri(url);
                            name = uri.Host;
                            if (name.StartsWith("www.")) name = name.Substring(4);
                        }
                        catch { }

                        WebBookmark bookmark = new WebBookmark
                        {
                            Name = name,
                            Url = url
                        };
                        BookmarkManager.AddBookmark(bookmark);
                        AddBookmarkIcon(bookmark);
                        MessageBox.Show($"Закладка '{name}' добавлена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Неверный URL!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AddBookmarkIcon(WebBookmark bookmark)
        {
            Image icon = BookmarkManager.LoadBookmarkIcon(bookmark);
            if (icon == null)
            {
                try
                {
                    icon = BookmarkManager.GenerateFaviconFromUrl(bookmark.Url);
                }
                catch { icon = null; }
                if (icon == null)
                    icon = CreateIcon("🌐");
            }

            DesktopIcon bookmarkIcon = new DesktopIcon(bookmark.Name, icon, "bookmark");
            bookmarkIcon.AppId = bookmark.Id;
            bookmarkIcon.Location = new Point(120, GetNextIconY());

            bookmarkIcon.Click += (s, e) =>
            {
                var found = BookmarkManager.GetBookmarks().Find(b => b.Id == bookmarkIcon.AppId);
                if (found != null)
                {
                    OpenBookmark(found);
                }
            };

            bookmarkIcon.OnDelete += (s, e) =>
            {
                DialogResult result = MessageBox.Show($"Удалить закладку '{bookmark.Name}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    BookmarkManager.RemoveBookmark(bookmarkIcon.AppId);
                    desktopPanel.Controls.Remove(bookmarkIcon);
                }
            };

            desktopPanel.Controls.Add(bookmarkIcon);
        }
        
        private void RefreshDesktop()
        {
            desktopPanel.Refresh();
        }

        private void SaveDesktopLayout()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Сохранить макет";
                dialog.Size = new Size(350, 120);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                Label labelName = new Label();
                labelName.Text = "Название макета:";
                labelName.Location = new Point(10, 10);
                labelName.Size = new Size(100, 20);
                dialog.Controls.Add(labelName);

                TextBox textBox = new TextBox();
                textBox.Text = $"Макет {DateTime.Now:dd.MM.yyyy HH:mm}";
                textBox.Location = new Point(120, 10);
                textBox.Size = new Size(200, 20);
                dialog.Controls.Add(textBox);

                Button okBtn = new Button();
                okBtn.Text = "Сохранить";
                okBtn.Location = new Point(150, 45);
                okBtn.Size = new Size(80, 25);
                okBtn.BackColor = Color.FromArgb(0, 120, 215);
                okBtn.ForeColor = Color.White;
                okBtn.FlatStyle = FlatStyle.Flat;
                okBtn.DialogResult = DialogResult.OK;
                dialog.Controls.Add(okBtn);

                Button cancelBtn = new Button();
                cancelBtn.Text = "Отмена";
                cancelBtn.Location = new Point(240, 45);
                cancelBtn.Size = new Size(80, 25);
                cancelBtn.FlatStyle = FlatStyle.Flat;
                cancelBtn.DialogResult = DialogResult.Cancel;
                dialog.Controls.Add(cancelBtn);

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(textBox.Text))
                {
                    string name = textBox.Text;
                    DesktopLayoutManager.SaveBackup(name, desktopPanel);
                }
            }
        }

        private void RestoreDesktopLayout()
        {
            var layouts = DesktopLayoutManager.GetLayoutsList();

            if (layouts.Count == 0)
            {
                MessageBox.Show("Нет сохранённых макетов!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form dialog = new Form())
            {
                dialog.Text = "Выберите макет для восстановления";
                dialog.Size = new Size(300, 350);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                ListBox listBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10),
                    BackColor = Color.White
                };
                foreach (string name in layouts)
                {
                    listBox.Items.Add(name);
                }

                Panel buttonPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    Padding = new Padding(10),
                    BackColor = Color.FromArgb(192, 192, 192)
                };

                Button restoreBtn = new Button
                {
                    Text = "Восстановить",
                    Location = new Point(10, 10),
                    Size = new Size(100, 30),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                restoreBtn.Click += (s, e) =>
                {
                    if (listBox.SelectedItem != null)
                    {
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Tag = listBox.SelectedItem.ToString();
                        dialog.Close();
                    }
                };

                Button cancelBtn = new Button();
                cancelBtn.Text = "Отмена";
                cancelBtn.Location = new Point(120, 10);
                cancelBtn.Size = new Size(100, 30);
                cancelBtn.FlatStyle = FlatStyle.Flat;
                cancelBtn.Cursor = Cursors.Hand;
                cancelBtn.Click += (s, e) => dialog.Close();

                buttonPanel.Controls.Add(restoreBtn);
                buttonPanel.Controls.Add(cancelBtn);
                dialog.Controls.Add(listBox);
                dialog.Controls.Add(buttonPanel);

                if (dialog.ShowDialog() == DialogResult.OK && dialog.Tag != null)
                {
                    string name = dialog.Tag.ToString();
                    DialogResult result = MessageBox.Show($"Восстановить макет '{name}'?\n" +
                        "Все текущие иконки (кроме системных) будут заменены.",
                        "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        DesktopLayoutManager.RestoreBackup(name, desktopPanel);
                    }
                }
            }
        }

        private void DeleteDesktopLayout()
        {
            var layouts = DesktopLayoutManager.GetLayoutsList();

            if (layouts.Count == 0)
            {
                MessageBox.Show("Нет сохранённых макетов!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form dialog = new Form())
            {
                dialog.Text = "Выберите макет для удаления";
                dialog.Size = new Size(300, 350);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                ListBox listBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10),
                    BackColor = Color.White
                };
                foreach (string name in layouts)
                {
                    listBox.Items.Add(name);
                }

                Panel buttonPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    Padding = new Padding(10),
                    BackColor = Color.FromArgb(192, 192, 192)
                };

                Button deleteBtn = new Button
                {
                    Text = "Удалить",
                    Location = new Point(10, 10),
                    Size = new Size(100, 30),
                    BackColor = Color.FromArgb(200, 80, 80),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                deleteBtn.Click += (s, e) =>
                {
                    if (listBox.SelectedItem != null)
                    {
                        dialog.DialogResult = DialogResult.OK;
                        dialog.Tag = listBox.SelectedItem.ToString();
                        dialog.Close();
                    }
                };

                Button cancelBtn = new Button();
                cancelBtn.Text = "Отмена";
                cancelBtn.Location = new Point(120, 10);
                cancelBtn.Size = new Size(100, 30);
                cancelBtn.FlatStyle = FlatStyle.Flat;
                cancelBtn.Cursor = Cursors.Hand;
                cancelBtn.Click += (s, e) => dialog.Close();

                buttonPanel.Controls.Add(deleteBtn);
                buttonPanel.Controls.Add(cancelBtn);
                dialog.Controls.Add(listBox);
                dialog.Controls.Add(buttonPanel);

                if (dialog.ShowDialog() == DialogResult.OK && dialog.Tag != null)
                {
                    string name = dialog.Tag.ToString();
                    DialogResult result = MessageBox.Show($"Удалить макет '{name}'?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        DesktopLayoutManager.DeleteBackup(name);
                    }
                }
            }
        }

        private void AddTaskbarButton(string text, CustomWindow window)
        {
            Panel taskButtonsPanel = null;
            foreach (Control ctrl in taskbar.Controls)
            {
                if (ctrl is Panel panel && panel.Name == "taskButtonsPanel")
                {
                    taskButtonsPanel = panel;
                    break;
                }
            }

            if (taskButtonsPanel == null) return;

            foreach (Control ctrl in taskButtonsPanel.Controls)
            {
                if (ctrl is Button btn && btn.Tag == window)
                {
                    return;
                }
            }

            int xPosition = 0;
            foreach (Control ctrl in taskButtonsPanel.Controls)
            {
                if (ctrl is Button btn)
                {
                    xPosition += btn.Width + btn.Margin.Left + btn.Margin.Right;
                }
            }

            Button taskBtn = new Button();
            taskBtn.Text = text;
            taskBtn.Font = new Font("Segoe UI", 8);
            taskBtn.BackColor = Color.FromArgb(192, 192, 192);
            taskBtn.ForeColor = Color.Black;
            taskBtn.FlatStyle = FlatStyle.Flat;
            taskBtn.FlatAppearance.BorderSize = 1;
            taskBtn.FlatAppearance.BorderColor = Color.FromArgb(128, 128, 128);
            taskBtn.Size = new Size(120, 30);
            taskBtn.Margin = new Padding(2);
            taskBtn.Location = new Point(xPosition, 4);
            taskBtn.Tag = window;
            taskBtn.Cursor = Cursors.Hand;
            taskBtn.TextAlign = ContentAlignment.MiddleCenter;

            taskBtn.MouseEnter += (s, e) =>
            {
                taskBtn.BackColor = Color.FromArgb(212, 208, 200);
            };
            taskBtn.MouseLeave += (s, e) =>
            {
                taskBtn.BackColor = Color.FromArgb(192, 192, 192);
            };

            taskBtn.Click += (sender, args) =>
            {
                window.RestoreWindow();
                taskButtonsPanel.Controls.Remove(taskBtn);
                UpdateTaskButtonsPosition();
            };

            taskButtonsPanel.Controls.Add(taskBtn);

            foreach (Control ctrl in taskbar.Controls)
            {
                if (ctrl is Panel infoPanel && infoPanel.Size.Width == 140)
                {
                    infoPanel.BringToFront();
                    break;
                }
            }
        }


        // Добавь этот метод для обновления позиций кнопок
        private void UpdateTaskButtonsPosition()
        {
            Panel taskButtonsPanel = null;
            foreach (Control ctrl in taskbar.Controls)
            {
                if (ctrl is Panel panel && panel.Name == "taskButtonsPanel")
                {
                    taskButtonsPanel = panel;
                    break;
                }
            }

            if (taskButtonsPanel == null) return;

            int xPosition = 0;
            foreach (Control ctrl in taskButtonsPanel.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.Location = new Point(xPosition, 4);
                    xPosition += btn.Width + btn.Margin.Left + btn.Margin.Right;
                }
            }
        }



        // Добавь метод для удаления кнопки
        private void RemoveTaskbarButton(CustomWindow window)
        {
            Panel taskButtonsPanel = null;
            foreach (Control ctrl in taskbar.Controls)
            {
                if (ctrl is Panel panel && panel.Name == "taskButtonsPanel")
                {
                    taskButtonsPanel = panel;
                    break;
                }
            }

            if (taskButtonsPanel == null) return;

            List<Control> toRemove = new List<Control>();
            foreach (Control ctrl in taskButtonsPanel.Controls)
            {
                if (ctrl is Button btn && btn.Tag == window)
                {
                    toRemove.Add(ctrl);
                }
            }

            foreach (var ctrl in toRemove)
            {
                taskButtonsPanel.Controls.Remove(ctrl);
            }

            UpdateTaskButtonsPosition();
        }



        private void OpenCalculator()
        {
            CustomWindow window = new CustomWindow("Калькулятор");
            CalculatorApp calculator = new CalculatorApp();
            window.ContentControl = calculator;
            window.Size = new Size(320, 420);
            window.MinimumSize = new Size(320, 420);
            window.MaximumSize = new Size(320, 420);
            this.Controls.Add(window);
            window.BringToFront();

            // При закрытии окна удаляем кнопку с панели задач
            window.Closed += (s, e) => RemoveTaskbarButton(window);
            window.Minimized += (s, e) => AddTaskbarButton("Калькулятор", window);
        }

        private void OpenNotepad()
        {
            CustomWindow window = new CustomWindow("Блокнот - MyOS 95");
            NotepadApp notepad = new NotepadApp();
            window.ContentControl = notepad;
            window.Size = new Size(700, 500);
            window.MinimumSize = new Size(400, 300);
            this.Controls.Add(window);
            window.BringToFront();

            window.Closed += (s, e) => RemoveTaskbarButton(window);
            window.Minimized += (s, e) => AddTaskbarButton("Блокнот", window);
        }

        private void OpenBrowser()
        {
            CustomWindow window = new CustomWindow("Браузер - MyOS 95");
            ModernBrowserApp browser = new ModernBrowserApp();
            window.ContentControl = browser;
            window.Size = new Size(1000, 700);
            window.MinimumSize = new Size(600, 400);
            this.Controls.Add(window);
            window.BringToFront();

            window.Closed += (s, e) => RemoveTaskbarButton(window);
            window.Minimized += (s, e) => AddTaskbarButton("Браузер", window);
        }

        private void OpenSettings()
        {
            CustomWindow window = new CustomWindow("⚙️ Настройки");
            SettingsApp settings = new SettingsApp(this);
            window.ContentControl = settings;
            window.Size = new Size(600, 550);
            window.MinimumSize = new Size(550, 500);
            this.Controls.Add(window);
            window.BringToFront();

            window.Closed += (s, e) => RemoveTaskbarButton(window);
            window.Minimized += (s, e) => AddTaskbarButton("Настройки", window);
        }

        private void OpenMusicPlayer()
        {
            CustomWindow window = new CustomWindow("🎵 Музыкальный плеер");
            MusicPlayerApp player = new MusicPlayerApp();
            window.ContentControl = player;
            window.Size = new Size(700, 600);
            window.MinimumSize = new Size(600, 500);
            this.Controls.Add(window);
            window.BringToFront();

            window.Closed += (s, e) => RemoveTaskbarButton(window);
            window.Minimized += (s, e) => AddTaskbarButton("Музыка", window);
        }

        public void OpenBookmark(WebBookmark bookmark)
        {
            CustomWindow window = new CustomWindow($"🌐 {bookmark.Name}");
            ModernBrowserApp browser = new ModernBrowserApp();
            window.ContentControl = browser;
            window.Size = new Size(1000, 700);
            window.MinimumSize = new Size(600, 400);
            this.Controls.Add(window);
            window.BringToFront();

            browser.NavigateTo(bookmark.Url);

            window.Closed += (s, e) => RemoveTaskbarButton(window);
            window.Minimized += (s, e) => AddTaskbarButton(bookmark.Name, window);
        }

        private void LoadWallpaperColor()
        {
            try
            {
                string settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MyOS95", "settings.json");

                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<WallpaperSettings>(json);
                    if (settings != null)
                    {
                        this.BackColor = Color.FromArgb(settings.Wallpaper);
                        savedWallpaperColor = this.BackColor;
                    }
                }
            }
            catch { }
        }

        public void SaveWallpaperColor(Color color)
        {
            try
            {
                string settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MyOS95", "settings.json");

                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                var settings = new WallpaperSettings { Wallpaper = color.ToArgb() };
                string json = System.Text.Json.JsonSerializer.Serialize(settings);
                File.WriteAllText(settingsPath, json);
                savedWallpaperColor = color;
            }
            catch { }
        }

        public void ResetIconsToDefault()
        {
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

            int y = 20;
            foreach (Control ctrl in desktopPanel.Controls)
            {
                if (ctrl is DesktopIcon icon && icon.Type == "system")
                {
                    icon.SetPosition(new Point(20, y));
                    y += 85;
                }
            }
        }

        public void ResetAllSettings()
        {
            this.BackColor = Color.FromArgb(0, 128, 128);
            SaveWallpaperColor(this.BackColor);
            ResetIconsToDefault();

            var layouts = DesktopLayoutManager.GetLayoutsList();
            foreach (var name in layouts)
            {
                DesktopLayoutManager.DeleteBackup(name);
            }
        }

        private class WallpaperSettings
        {
            public int Wallpaper { get; set; }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (taskbar != null && clockLabel != null)
            {
                // Часы обновляются автоматически через таймер
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (clockTimer != null)
            {
                clockTimer.Stop();
                clockTimer.Dispose();
            }
        }
    }
}