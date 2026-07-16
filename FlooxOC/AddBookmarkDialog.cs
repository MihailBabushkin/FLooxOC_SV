using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FlooxOC
{
    public class AddBookmarkDialog : Form
    {
        private TextBox txtName, txtUrl;
        private PictureBox iconPreview;
        private Button btnBrowseIcon, btnAdd, btnCancel;
        private Button btnGetFavicon;
        private string selectedIconPath = "";
        private WebBookmark editingBookmark = null;

        public WebBookmark ResultBookmark { get; private set; }

        public AddBookmarkDialog(WebBookmark editBookmark = null)
        {
            this.Text = editBookmark == null ? "Добавить закладку" : "Редактировать закладку";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);

            editingBookmark = editBookmark;
            InitializeControls();

            if (editBookmark != null)
                LoadBookmarkData(editBookmark);
        }

        private void InitializeControls()
        {
            int y = 20;

            // Название
            AddLabel("Название:", 20, y);
            txtName = new TextBox { Location = new Point(130, y - 3), Size = new Size(310, 20) };
            this.Controls.Add(txtName);

            y += 40;

            // URL
            AddLabel("URL:", 20, y);
            txtUrl = new TextBox { Location = new Point(130, y - 3), Size = new Size(310, 20) };
            txtUrl.Text = "https://";
            this.Controls.Add(txtUrl);

            y += 40;

            // Иконка
            AddLabel("Иконка:", 20, y);

            btnBrowseIcon = new Button
            {
                Text = "Выбрать...",
                Location = new Point(130, y - 3),
                Size = new Size(80, 25)
            };
            btnBrowseIcon.Click += BrowseIcon;

            btnGetFavicon = new Button
            {
                Text = "Скачать favicon",
                Location = new Point(215, y - 3),
                Size = new Size(110, 25)
            };
            btnGetFavicon.Click += DownloadFavicon;

            iconPreview = new PictureBox
            {
                Location = new Point(335, y - 5),
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(btnBrowseIcon);
            this.Controls.Add(btnGetFavicon);
            this.Controls.Add(iconPreview);

            y += 60;

            // Кнопки
            btnAdd = new Button
            {
                Text = editingBookmark == null ? "Добавить" : "Сохранить",
                Location = new Point(130, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.Click += AddBookmark;

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(240, y),
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
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lbl);
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

        private async void DownloadFavicon(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Введите URL сайта!");
                return;
            }

            try
            {
                btnGetFavicon.Enabled = false;
                btnGetFavicon.Text = "Загрузка...";

                // Добавляем обработку ошибок
                Image icon = null;
                try
                {
                    icon = await System.Threading.Tasks.Task.Run(() =>
                        BookmarkManager.GenerateFaviconFromUrl(url));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки favicon: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (icon != null)
                {
                    // Сохраняем временно
                    string tempPath = Path.GetTempFileName() + ".png";
                    icon.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
                    selectedIconPath = tempPath;
                    iconPreview.Image = icon;
                    MessageBox.Show("Иконка загружена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить иконку с сайта.\n" +
                        "Попробуйте выбрать иконку вручную.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGetFavicon.Enabled = true;
                btnGetFavicon.Text = "Скачать favicon";
            }
        }

        private void LoadBookmarkData(WebBookmark bookmark)
        {
            txtName.Text = bookmark.Name;
            txtUrl.Text = bookmark.Url;

            if (!string.IsNullOrEmpty(bookmark.IconPath) && File.Exists(bookmark.IconPath))
            {
                try
                {
                    iconPreview.Image = Image.FromFile(bookmark.IconPath);
                    selectedIconPath = bookmark.IconPath;
                }
                catch { }
            }
        }

        private void AddBookmark(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            if (string.IsNullOrEmpty(txtUrl.Text))
            {
                MessageBox.Show("Введите URL!");
                return;
            }

            // Проверяем URL
            string url = txtUrl.Text.Trim();
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            ResultBookmark = new WebBookmark
            {
                Name = txtName.Text,
                Url = url,
                IconPath = selectedIconPath
            };

            if (editingBookmark != null)
                ResultBookmark.Id = editingBookmark.Id;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}