using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace FlooxOC
{
    public class InstallerApp : Form
    {
        private ProgressBar progressBar;
        private Label lblStatus, lblProgress;
        private Button btnInstall, btnBrowse, btnCancel;
        private TextBox txtPath;
        private OpenFileDialog openFileDialog;
        private FolderBrowserDialog folderBrowserDialog;
        private string zipPath = "";
        private string installPath = "";
        private Label lblLogo;
        private TextBox txtInstallPath;

        public InstallerApp()
        {
            this.Text = "🚀 Установщик MyOS 95";
            this.Size = new Size(520, 340);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Icon = SystemIcons.Application;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            int y = 15;

            lblLogo = new Label();
            lblLogo.Text = "🖥️ MyOS 95";
            lblLogo.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblLogo.ForeColor = Color.FromArgb(0, 0, 128);
            lblLogo.Location = new Point(20, y);
            lblLogo.Size = new Size(200, 40);
            this.Controls.Add(lblLogo);
            y += 50;

            Label lblInfo = new Label();
            lblInfo.Text = "Выберите ZIP-архив с программой и папку для установки:";
            lblInfo.Font = new Font("Segoe UI", 9);
            lblInfo.Location = new Point(20, y);
            lblInfo.Size = new Size(470, 25);
            this.Controls.Add(lblInfo);
            y += 35;

            Label lblZip = new Label();
            lblZip.Text = "ZIP архив:";
            lblZip.Location = new Point(20, y);
            lblZip.Size = new Size(80, 25);
            this.Controls.Add(lblZip);

            txtPath = new TextBox();
            txtPath.Location = new Point(105, y);
            txtPath.Size = new Size(295, 25);
            txtPath.ReadOnly = true;
            txtPath.BackColor = Color.White;
            this.Controls.Add(txtPath);

            btnBrowse = new Button();
            btnBrowse.Text = "📂 Обзор...";
            btnBrowse.Location = new Point(405, y);
            btnBrowse.Size = new Size(85, 25);
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.BackColor = Color.FromArgb(0, 120, 215);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.Click += BrowseZip;
            this.Controls.Add(btnBrowse);
            y += 40;

            Label lblInstall = new Label();
            lblInstall.Text = "Установка в:";
            lblInstall.Location = new Point(20, y);
            lblInstall.Size = new Size(80, 25);
            this.Controls.Add(lblInstall);

            txtInstallPath = new TextBox();
            txtInstallPath.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "MyOS95");
            txtInstallPath.Location = new Point(105, y);
            txtInstallPath.Size = new Size(295, 25);
            txtInstallPath.Name = "txtInstallPath";
            txtInstallPath.ReadOnly = true;
            txtInstallPath.BackColor = Color.White;
            installPath = txtInstallPath.Text;
            this.Controls.Add(txtInstallPath);

            Button btnInstallPath = new Button();
            btnInstallPath.Text = "📁 Обзор...";
            btnInstallPath.Location = new Point(405, y);
            btnInstallPath.Size = new Size(85, 25);
            btnInstallPath.FlatStyle = FlatStyle.Flat;
            btnInstallPath.BackColor = Color.FromArgb(0, 150, 80);
            btnInstallPath.ForeColor = Color.White;
            btnInstallPath.Cursor = Cursors.Hand;
            btnInstallPath.Click += BrowseInstallPath;
            this.Controls.Add(btnInstallPath);
            y += 45;

            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, y);
            progressBar.Size = new Size(470, 25);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            this.Controls.Add(progressBar);
            y += 30;

            lblStatus = new Label();
            lblStatus.Text = "Готов к установке";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Location = new Point(20, y);
            lblStatus.Size = new Size(350, 25);
            this.Controls.Add(lblStatus);

            lblProgress = new Label();
            lblProgress.Text = "0%";
            lblProgress.Font = new Font("Segoe UI", 9);
            lblProgress.Location = new Point(400, y);
            lblProgress.Size = new Size(90, 25);
            lblProgress.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblProgress);
            y += 45;

            btnInstall = new Button();
            btnInstall.Text = "📦 Установить";
            btnInstall.Size = new Size(130, 45);
            btnInstall.Location = new Point(140, y);
            btnInstall.BackColor = Color.FromArgb(0, 120, 215);
            btnInstall.ForeColor = Color.White;
            btnInstall.FlatStyle = FlatStyle.Flat;
            btnInstall.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnInstall.Cursor = Cursors.Hand;
            btnInstall.Enabled = false;
            btnInstall.Click += InstallApp;
            this.Controls.Add(btnInstall);

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Size = new Size(100, 45);
            btnCancel.Location = new Point(280, y);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 10);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ZIP архивы (*.zip)|*.zip|Все файлы (*.*)|*.*";
            openFileDialog.Title = "Выберите ZIP архив с программой";

            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Выберите папку для установки";
        }

        private void BrowseZip(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                zipPath = openFileDialog.FileName;
                txtPath.Text = zipPath;
                lblStatus.Text = $"✅ Выбран архив: {Path.GetFileName(zipPath)}";
                lblStatus.ForeColor = Color.DarkGreen;
                btnInstall.Enabled = true;
            }
        }

        private void BrowseInstallPath(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                installPath = folderBrowserDialog.SelectedPath;
                txtInstallPath.Text = installPath;
            }
        }

        private async void InstallApp(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
            {
                MessageBox.Show("Выберите ZIP архив!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(installPath))
            {
                MessageBox.Show("Выберите папку для установки!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                btnInstall.Enabled = false;
                btnBrowse.Enabled = false;
                btnCancel.Enabled = false;
                lblStatus.Text = "⏳ Распаковка архива...";
                lblStatus.ForeColor = Color.DarkBlue;
                progressBar.Value = 0;
                lblProgress.Text = "0%";

                if (!Directory.Exists(installPath))
                {
                    Directory.CreateDirectory(installPath);
                }

                // Используем ZipFile
                await System.Threading.Tasks.Task.Run(() =>
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {
                        int totalFiles = archive.Entries.Count;
                        int processedFiles = 0;

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string destPath = Path.Combine(installPath, entry.FullName);

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(destPath);
                            }
                            else
                            {
                                string destDir = Path.GetDirectoryName(destPath);
                                if (!Directory.Exists(destDir))
                                {
                                    Directory.CreateDirectory(destDir);
                                }
                                entry.ExtractToFile(destPath, true);
                            }

                            processedFiles++;
                            int progress = Math.Min((int)((processedFiles / (double)totalFiles) * 100), 100);

                            this.BeginInvoke((Action)(() =>
                            {
                                progressBar.Value = progress;
                                lblProgress.Text = $"{progress}%";
                                lblStatus.Text = $"⏳ Распаковка: {entry.FullName}";
                            }));
                        }
                    }
                });

                // Ищем .sln файл
                string slnFile = "";
                foreach (string file in Directory.GetFiles(installPath, "*.sln", SearchOption.TopDirectoryOnly))
                {
                    slnFile = file;
                    break;
                }

                if (string.IsNullOrEmpty(slnFile))
                {
                    foreach (string file in Directory.GetFiles(installPath, "*.sln", SearchOption.AllDirectories))
                    {
                        slnFile = file;
                        break;
                    }
                }

                lblStatus.Text = "✅ Установка завершена!";
                lblStatus.ForeColor = Color.DarkGreen;
                progressBar.Value = 100;
                lblProgress.Text = "100%";

                if (!string.IsNullOrEmpty(slnFile))
                {
                    CreateShortcut(slnFile);
                    MessageBox.Show(
                        $"✅ Установка успешно завершена!\n\n" +
                        $"📁 Папка: {installPath}\n" +
                        $"📄 Файл проекта: {Path.GetFileName(slnFile)}\n" +
                        $"🖥️ Ярлык создан на рабочем столе",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"✅ Установка завершена!\n\n" +
                        $"📁 Папка: {installPath}\n" +
                        $"⚠️ Файл решения (.sln) не найден.\n" +
                        $"Ярлык не создан.",
                        "Информация",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                btnInstall.Text = "✅ Установлено";
                btnInstall.BackColor = Color.FromArgb(0, 150, 80);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка установки:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Ошибка установки";
                lblStatus.ForeColor = Color.Red;
                btnInstall.Enabled = true;
                btnBrowse.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private void CreateShortcut(string targetPath)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutName = Path.GetFileNameWithoutExtension(targetPath) + ".lnk";
                string shortcutPath = Path.Combine(desktopPath, shortcutName);

                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.Description = $"Проект {Path.GetFileName(targetPath)}";
                shortcut.Save();

                lblStatus.Text = $"✅ Ярлык создан: {shortcutName}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"⚠️ Не удалось создать ярлык: {ex.Message}";
                lblStatus.ForeColor = Color.DarkOrange;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                openFileDialog?.Dispose();
                folderBrowserDialog?.Dispose();
                progressBar?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}