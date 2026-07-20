using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace FlooxOC
{
    public class SystemInfoApp : Panel
    {
        // ===== ПОЛЯ =====
        private Label labelKernel;
        private Label labelMinRequirements;
        private Label labelOS;
        private Label labelEdition;
        private Label labelBuild;
        private Label labelVersionStatus;
        private Button buttonCheckUpdates;
        private string currentVersion = "v0.0.3";
        private string versionFile = "";

        // ===== КОМАНДНАЯ СТРОКА =====
        private TextBox textCommand;
        private Button buttonExecute;
        private RichTextBox richOutput;
        private Label labelCommandStatus;

        // ===== ССЫЛКИ =====
        private string versionUrl = "https://raw.githubusercontent.com/MihailBabushkin/FLooxOC_SV/master/version.txt";

        // ===== КОНСТРУКТОР =====
        public SystemInfoApp()
        {
            InitializeApp();
            LoadVersion();
            InitializeComponents();
            ApplyColors();
            StartSilentUpdateCheck();
        }

        // ===== ИНИЦИАЛИЗАЦИЯ ПРИЛОЖЕНИЯ =====
        private void InitializeApp()
        {
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(15);
            this.AutoScroll = true;

            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Floox OC. Home Version");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            versionFile = Path.Combine(appDataPath, "SMI_version.txt");
        }

        // ===== ЗАГРУЗКА ВЕРСИИ =====
        private void LoadVersion()
        {
            try
            {
                if (File.Exists(versionFile))
                {
                    string version = File.ReadAllText(versionFile).Trim();
                    if (!string.IsNullOrEmpty(version))
                    {
                        currentVersion = version;
                    }
                }
                else
                {
                    File.WriteAllText(versionFile, currentVersion);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки версии: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveVersion()
        {
            try
            {
                File.WriteAllText(versionFile, currentVersion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения версии: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== ПРИМЕНЕНИЕ ЦВЕТОВ =====
        private void ApplyColors()
        {
            ColorHelper.ApplyContrastToControls(this);
            this.Resize += (s, e) => ColorHelper.ApplyContrastToControls(this);
        }

        // ===== ТИХАЯ ПРОВЕРКА ОБНОВЛЕНИЙ =====
        private void StartSilentUpdateCheck()
        {
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
            {
                this.BeginInvoke((Action)(() => CheckForUpdatesSilently()));
            });
        }

        // ===== ИНИЦИАЛИЗАЦИЯ КОМПОНЕНТОВ =====
        private void InitializeComponents()
        {
            int y = 10;

            // ---- ЗАГОЛОВОК ----
            Label titleLabel = CreateLabel("ℹ️ Информация о системе", new Font("Segoe UI", 18, FontStyle.Bold), new Point(10, y), new Size(350, 35));
            this.Controls.Add(titleLabel);
            y += 50;

            // ---- РАЗДЕЛИТЕЛЬ ----
            this.Controls.Add(CreateSeparator(new Point(10, y), new Size(400, 2)));
            y += 15;

            // ---- ОСНОВНОЕ ----
            Label mainTitle = CreateLabel("🖥️ Основное", new Font("Segoe UI", 12, FontStyle.Bold), new Point(10, y), new Size(200, 25));
            this.Controls.Add(mainTitle);
            y += 30;

            // Ядро
            this.Controls.Add(CreateLabel("Ядро:", new Font("Segoe UI", 10), new Point(20, y), new Size(120, 25)));
            labelKernel = CreateLabel("Windows 10 (NT 10.0)", new Font("Segoe UI", 10, FontStyle.Bold), new Point(140, y), new Size(250, 25));
            labelKernel.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
            this.Controls.Add(labelKernel);
            y += 30;

            // Минимальные требования
            this.Controls.Add(CreateLabel("Минимальные требования:", new Font("Segoe UI", 10), new Point(20, y), new Size(140, 25)));
            labelMinRequirements = CreateLabel("CPU: 1 ГГц, RAM: 1 ГБ, HDD: 500 МБ", new Font("Segoe UI", 10), new Point(160, y), new Size(250, 25));
            this.Controls.Add(labelMinRequirements);
            y += 40;

            // ---- РАЗДЕЛИТЕЛЬ ----
            this.Controls.Add(CreateSeparator(new Point(10, y), new Size(400, 2)));
            y += 15;

            // ---- ВЕРСИЯ ----
            Label versionTitle = CreateLabel("📌 Версия", new Font("Segoe UI", 12, FontStyle.Bold), new Point(10, y), new Size(200, 25));
            this.Controls.Add(versionTitle);
            y += 30;

            // ОС
            this.Controls.Add(CreateLabel("ОС:", new Font("Segoe UI", 10), new Point(20, y), new Size(120, 25)));
            labelOS = CreateLabel("Floox'OC", new Font("Segoe UI", 10, FontStyle.Bold), new Point(140, y), new Size(250, 25));
            labelOS.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
            this.Controls.Add(labelOS);
            y += 30;

            // Набор
            this.Controls.Add(CreateLabel("Набор:", new Font("Segoe UI", 10), new Point(20, y), new Size(120, 25)));
            labelEdition = CreateLabel("Home Version", new Font("Segoe UI", 10, FontStyle.Bold), new Point(140, y), new Size(250, 25));
            this.Controls.Add(labelEdition);
            y += 30;

            // Версия сборки
            this.Controls.Add(CreateLabel("Версия сборки:", new Font("Segoe UI", 10), new Point(20, y), new Size(120, 25)));
            labelBuild = CreateLabel(currentVersion, new Font("Segoe UI", 10, FontStyle.Bold), new Point(140, y), new Size(250, 25));
            labelBuild.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
            this.Controls.Add(labelBuild);
            y += 40;

            // ---- РАЗДЕЛИТЕЛЬ ----
            this.Controls.Add(CreateSeparator(new Point(10, y), new Size(400, 2)));
            y += 15;

            // ---- СТАТУС ОБНОВЛЕНИЙ ----
            labelVersionStatus = CreateLabel("🔍 Проверка обновлений...", new Font("Segoe UI", 10), new Point(20, y), new Size(380, 25));
            this.Controls.Add(labelVersionStatus);
            y += 35;

            // Кнопка проверки обновлений
            buttonCheckUpdates = CreateButton("🔄 Проверить обновления", new Point(20, y), new Size(200, 40));
            buttonCheckUpdates.BackColor = Color.FromArgb(0, 120, 215);
            buttonCheckUpdates.ForeColor = ColorHelper.GetContrastTextColor(buttonCheckUpdates.BackColor);
            buttonCheckUpdates.Click += OnCheckUpdatesClick;
            this.Controls.Add(buttonCheckUpdates);
            y += 55;

            // Кнопка обновления
            Button updateButton = CreateButton("⬇️ Обновить систему", new Point(20, y), new Size(200, 40));
            updateButton.BackColor = Color.FromArgb(0, 150, 80);
            updateButton.ForeColor = ColorHelper.GetContrastTextColor(updateButton.BackColor);
            updateButton.Click += OnPerformUpdateClick;
            this.Controls.Add(updateButton);
            y += 60;

            // ---- РАЗДЕЛИТЕЛЬ ----
            this.Controls.Add(CreateSeparator(new Point(10, y), new Size(400, 2)));
            y += 15;

            // ---- КОМАНДНАЯ СТРОКА ----
            Label commandTitle = CreateLabel("⌨️ Командная строка", new Font("Segoe UI", 12, FontStyle.Bold), new Point(10, y), new Size(200, 25));
            this.Controls.Add(commandTitle);
            y += 30;

            // Поле ввода команды
            this.Controls.Add(CreateLabel("Команда:", new Font("Segoe UI", 10), new Point(20, y), new Size(80, 25)));

            textCommand = new TextBox
            {
                Location = new Point(100, y),
                Size = new Size(250, 25),
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.Lime
            };
            textCommand.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                    ExecuteCommand();
            };
            this.Controls.Add(textCommand);
            y += 35;

            // Кнопка выполнения
            buttonExecute = CreateButton("▶️ Выполнить", new Point(20, y), new Size(120, 30));
            buttonExecute.BackColor = Color.FromArgb(0, 120, 215);
            buttonExecute.ForeColor = ColorHelper.GetContrastTextColor(buttonExecute.BackColor);
            buttonExecute.Click += (s, e) => ExecuteCommand();
            this.Controls.Add(buttonExecute);

            // Статус команды
            labelCommandStatus = CreateLabel("Готов к выполнению команд", new Font("Segoe UI", 9), new Point(150, y), new Size(250, 30));
            labelCommandStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
            this.Controls.Add(labelCommandStatus);
            y += 40;

            // Вывод результата
            this.Controls.Add(CreateLabel("Вывод:", new Font("Segoe UI", 10), new Point(20, y), new Size(60, 25)));

            richOutput = new RichTextBox
            {
                Location = new Point(20, y + 25),
                Size = new Size(460, 150),
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.Lime,
                ReadOnly = true,
                BorderStyle = BorderStyle.Fixed3D,
                Text = "> Готов к работе...\n"
            };
            this.Controls.Add(richOutput);

            // Применяем контрастные цвета
            ColorHelper.ApplyContrastToControls(this);
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====
        private Label CreateLabel(string text, Font font, Point location, Size size)
        {
            return new Label
            {
                Text = text,
                Font = font,
                Location = location,
                Size = size
            };
        }

        private Panel CreateSeparator(Point location, Size size)
        {
            return new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.FromArgb(128, 128, 128)
            };
        }

        private Button CreateButton(string text, Point location, Size size)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        // ===== ВЫПОЛНЕНИЕ КОМАНД =====
        private async void ExecuteCommand()
        {
            string command = textCommand.Text.Trim();

            if (string.IsNullOrEmpty(command))
            {
                AppendOutput("Ошибка: Введите команду!");
                return;
            }

            AppendOutput($"> {command}");

            switch (command.ToLower())
            {
                case "check_connection":
                    await CheckMySQLConnection();
                    break;

                case "code_stats":
                    await ShowCodeStatistics();
                    break;

                case "help":
                    ShowHelp();
                    break;

                case "clear":
                    richOutput.Clear();
                    AppendOutput("> Очищено");
                    break;

                case "version":
                    AppendOutput($"Текущая версия: {currentVersion}");
                    break;

                case "user":
                    ShowUserInfo();
                    break;

                case "date":
                    ShowRegistrationDate();
                    break;

                default:
                    AppendOutput($"Неизвестная команда: {command}");
                    AppendOutput("Введите 'help' для списка команд");
                    break;
            }

            textCommand.Clear();
            labelCommandStatus.Text = "Выполнено";
            labelCommandStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
        }

        // ====== ПРОВЕРКА СВЯЗИ ======
        private async System.Threading.Tasks.Task CheckMySQLConnection()
        {
            try
            {
                labelCommandStatus.Text = "⏳ Проверка связи с сервером...";
                labelCommandStatus.ForeColor = Color.DarkBlue;
                AppendOutput("🔍 Проверка связи с сервером...");

                var result = await MySQLManager.CheckConnection();

                if (result.Success)
                {
                    AppendOutput($"✅ {result.Message}");
                    AppendOutput($"📊 Всего создано: {result.TotalCodes}");
                    AppendOutput($"   ✅ Свободно: {result.FreeCodes}");
                    AppendOutput($"   ❌ Использовано: {result.ActivatedUsers}");
                    labelCommandStatus.Text = "✅ Связь с сервером установлена!";
                    labelCommandStatus.ForeColor = Color.DarkGreen;
                }
                else
                {
                    AppendOutput($"❌ {result.Message}");
                    labelCommandStatus.Text = "❌ Ошибка связи с сервером!";
                    labelCommandStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"❌ Ошибка: {ex.Message}");
                labelCommandStatus.Text = "❌ Ошибка!";
                labelCommandStatus.ForeColor = Color.Red;
            }
        }

        // ====== СТАТИСТИКА КОДОВ ======
        private async System.Threading.Tasks.Task ShowCodeStatistics()
        {
            try
            {
                AppendOutput("📊 Получение статистики кодов...");
                string stats = await MySQLManager.GetStatistics();
                AppendOutput(stats);
                labelCommandStatus.Text = "✅ Статистика получена";
                labelCommandStatus.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                AppendOutput($"❌ Ошибка: {ex.Message}");
                labelCommandStatus.Text = "❌ Ошибка!";
                labelCommandStatus.ForeColor = Color.Red;
            }
        }

        // ====== ИНФОРМАЦИЯ О ПОЛЬЗОВАТЕЛЕ ======
        private void ShowUserInfo()
        {
            var user = AccountManager.GetCurrentUser();
            if (user != null)
            {
                AppendOutput("👤 Информация о пользователе:");
                AppendOutput($"   Логин: {user.Login}");
                AppendOutput($"   Имя: {user.UserName ?? "Не указано"}");
                AppendOutput($"   Дата регистрации: {user.CreatedDate:dd.MM.yyyy HH:mm}");
                AppendOutput($"   Последний вход: {user.LastLogin:dd.MM.yyyy HH:mm}");
                AppendOutput($"   Статус: {(user.IsActivated ? "✅ Активирован" : "❌ Не активирован")}");
            }
            else
            {
                AppendOutput("❌ Пользователь не найден!");
            }
        }

        // ====== ДАТА РЕГИСТРАЦИИ ======
        private void ShowRegistrationDate()
        {
            var user = AccountManager.GetCurrentUser();
            if (user != null)
            {
                AppendOutput($"📅 Дата регистрации: {user.CreatedDate:dd.MM.yyyy HH:mm}");
                AppendOutput($"   Прошло дней: {(DateTime.Now - user.CreatedDate).Days} дней");
            }
            else
            {
                AppendOutput("❌ Пользователь не найден!");
            }
        }

        // ====== ПОМОЩЬ ======
        private void ShowHelp()
        {
            AppendOutput("📋 Доступные команды:");
            AppendOutput("");
            AppendOutput("  check_connection     - Проверить связь с сервером");
            AppendOutput("  code_stats           - Показать статистику кодов");
            AppendOutput("  user                 - Информация о текущем пользователе");
            AppendOutput("  date                 - Дата регистрации пользователя");
            AppendOutput("  version              - Показать текущую версию");
            AppendOutput("  clear                - Очистить вывод");
            AppendOutput("  help                 - Показать эту справку");
            AppendOutput("");
            AppendOutput("💡 Пример: check_connection");
        }

        // ===== ДОБАВЛЕНИЕ В ВЫВОД =====
        private void AppendOutput(string text)
        {
            if (richOutput.InvokeRequired)
            {
                richOutput.Invoke((Action)(() => AppendOutput(text)));
                return;
            }

            richOutput.AppendText(text + "\n");
            richOutput.ScrollToCaret();
        }

        // ===== ОБНОВЛЕНИЯ =====
        private async void OnCheckUpdatesClick(object sender, EventArgs e)
        {
            buttonCheckUpdates.Enabled = false;
            buttonCheckUpdates.Text = "⏳ Проверка...";
            labelVersionStatus.Text = "⏳ Проверка обновлений...";
            labelVersionStatus.ForeColor = Color.DarkBlue;

            try
            {
                string latestVersion = await GetLatestVersionFromGitHub();

                if (string.IsNullOrEmpty(latestVersion))
                {
                    labelVersionStatus.Text = "❌ Не удалось проверить обновления (проверьте интернет)";
                    labelVersionStatus.ForeColor = Color.Red;
                    return;
                }

                if (CompareVersions(currentVersion, latestVersion) == 0)
                {
                    labelVersionStatus.Text = "✅ У вас последняя версия!";
                    labelVersionStatus.ForeColor = Color.DarkGreen;
                }
                else if (CompareVersions(currentVersion, latestVersion) < 0)
                {
                    labelVersionStatus.Text = $"🔄 Доступна новая версия: {latestVersion} (текущая: {currentVersion})";
                    labelVersionStatus.ForeColor = Color.DarkOrange;
                }
                else
                {
                    labelVersionStatus.Text = $"⚠️ Ваша версия ({currentVersion}) новее чем на GitHub ({latestVersion})";
                    labelVersionStatus.ForeColor = Color.DarkOrange;
                }
            }
            catch (Exception ex)
            {
                labelVersionStatus.Text = $"❌ Ошибка: {ex.Message}";
                labelVersionStatus.ForeColor = Color.Red;
            }
            finally
            {
                buttonCheckUpdates.Enabled = true;
                buttonCheckUpdates.Text = "🔄 Проверить обновления";
                ColorHelper.ApplyContrastToControls(this);
            }
        }

        private async void OnPerformUpdateClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "⚠️ Обновление системы заменит все файлы.\n\n" +
                "Рекомендуется сделать резервную копию.\n\n" +
                "Продолжить?",
                "Обновление системы",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                buttonCheckUpdates.Enabled = false;
                buttonCheckUpdates.Text = "⏳ Обновление...";
                labelVersionStatus.Text = "⏳ Загрузка обновлений...";
                labelVersionStatus.ForeColor = Color.DarkBlue;

                string latestVersion = await GetLatestVersionFromGitHub();

                if (string.IsNullOrEmpty(latestVersion))
                {
                    MessageBox.Show("Не удалось получить информацию об обновлениях!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (CompareVersions(currentVersion, latestVersion) >= 0)
                {
                    MessageBox.Show("У вас уже установлена последняя версия!", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                currentVersion = latestVersion;
                labelBuild.Text = currentVersion;
                SaveVersion();

                labelVersionStatus.Text = $"✅ Обновление до версии {latestVersion} выполнено!";
                labelVersionStatus.ForeColor = Color.DarkGreen;

                MessageBox.Show($"✅ Система обновлена до версии {latestVersion}!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                labelVersionStatus.Text = $"❌ Ошибка обновления: {ex.Message}";
                labelVersionStatus.ForeColor = Color.Red;
            }
            finally
            {
                buttonCheckUpdates.Enabled = true;
                buttonCheckUpdates.Text = "🔄 Проверить обновления";
                ColorHelper.ApplyContrastToControls(this);
            }
        }

        // ===== ПОЛУЧЕНИЕ ВЕРСИИ С GITHUB =====
        private async System.Threading.Tasks.Task<string> GetLatestVersionFromGitHub()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    string url = "https://raw.githubusercontent.com/MihailBabushkin/FLooxOC_SV/master/version.txt";

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string version = await response.Content.ReadAsStringAsync();
                        version = version.Trim();

                        if (!string.IsNullOrEmpty(version))
                        {
                            return version;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка загрузки версии: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLatestVersionFromGitHub error: {ex.Message}");
            }

            return currentVersion;
        }

        // ===== СРАВНЕНИЕ ВЕРСИЙ =====
        private int CompareVersions(string v1, string v2)
        {
            try
            {
                string v1Clean = v1.TrimStart('v', 'V');
                string v2Clean = v2.TrimStart('v', 'V');

                var version1 = new Version(v1Clean);
                var version2 = new Version(v2Clean);

                return version1.CompareTo(version2);
            }
            catch
            {
                return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
            }
        }

        // ===== ТИХАЯ ПРОВЕРКА =====
        private async void CheckForUpdatesSilently()
        {
            try
            {
                string latestVersion = await GetLatestVersionFromGitHub();

                if (!string.IsNullOrEmpty(latestVersion) && latestVersion != currentVersion)
                {
                    if (CompareVersions(currentVersion, latestVersion) < 0)
                    {
                        labelVersionStatus.Text = $"🔄 Доступна новая версия: {latestVersion}";
                        labelVersionStatus.ForeColor = Color.DarkOrange;
                        return;
                    }
                }

                labelVersionStatus.Text = "✅ Система актуальна";
                labelVersionStatus.ForeColor = Color.DarkGreen;
            }
            catch
            {
                labelVersionStatus.Text = "ℹ️ Не удалось проверить обновления";
                labelVersionStatus.ForeColor = Color.Gray;
            }
        }
    }
}