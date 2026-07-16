using FlooxOC;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FlooxOC
{
    public class AddAppDialog : Form
    {
        private TextBox txtName, txtPath, txtArgs, txtWorkDir;
        private PictureBox iconPreview;
        private Button btnBrowseExe, btnBrowseIcon, btnBrowseDir, btnAdd, btnCancel;
        private string selectedIconPath = "";
        private AppInfo editingApp = null;

        public AppInfo ResultApp { get; private set; }

        public AddAppDialog(AppInfo editApp = null)
        {
            this.Text = editApp == null ? "Добавить приложение" : "Редактировать приложение";
            this.Size = new Size(600, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);

            editingApp = editApp;
            InitializeControls();

            if (editApp != null)
                LoadAppData(editApp);
        }

        private void InitializeControls()
        {
            int y = 20;
            int labelWidth = 140;

            // ===== ИКОНКА (ПРЕДПРОСМОТР) =====
            Label lblIcon = new Label();
            lblIcon.Text = "Иконка:";
            lblIcon.Location = new Point(20, y);
            lblIcon.Size = new Size(labelWidth, 25);
            lblIcon.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblIcon);

            iconPreview = new PictureBox();
            iconPreview.Size = new Size(64, 64);
            iconPreview.Location = new Point(170, y - 5);
            iconPreview.SizeMode = PictureBoxSizeMode.StretchImage;
            iconPreview.BackColor = Color.White;
            iconPreview.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(iconPreview);

            // Кнопка извлечения иконки из файла
            Button btnExtractIcon = new Button();
            btnExtractIcon.Text = "📥 Извлечь из .exe";
            btnExtractIcon.Size = new Size(120, 30);
            btnExtractIcon.Location = new Point(245, y);
            btnExtractIcon.FlatStyle = FlatStyle.Flat;
            btnExtractIcon.BackColor = Color.FromArgb(0, 120, 215);
            btnExtractIcon.ForeColor = Color.White;
            btnExtractIcon.Cursor = Cursors.Hand;
            btnExtractIcon.Click += (s, e) => ExtractIconFromExe();
            this.Controls.Add(btnExtractIcon);

            // Кнопка выбора иконки
            btnBrowseIcon = new Button();
            btnBrowseIcon.Text = "📂 Выбрать...";
            btnBrowseIcon.Size = new Size(80, 30);
            btnBrowseIcon.Location = new Point(375, y);
            btnBrowseIcon.FlatStyle = FlatStyle.Flat;
            btnBrowseIcon.BackColor = Color.FromArgb(150, 150, 150);
            btnBrowseIcon.ForeColor = Color.White;
            btnBrowseIcon.Cursor = Cursors.Hand;
            btnBrowseIcon.Click += BrowseIcon;
            this.Controls.Add(btnBrowseIcon);

            y += 50;

            // ===== НАЗВАНИЕ =====
            Label lblName = new Label();
            lblName.Text = "Название:";
            lblName.Location = new Point(20, y);
            lblName.Size = new Size(labelWidth, 25);
            lblName.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblName);

            txtName = new TextBox();
            txtName.Location = new Point(170, y);
            txtName.Size = new Size(350, 25);
            txtName.Font = new Font("Segoe UI", 10);
            this.Controls.Add(txtName);
            y += 40;

            // ===== ПУТЬ К .EXE =====
            Label lblPath = new Label();
            lblPath.Text = "Путь к .exe:";
            lblPath.Location = new Point(20, y);
            lblPath.Size = new Size(labelWidth, 25);
            lblPath.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblPath);

            txtPath = new TextBox();
            txtPath.Location = new Point(170, y);
            txtPath.Size = new Size(300, 25);
            txtPath.Font = new Font("Segoe UI", 10);
            txtPath.TextChanged += (s, e) => UpdatePreview();
            this.Controls.Add(txtPath);

            btnBrowseExe = new Button();
            btnBrowseExe.Text = "Обзор...";
            btnBrowseExe.Size = new Size(70, 25);
            btnBrowseExe.Location = new Point(475, y);
            btnBrowseExe.FlatStyle = FlatStyle.Flat;
            btnBrowseExe.BackColor = Color.FromArgb(0, 120, 215);
            btnBrowseExe.ForeColor = Color.White;
            btnBrowseExe.Cursor = Cursors.Hand;
            btnBrowseExe.Click += BrowseExe;
            this.Controls.Add(btnBrowseExe);
            y += 40;

            // ===== АРГУМЕНТЫ =====
            Label lblArgs = new Label();
            lblArgs.Text = "Аргументы:";
            lblArgs.Location = new Point(20, y);
            lblArgs.Size = new Size(labelWidth, 25);
            lblArgs.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblArgs);

            txtArgs = new TextBox();
            txtArgs.Location = new Point(170, y);
            txtArgs.Size = new Size(350, 25);
            txtArgs.Font = new Font("Segoe UI", 10);
            this.Controls.Add(txtArgs);
            y += 40;

            // ===== РАБОЧАЯ ПАПКА =====
            Label lblWorkDir = new Label();
            lblWorkDir.Text = "Рабочая папка:";
            lblWorkDir.Location = new Point(20, y);
            lblWorkDir.Size = new Size(labelWidth, 25);
            lblWorkDir.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblWorkDir);

            txtWorkDir = new TextBox();
            txtWorkDir.Location = new Point(170, y);
            txtWorkDir.Size = new Size(300, 25);
            txtWorkDir.Font = new Font("Segoe UI", 10);
            this.Controls.Add(txtWorkDir);

            btnBrowseDir = new Button();
            btnBrowseDir.Text = "Обзор...";
            btnBrowseDir.Size = new Size(70, 25);
            btnBrowseDir.Location = new Point(475, y);
            btnBrowseDir.FlatStyle = FlatStyle.Flat;
            btnBrowseDir.BackColor = Color.FromArgb(0, 120, 215);
            btnBrowseDir.ForeColor = Color.White;
            btnBrowseDir.Cursor = Cursors.Hand;
            btnBrowseDir.Click += BrowseDirectory;
            this.Controls.Add(btnBrowseDir);
            y += 50;

            // ===== КНОПКИ =====
            btnAdd = new Button();
            btnAdd.Text = editingApp == null ? "✅ Добавить" : "💾 Сохранить";
            btnAdd.Size = new Size(120, 40);
            btnAdd.Location = new Point(170, y);
            btnAdd.BackColor = Color.FromArgb(0, 120, 215);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += AddApp;
            this.Controls.Add(btnAdd);

            btnCancel = new Button();
            btnCancel.Text = "❌ Отмена";
            btnCancel.Size = new Size(120, 40);
            btnCancel.Location = new Point(300, y);
            btnCancel.BackColor = Color.FromArgb(200, 80, 80);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        // ====== ИЗВЛЕЧЕНИЕ ИКОНКИ ИЗ .EXE ======
        private void ExtractIconFromExe()
        {
            string path = txtPath.Text.Trim();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                MessageBox.Show("Сначала выберите .exe файл!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Image icon = AppManager.ExtractIconFromFile(path);
                if (icon != null)
                {
                    iconPreview.Image = icon;
                    selectedIconPath = path;
                    MessageBox.Show("Иконка успешно извлечена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось извлечь иконку из файла.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePreview()
        {
            string path = txtPath.Text.Trim();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                // Обновляем название, если оно пустое
                if (string.IsNullOrEmpty(txtName.Text) || editingApp == null)
                {
                    txtName.Text = Path.GetFileNameWithoutExtension(path);
                }

                // Обновляем рабочую папку
                if (string.IsNullOrEmpty(txtWorkDir.Text))
                {
                    txtWorkDir.Text = Path.GetDirectoryName(path);
                }
            }
        }

        private void BrowseExe(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Исполняемые файлы (*.exe)|*.exe|Ярлыки (*.lnk)|*.lnk|Все файлы (*.*)|*.*";
                ofd.Title = "Выберите программу";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = ofd.FileName;

                    // Автозаполнение
                    if (string.IsNullOrEmpty(txtName.Text) || editingApp == null)
                        txtName.Text = Path.GetFileNameWithoutExtension(ofd.FileName);

                    if (string.IsNullOrEmpty(txtWorkDir.Text))
                        txtWorkDir.Text = Path.GetDirectoryName(ofd.FileName);

                    // Автоматически извлекаем иконку
                    try
                    {
                        Image icon = AppManager.ExtractIconFromFile(ofd.FileName);
                        if (icon != null)
                        {
                            iconPreview.Image = icon;
                            selectedIconPath = ofd.FileName;
                        }
                    }
                    catch { }
                }
            }
        }

        private void BrowseIcon(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Изображения (*.png;*.jpg;*.bmp;*.ico)|*.png;*.jpg;*.bmp;*.ico|Все файлы (*.*)|*.*";
                ofd.Title = "Выберите иконку";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        selectedIconPath = ofd.FileName;
                        iconPreview.Image = Image.FromFile(ofd.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки иконки: {ex.Message}");
                    }
                }
            }
        }

        private void BrowseDirectory(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Выберите рабочую папку";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtWorkDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void LoadAppData(AppInfo app)
        {
            txtName.Text = app.Name;
            txtPath.Text = app.ExecutablePath;
            txtArgs.Text = app.Arguments;
            txtWorkDir.Text = app.WorkingDirectory;

            if (!string.IsNullOrEmpty(app.IconPath) && File.Exists(app.IconPath))
            {
                try
                {
                    iconPreview.Image = Image.FromFile(app.IconPath);
                    selectedIconPath = app.IconPath;
                }
                catch { }
            }
        }

        private void AddApp(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название приложения!");
                return;
            }

            if (string.IsNullOrEmpty(txtPath.Text) || !File.Exists(txtPath.Text))
            {
                MessageBox.Show("Выберите корректный .exe файл!");
                return;
            }

            ResultApp = new AppInfo
            {
                Name = txtName.Text,
                ExecutablePath = txtPath.Text,
                Arguments = txtArgs.Text,
                WorkingDirectory = txtWorkDir.Text,
                IconPath = selectedIconPath
            };

            if (editingApp != null)
                ResultApp.Id = editingApp.Id;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}