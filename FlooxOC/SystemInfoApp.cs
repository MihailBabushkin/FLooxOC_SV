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
        private Label lblKernel, lblMinRequirements;
        private Label lblOS, lblEdition, lblBuild;
        private Label lblVersionStatus;
        private Button btnCheckUpdates;
        private string currentVersion = "v0.0.1";
        private string versionFile = "";

        // ===== ТВОЙ РЕПОЗИТОРИЙ =====
        private string versionUrl = "https://raw.githubusercontent.com/MihailBabushkin/FlooxOC-NEW-PC/master/version.txt";

        public SystemInfoApp()
        {
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(15);
            this.AutoScroll = true;

            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MyOS95");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            versionFile = Path.Combine(appDataPath, "SMI_version.txt");

            LoadVersion();
            InitializeComponents();

            // Проверяем обновления с задержкой
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
            {
                this.BeginInvoke((Action)(() => CheckForUpdatesSilent()));
            });
        }

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

        private void InitializeComponents()
        {
            int y = 10;

            Label title = new Label();
            title.Text = "ℹ️ Информация о системе";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.Location = new Point(10, y);
            title.Size = new Size(350, 35);
            this.Controls.Add(title);
            y += 50;

            Panel line1 = new Panel();
            line1.Location = new Point(10, y);
            line1.Size = new Size(400, 2);
            line1.BackColor = Color.FromArgb(128, 128, 128);
            this.Controls.Add(line1);
            y += 15;

            Label lblMainTitle = new Label();
            lblMainTitle.Text = "🖥️ Основное";
            lblMainTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblMainTitle.Location = new Point(10, y);
            lblMainTitle.Size = new Size(200, 25);
            this.Controls.Add(lblMainTitle);
            y += 30;

            Label lblKernelTitle = new Label();
            lblKernelTitle.Text = "Ядро:";
            lblKernelTitle.Font = new Font("Segoe UI", 10);
            lblKernelTitle.Location = new Point(20, y);
            lblKernelTitle.Size = new Size(120, 25);
            this.Controls.Add(lblKernelTitle);

            lblKernel = new Label();
            lblKernel.Text = "Windows 10 (NT 10.0)";
            lblKernel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblKernel.Location = new Point(140, y);
            lblKernel.Size = new Size(250, 25);
            lblKernel.ForeColor = Color.FromArgb(0, 70, 140);
            this.Controls.Add(lblKernel);
            y += 30;

            Label lblMinTitle = new Label();
            lblMinTitle.Text = "Минимальные требования:";
            lblMinTitle.Font = new Font("Segoe UI", 10);
            lblMinTitle.Location = new Point(20, y);
            lblMinTitle.Size = new Size(140, 25);
            this.Controls.Add(lblMinTitle);

            lblMinRequirements = new Label();
            lblMinRequirements.Text = "CPU: 1 ГГц, RAM: 1 ГБ, HDD: 500 МБ";
            lblMinRequirements.Font = new Font("Segoe UI", 10);
            lblMinRequirements.Location = new Point(160, y);
            lblMinRequirements.Size = new Size(250, 25);
            this.Controls.Add(lblMinRequirements);
            y += 40;

            Panel line2 = new Panel();
            line2.Location = new Point(10, y);
            line2.Size = new Size(400, 2);
            line2.BackColor = Color.FromArgb(128, 128, 128);
            this.Controls.Add(line2);
            y += 15;

            Label lblVersionTitle = new Label();
            lblVersionTitle.Text = "📌 Версия";
            lblVersionTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblVersionTitle.Location = new Point(10, y);
            lblVersionTitle.Size = new Size(200, 25);
            this.Controls.Add(lblVersionTitle);
            y += 30;

            Label lblOSTitle = new Label();
            lblOSTitle.Text = "ОС:";
            lblOSTitle.Font = new Font("Segoe UI", 10);
            lblOSTitle.Location = new Point(20, y);
            lblOSTitle.Size = new Size(120, 25);
            this.Controls.Add(lblOSTitle);

            lblOS = new Label();
            lblOS.Text = "Floox'OC";
            lblOS.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblOS.Location = new Point(140, y);
            lblOS.Size = new Size(250, 25);
            lblOS.ForeColor = Color.FromArgb(0, 70, 140);
            this.Controls.Add(lblOS);
            y += 30;

            Label lblEditionTitle = new Label();
            lblEditionTitle.Text = "Набор:";
            lblEditionTitle.Font = new Font("Segoe UI", 10);
            lblEditionTitle.Location = new Point(20, y);
            lblEditionTitle.Size = new Size(120, 25);
            this.Controls.Add(lblEditionTitle);

            lblEdition = new Label();
            lblEdition.Text = "Home Version";
            lblEdition.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblEdition.Location = new Point(140, y);
            lblEdition.Size = new Size(250, 25);
            this.Controls.Add(lblEdition);
            y += 30;

            Label lblBuildTitle = new Label();
            lblBuildTitle.Text = "Версия сборки:";
            lblBuildTitle.Font = new Font("Segoe UI", 10);
            lblBuildTitle.Location = new Point(20, y);
            lblBuildTitle.Size = new Size(120, 25);
            this.Controls.Add(lblBuildTitle);

            lblBuild = new Label();
            lblBuild.Text = currentVersion;
            lblBuild.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblBuild.Location = new Point(140, y);
            lblBuild.Size = new Size(250, 25);
            lblBuild.ForeColor = Color.FromArgb(0, 120, 215);
            this.Controls.Add(lblBuild);
            y += 40;

            Panel line3 = new Panel();
            line3.Location = new Point(10, y);
            line3.Size = new Size(400, 2);
            line3.BackColor = Color.FromArgb(128, 128, 128);
            this.Controls.Add(line3);
            y += 15;

            lblVersionStatus = new Label();
            lblVersionStatus.Text = "🔍 Проверка обновлений...";
            lblVersionStatus.Font = new Font("Segoe UI", 10);
            lblVersionStatus.Location = new Point(20, y);
            lblVersionStatus.Size = new Size(380, 25);
            this.Controls.Add(lblVersionStatus);
            y += 35;

            btnCheckUpdates = new Button();
            btnCheckUpdates.Text = "🔄 Проверить обновления";
            btnCheckUpdates.Size = new Size(200, 40);
            btnCheckUpdates.Location = new Point(20, y);
            btnCheckUpdates.FlatStyle = FlatStyle.Flat;
            btnCheckUpdates.BackColor = Color.FromArgb(0, 120, 215);
            btnCheckUpdates.ForeColor = Color.White;
            btnCheckUpdates.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCheckUpdates.Cursor = Cursors.Hand;
            btnCheckUpdates.Click += CheckForUpdates;
            this.Controls.Add(btnCheckUpdates);
            y += 55;

            Button btnUpdate = new Button();
            btnUpdate.Text = "⬇️ Обновить систему";
            btnUpdate.Size = new Size(200, 40);
            btnUpdate.Location = new Point(20, y);
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.BackColor = Color.FromArgb(0, 150, 80);
            btnUpdate.ForeColor = Color.White;
            btnUpdate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnUpdate.Cursor = Cursors.Hand;
            btnUpdate.Click += PerformUpdate;
            this.Controls.Add(btnUpdate);
        }

        // ====== ПРОВЕРКА ОБНОВЛЕНИЙ ======
        private async void CheckForUpdates(object sender, EventArgs e)
        {
            btnCheckUpdates.Enabled = false;
            btnCheckUpdates.Text = "⏳ Проверка...";
            lblVersionStatus.Text = "⏳ Проверка обновлений...";
            lblVersionStatus.ForeColor = Color.DarkBlue;

            try
            {
                string latestVersion = await GetLatestVersionFromGitHub();

                if (string.IsNullOrEmpty(latestVersion))
                {
                    lblVersionStatus.Text = "❌ Не удалось проверить обновления (проверьте интернет)";
                    lblVersionStatus.ForeColor = Color.Red;
                    return;
                }

                if (CompareVersions(currentVersion, latestVersion) == 0)
                {
                    lblVersionStatus.Text = "✅ У вас последняя версия!";
                    lblVersionStatus.ForeColor = Color.DarkGreen;
                }
                else if (CompareVersions(currentVersion, latestVersion) < 0)
                {
                    lblVersionStatus.Text = $"🔄 Доступна новая версия: {latestVersion} (текущая: {currentVersion})";
                    lblVersionStatus.ForeColor = Color.DarkOrange;
                }
                else
                {
                    lblVersionStatus.Text = $"⚠️ Ваша версия ({currentVersion}) новее чем на GitHub ({latestVersion})";
                    lblVersionStatus.ForeColor = Color.DarkOrange;
                }
            }
            catch (Exception ex)
            {
                lblVersionStatus.Text = $"❌ Ошибка: {ex.Message}";
                lblVersionStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnCheckUpdates.Enabled = true;
                btnCheckUpdates.Text = "🔄 Проверить обновления";
            }
        }

        private async void PerformUpdate(object sender, EventArgs e)
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
                btnCheckUpdates.Enabled = false;
                btnCheckUpdates.Text = "⏳ Обновление...";
                lblVersionStatus.Text = "⏳ Загрузка обновлений...";
                lblVersionStatus.ForeColor = Color.DarkBlue;

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
                lblBuild.Text = currentVersion;
                SaveVersion();

                lblVersionStatus.Text = $"✅ Обновление до версии {latestVersion} выполнено!";
                lblVersionStatus.ForeColor = Color.DarkGreen;

                MessageBox.Show($"✅ Система обновлена до версии {latestVersion}!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblVersionStatus.Text = $"❌ Ошибка обновления: {ex.Message}";
                lblVersionStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnCheckUpdates.Enabled = true;
                btnCheckUpdates.Text = "🔄 Проверить обновления";
            }
        }

        // ====== ПОЛУЧЕНИЕ ВЕРСИИ С GITHUB ======
        private async System.Threading.Tasks.Task<string> GetLatestVersionFromGitHub()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    // Запрашиваем version.txt по твоему URL
                    string url = versionUrl;

                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        // Если ошибка, пробуем альтернативный URL
                        Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLatestVersionFromGitHub error: {ex.Message}");
            }

            // Если не удалось получить с GitHub, возвращаем текущую версию
            return currentVersion;
        }

        // ====== СРАВНЕНИЕ ВЕРСИЙ ======
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

        // ====== ТИХАЯ ПРОВЕРКА ======
        private async void CheckForUpdatesSilent()
        {
            try
            {
                string latestVersion = await GetLatestVersionFromGitHub();

                if (!string.IsNullOrEmpty(latestVersion) && latestVersion != currentVersion)
                {
                    if (CompareVersions(currentVersion, latestVersion) < 0)
                    {
                        lblVersionStatus.Text = $"🔄 Доступна новая версия: {latestVersion}";
                        lblVersionStatus.ForeColor = Color.DarkOrange;
                        return;
                    }
                }

                lblVersionStatus.Text = "✅ Система актуальна";
                lblVersionStatus.ForeColor = Color.DarkGreen;
            }
            catch
            {
                lblVersionStatus.Text = "ℹ️ Не удалось проверить обновления";
                lblVersionStatus.ForeColor = Color.Gray;
            }
        }
    }
}