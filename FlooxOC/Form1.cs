using FlooxOC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        private Timer demoStatusTimer;
        private Label demoStatusLabel;
        private Label languageLabel;

        public Form1()
        {
            this.Text = "Floox OC. Home Version";
            this.BackColor = Color.FromArgb(0, 128, 128);
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            LoadWallpaperColor();
            InitializeTaskbar();
            InitializeDesktop();
            InitializeDesktopContextMenu();
            LoadSavedItems();

            this.Load += (s, e) => CheckDemoStatus();

            // ===== ТАЙМЕР ДЛЯ ОБНОВЛЕНИЯ РАСКЛАДКИ КЛАВИАТУРЫ =====
            Timer layoutTimer = new Timer();
            layoutTimer.Interval = 500;
            layoutTimer.Tick += (s, e) => UpdateLanguageIndicator();
            layoutTimer.Start();
        }

        // ===== ОБНОВЛЕНИЕ ИНДИКАТОРА РАСКЛАДКИ КЛАВИАТУРЫ =====
        private void UpdateLanguageIndicator()
        {
            if (languageLabel == null) return;

            try
            {
                // Получаем текущую раскладку клавиатуры
                InputLanguage currentLayout = InputLanguage.CurrentInputLanguage;
                string layoutName = currentLayout.Culture.Name;
                string language = layoutName.Split('-')[0];
                string code = GetLanguageCode(language);

                if (languageLabel.Text != code)
                {
                    languageLabel.Text = code;
                }
            }
            catch
            {
                try
                {
                    string cultureName = CultureInfo.CurrentCulture.Name;
                    string language = cultureName.Split('-')[0];
                    languageLabel.Text = GetLanguageCode(language);
                }
                catch
                {
                    languageLabel.Text = "EN";
                }
            }
        }

        private string GetLanguageCode(string language)
        {
            switch (language.ToLower())
            {
                case "ru": return "RU";
                case "en": return "EN";
                case "de": return "DE";
                case "fr": return "FR";
                case "es": return "ES";
                case "it": return "IT";
                case "zh": return "ZH";
                case "ja": return "JA";
                case "ko": return "KO";
                case "ar": return "AR";
                case "pt": return "PT";
                default: return language.ToUpper().Substring(0, Math.Min(2, language.Length));
            }
        }

        private void InitializeTaskbar()
        {
            taskbar = new Panel();
            taskbar.Height = 55;
            taskbar.Dock = DockStyle.Bottom;
            taskbar.BackColor = Color.FromArgb(192, 192, 192);
            taskbar.BorderStyle = BorderStyle.Fixed3D;
            taskbar.Name = "taskbar";
            taskbar.Padding = new Padding(3, 0, 3, 0);

            // ===== КНОПКА ПУСК =====
            Button startBtn = new Button();
            startBtn.Text = " Пуск ";
            startBtn.Location = new Point(4, 8);
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
            taskButtonsPanel.Size = new Size(taskbar.Width - 270, 42);
            taskButtonsPanel.BackColor = Color.Transparent;
            taskButtonsPanel.Name = "taskButtonsPanel";
            taskbar.Controls.Add(taskButtonsPanel);

            // ===== ПАНЕЛЬ ИНФОРМАЦИИ =====
            Panel infoPanel = new Panel();
            infoPanel.Size = new Size(175, 44);
            infoPanel.Location = new Point(taskbar.Width - 180, 4);
            infoPanel.BackColor = Color.FromArgb(192, 192, 192);
            infoPanel.BorderStyle = BorderStyle.Fixed3D;
            infoPanel.Cursor = Cursors.Hand;

            // ===== ИНДИКАТОР РАСКЛАДКИ КЛАВИАТУРЫ =====
            string initialLanguage = "EN";
            try
            {
                string layoutName = InputLanguage.CurrentInputLanguage.Culture.Name;
                string lang = layoutName.Split('-')[0];
                initialLanguage = GetLanguageCode(lang);
            }
            catch { }

            languageLabel = new Label();
            languageLabel.Text = initialLanguage;
            languageLabel.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            languageLabel.ForeColor = Color.Black;
            languageLabel.Location = new Point(4, 0);
            languageLabel.Size = new Size(32, 14);
            languageLabel.TextAlign = ContentAlignment.MiddleCenter;
            languageLabel.BackColor = Color.FromArgb(210, 210, 210);
            languageLabel.BorderStyle = BorderStyle.FixedSingle;
            infoPanel.Controls.Add(languageLabel);

            // Дата
            Label dateLabel = new Label();
            dateLabel.Text = DateTime.Now.ToString("dd.MM.yyyy");
            dateLabel.Font = new Font("Segoe UI", 7);
            dateLabel.ForeColor = Color.Black;
            dateLabel.Location = new Point(40, 0);
            dateLabel.Size = new Size(130, 14);
            dateLabel.TextAlign = ContentAlignment.MiddleCenter;
            infoPanel.Controls.Add(dateLabel);

            // Время
            clockLabel = new Label();
            clockLabel.Text = DateTime.Now.ToString("HH:mm");
            clockLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            clockLabel.ForeColor = Color.Black;
            clockLabel.Location = new Point(4, 14);
            clockLabel.Size = new Size(166, 22);
            clockLabel.TextAlign = ContentAlignment.MiddleCenter;
            clockLabel.Name = "clockLabel";
            infoPanel.Controls.Add(clockLabel);

            // Демо-статус
            demoStatusLabel = new Label();
            demoStatusLabel.Text = "";
            demoStatusLabel.Font = new Font("Segoe UI", 7);
            demoStatusLabel.ForeColor = Color.FromArgb(200, 50, 50);
            demoStatusLabel.Location = new Point(4, 34);
            demoStatusLabel.Size = new Size(166, 12);
            demoStatusLabel.TextAlign = ContentAlignment.MiddleCenter;
            demoStatusLabel.Visible = false;
            infoPanel.Controls.Add(demoStatusLabel);

            // Клик по панели
            infoPanel.Click += (s, e) => ShowDateTimeInfo();
            dateLabel.Click += (s, e) => ShowDateTimeInfo();
            clockLabel.Click += (s, e) => ShowDateTimeInfo();
            demoStatusLabel.Click += (s, e) => ShowDateTimeInfo();
            languageLabel.Click += (s, e) => ShowDateTimeInfo();

            taskbar.Controls.Add(infoPanel);

            // ===== ТАЙМЕР ДЛЯ ЧАСОВ =====
            clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) =>
            {
                if (clockLabel != null)
                {
                    clockLabel.Text = DateTime.Now.ToString("HH:mm");
                    dateLabel.Text = DateTime.Now.ToString("dd.MM.yyyy");
                }
                UpdateDemoStatus();
            };
            clockTimer.Start();

            this.Controls.Add(taskbar);

            this.Resize += (s, e) =>
            {
                if (taskbar != null)
                {
                    infoPanel.Location = new Point(taskbar.Width - 180, 4);
                    taskButtonsPanel.Width = taskbar.Width - 270;
                }
            };
        }

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
            y += 85;

            DesktopIcon infoIcon = new DesktopIcon("Информация", CreateIcon("ℹ️"), "system");
            infoIcon.Location = new Point(20, y);
            infoIcon.Click += (s, e) => OpenSystemInfo();
            desktopPanel.Controls.Add(infoIcon);
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
                menu.Size = new Size(250, 400);
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

                Button infoBtn = new Button
                {
                    Text = "ℹ️ Информация",
                    Location = new Point(10, y),
                    Size = new Size(210, 35),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 220, 220)
                };
                infoBtn.Click += (s, e) => { menu.Close(); OpenSystemInfo(); };
                menu.Controls.Add(infoBtn);
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
                        string name = Path.GetFileNameWithoutExtension(ofd.FileName);
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

            window.Minimized += (s, e) => AddTaskbarButton(bookmark.Name, window);
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
            taskBtn.Location = new Point(xPosition, 6);
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
                if (ctrl is Panel infoPanel && infoPanel.Size.Width == 175)
                {
                    infoPanel.BringToFront();
                    break;
                }
            }
        }

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

        // ====== ПРИЛОЖЕНИЯ ======

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

            window.Closed += (s, e) => RemoveTaskbarButton(window);
            window.Minimized += (s, e) => AddTaskbarButton("Калькулятор", window);
        }

        private void OpenNotepad()
        {
            CustomWindow window = new CustomWindow("Блокнот - Floox OC");
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
            CustomWindow window = new CustomWindow("Браузер - Floox OC");
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

        private void OpenSystemInfo()
        {
            CustomWindow window = new CustomWindow("ℹ️ Информация о системе");
            SystemInfoApp info = new SystemInfoApp();
            window.ContentControl = info;
            window.Size = new Size(500, 550);
            window.MinimumSize = new Size(450, 500);
            this.Controls.Add(window);
            window.BringToFront();

            window.Closed += (s, e) => RemoveTaskbarButton(window);
            window.Minimized += (s, e) => AddTaskbarButton("Информация", window);
        }

        // ====== ДЕМО-РЕЖИМ ======

        public void CheckDemoStatus()
        {
            if (AccountManager.IsDemoMode)
            {
                if (AccountManager.IsDemoExpired)
                {
                    this.Enabled = false;
                    ShowActivationDialog();
                }
                else
                {
                    UpdateDemoStatus();

                    if (demoStatusTimer == null)
                    {
                        demoStatusTimer = new Timer();
                        demoStatusTimer.Interval = 5000;
                        demoStatusTimer.Tick += (s, e) => UpdateDemoStatus();
                        demoStatusTimer.Start();
                    }

                    if (demoStatusLabel != null)
                    {
                        demoStatusLabel.Visible = true;
                        demoStatusLabel.Text = "🆓 ДЕМО";
                    }
                }
            }
        }

        private void UpdateDemoStatus()
        {
            if (AccountManager.IsDemoMode && !AccountManager.IsDemoExpired)
            {
                string remaining = AccountManager.GetDemoTimeRemaining();
                this.Text = $"Floox OC. Home Version (ДЕМО - {remaining})";

                if (demoStatusLabel != null)
                {
                    demoStatusLabel.Visible = true;
                    demoStatusLabel.Text = $"🆓 ДЕМО: {remaining}";
                    demoStatusLabel.ForeColor = Color.FromArgb(200, 50, 50);
                }
            }
            else if (AccountManager.IsDemoExpired)
            {
                if (demoStatusLabel != null)
                {
                    demoStatusLabel.Visible = true;
                    demoStatusLabel.Text = "⛔ ДЕМО ИСТЕКЛО!";
                    demoStatusLabel.ForeColor = Color.Red;
                }
            }
            else
            {
                if (demoStatusLabel != null)
                {
                    demoStatusLabel.Visible = false;
                }
            }
        }

        public void ShowActivationDialog()
        {
            using (var activationDialog = new ActivationDialog())
            {
                activationDialog.ShowDialog();
                if (activationDialog.IsActivated)
                {
                    AccountManager.StopDemoTimers();
                    AccountManager.IsDemoMode = false;
                    AccountManager.IsDemoExpired = false;

                    this.Enabled = true;
                    this.Text = "Floox OC. Home Version";

                    if (demoStatusLabel != null)
                    {
                        demoStatusLabel.Visible = false;
                    }

                    if (demoStatusTimer != null)
                    {
                        demoStatusTimer.Stop();
                        demoStatusTimer.Dispose();
                        demoStatusTimer = null;
                    }

                    MessageBox.Show("✅ Система активирована! Добро пожаловать!",
                        "Активация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (AccountManager.IsDemoExpired)
                    {
                        MessageBox.Show("❌ Активация не выполнена. Приложение будет закрыто.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }
                }
            }
        }

        // ====== ИНФОРМАЦИЯ ======

        private void ShowDateTimeInfo()
        {
            try
            {
                string uptime = GetSystemUptime();
                string demoInfo = AccountManager.IsDemoMode ? $"\n🆓 ДЕМО-РЕЖИМ: {AccountManager.GetDemoTimeRemaining()}" : "";
                string activationInfo = AccountManager.IsActivated() ? "\n✅ Система активирована" : "";

                MessageBox.Show(
                    $"📅 {DateTime.Now.ToString("dd.MM.yyyy")}\n" +
                    $"📆 {GetDayOfWeek()}\n" +
                    $"⏰ {DateTime.Now.ToString("HH:mm:ss")}\n" +
                    $"\n" +
                    $"🕐 Время работы: {uptime}\n" +
                    $"💻 {Environment.MachineName}\n" +
                    $"{demoInfo}" +
                    $"{activationInfo}",
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

        // ====== НАСТРОЙКИ ======

        private void LoadWallpaperColor()
        {
            try
            {
                string settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Floox OC. Home Version", "settings.json");

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
                    "Floox OC. Home Version", "settings.json");

                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                var settings = new WallpaperSettings { Wallpaper = color.ToArgb() };
                string json = System.Text.Json.JsonSerializer.Serialize(settings);
                File.WriteAllText(settingsPath, json);
                savedWallpaperColor = color;

                // ===== ПРИМЕНЯЕМ ЦВЕТА =====
                this.BackColor = color;
                ColorHelper.ApplyContrastToControls(this);
                this.Refresh();
            }
            catch { }
        }

        public void ResetIconsToDefault()
        {
            // ===== НОВАЯ ЛОГИКА: ТОЛЬКО РАССТАНОВКА, БЕЗ УДАЛЕНИЯ =====

            // Получаем все иконки (и системные, и добавленные)
            List<DesktopIcon> icons = new List<DesktopIcon>();
            foreach (Control ctrl in desktopPanel.Controls)
            {
                if (ctrl is DesktopIcon icon)
                {
                    icons.Add(icon);
                }
            }

            if (icons.Count == 0)
            {
                MessageBox.Show("Нет иконок для расстановки!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Сортируем иконки: сначала системные, потом остальные (по алфавиту)
            icons.Sort((a, b) =>
            {
                // Системные иконки всегда сверху
                if (a.Type == "system" && b.Type != "system") return -1;
                if (a.Type != "system" && b.Type == "system") return 1;

                // Остальные по алфавиту
                return string.Compare(a.GetText(), b.GetText(), StringComparison.OrdinalIgnoreCase);
            });

            // Расставляем иконки по сетке
            int x = 20;
            int y = 20;
            int spacingX = 95;
            int spacingY = 85;
            int maxPerRow = 6;

            foreach (var icon in icons)
            {
                icon.Location = new Point(x, y);

                x += spacingX;
                if (x > maxPerRow * spacingX + 20)
                {
                    x = 20;
                    y += spacingY;
                }
            }

            // Сохраняем текущий макет
            DesktopLayoutManager.SaveCurrentLayout(desktopPanel);

            // Уведомляем пользователя
            MessageBox.Show($"✅ Иконки расставлены!\nВсего иконок: {icons.Count}\n" +
                $"Системных: {icons.FindAll(i => i.Type == "system").Count}\n" +
                $"Добавленных: {icons.FindAll(i => i.Type != "system").Count}",
                "Расстановка иконок",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public void ResetAllSettings()
        {
            this.BackColor = Color.FromArgb(0, 128, 128);
            SaveWallpaperColor(this.BackColor);

            // ===== НЕ УДАЛЯЕМ ИКОНКИ =====
            // Просто расставляем их по сетке
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
            if (taskbar != null)
            {
                foreach (Control ctrl in taskbar.Controls)
                {
                    if (ctrl is Panel infoPanel && infoPanel.Size.Width == 175)
                    {
                        infoPanel.Location = new Point(taskbar.Width - 180, 2);
                        break;
                    }
                }
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

            if (demoStatusTimer != null)
            {
                demoStatusTimer.Stop();
                demoStatusTimer.Dispose();
            }

            AccountManager.StopDemoTimers();
        }
    }
}