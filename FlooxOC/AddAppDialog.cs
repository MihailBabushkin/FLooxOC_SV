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
            this.Size = new Size(550, 500);
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
            // Название
            AddLabel("Название:", 20, y);
            txtName = new TextBox { Location = new Point(150, y - 3), Size = new Size(350, 20) };
            this.Controls.Add(txtName);

            y += 40;

            // Путь к .exe
            AddLabel("Путь к .exe:", 20, y);
            txtPath = new TextBox { Location = new Point(150, y - 3), Size = new Size(300, 20) };
            btnBrowseExe = new Button { Text = "Обзор...", Location = new Point(455, y - 3), Size = new Size(45, 25) };
            btnBrowseExe.Click += BrowseExe;
            this.Controls.Add(txtPath);
            this.Controls.Add(btnBrowseExe);

            y += 40;

            // Аргументы
            AddLabel("Аргументы:", 20, y);
            txtArgs = new TextBox { Location = new Point(150, y - 3), Size = new Size(350, 20) };
            this.Controls.Add(txtArgs);

            y += 40;

            // Рабочая папка
            AddLabel("Рабочая папка:", 20, y);
            txtWorkDir = new TextBox { Location = new Point(150, y - 3), Size = new Size(300, 20) };
            btnBrowseDir = new Button { Text = "Обзор...", Location = new Point(455, y - 3), Size = new Size(45, 25) };
            btnBrowseDir.Click += BrowseDirectory;
            this.Controls.Add(txtWorkDir);
            this.Controls.Add(btnBrowseDir);

            y += 40;

            // Иконка
            AddLabel("Иконка:", 20, y);
            btnBrowseIcon = new Button { Text = "Выбрать иконку...", Location = new Point(150, y - 3), Size = new Size(120, 25) };
            btnBrowseIcon.Click += BrowseIcon;

            iconPreview = new PictureBox
            {
                Location = new Point(280, y - 3),
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(btnBrowseIcon);
            this.Controls.Add(iconPreview);

            y += 60;

            // Кнопки
            btnAdd = new Button
            {
                Text = editingApp == null ? "Добавить" : "Сохранить",
                Location = new Point(150, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.Click += AddApp;

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(260, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(200, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnAdd);
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(130, 20),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lbl);
        }

        private void BrowseExe(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = ofd.FileName;

                    // Автозаполнение названия
                    if (string.IsNullOrEmpty(txtName.Text) || editingApp == null)
                        txtName.Text = Path.GetFileNameWithoutExtension(ofd.FileName);

                    // Автозаполнение рабочей папки
                    if (string.IsNullOrEmpty(txtWorkDir.Text))
                        txtWorkDir.Text = Path.GetDirectoryName(ofd.FileName);

                    // Автоматически извлекаем иконку
                    if (string.IsNullOrEmpty(selectedIconPath))
                    {
                        try
                        {
                            var icon = System.Drawing.Icon.ExtractAssociatedIcon(ofd.FileName);
                            if (icon != null)
                            {
                                iconPreview.Image = icon.ToBitmap();
                                // Сохраняем иконку временно
                                selectedIconPath = ofd.FileName;
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private void BrowseIcon(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Изображения (*.png;*.jpg;*.bmp;*.ico)|*.png;*.jpg;*.bmp;*.ico|Все файлы (*.*)|*.*";
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