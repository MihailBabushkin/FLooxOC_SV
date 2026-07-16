using FlooxOC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class SettingsApp : Panel
    {
        private ColorDialog colorDialog;
        private FlowLayoutPanel wallpaperPanel;
        private Label lblStatus;
        private Form1 mainForm;
        private Color selectedWallpaperColor;
        private Color selectedAppColor;

        private Color[] wallpaperColors = new Color[]
        {
            Color.FromArgb(0, 128, 128), Color.FromArgb(0, 70, 140),
            Color.FromArgb(0, 100, 0), Color.FromArgb(128, 0, 0),
            Color.FromArgb(128, 0, 128), Color.FromArgb(32, 32, 32),
            Color.FromArgb(64, 64, 64), Color.FromArgb(20, 20, 40),
            Color.FromArgb(40, 20, 20), Color.FromArgb(20, 40, 20),
            Color.FromArgb(240, 240, 240), Color.FromArgb(220, 220, 220),
            Color.FromArgb(200, 220, 255), Color.FromArgb(255, 220, 220),
            Color.FromArgb(220, 255, 220), Color.FromArgb(0, 100, 200),
            Color.FromArgb(0, 150, 255), Color.FromArgb(50, 100, 200),
            Color.FromArgb(0, 50, 150), Color.FromArgb(100, 150, 255),
            Color.FromArgb(0, 150, 0), Color.FromArgb(50, 200, 50),
            Color.FromArgb(0, 100, 50), Color.FromArgb(100, 200, 100),
            Color.FromArgb(50, 150, 100), Color.FromArgb(200, 50, 50),
            Color.FromArgb(255, 100, 50), Color.FromArgb(200, 100, 50),
            Color.FromArgb(255, 150, 50), Color.FromArgb(150, 50, 50),
            Color.FromArgb(150, 50, 200), Color.FromArgb(200, 50, 150),
            Color.FromArgb(255, 100, 200), Color.FromArgb(100, 50, 150),
            Color.FromArgb(200, 150, 255), Color.FromArgb(128, 128, 128),
            Color.FromArgb(160, 160, 160), Color.FromArgb(96, 96, 96),
            Color.FromArgb(200, 200, 200), Color.FromArgb(80, 80, 80),
        };

        private string[] wallpaperNames = new string[]
        {
            "Teal", "Синий", "Зелёный", "Красный", "Фиолетовый",
            "Чёрный", "Тёмно-серый", "Тёмно-синий", "Тёмно-красный", "Тёмно-зелёный",
            "Белый", "Светло-серый", "Светло-голубой", "Светло-розовый", "Светло-зелёный",
            "Ярко-синий", "Голубой", "Серо-синий", "Тёмно-синий", "Светло-синий",
            "Ярко-зелёный", "Салатовый", "Тёмно-зелёный", "Светло-зелёный", "Мятный",
            "Красный", "Оранжевый", "Коричневый", "Светло-оранжевый", "Бордовый",
            "Фиолетовый", "Розовый", "Светло-розовый", "Тёмно-фиолетовый", "Светло-фиолетовый",
            "Серый", "Светло-серый", "Тёмно-серый", "Очень светло-серый", "Тёмно-серый"
        };

        public SettingsApp(Form1 form)
        {
            mainForm = form;
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(15);
            this.AutoScroll = true;

            selectedWallpaperColor = mainForm.BackColor;
            selectedAppColor = Color.White;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            int y = 20;

            // ===== ЗАГОЛОВОК =====
            Label title = new Label();
            title.Text = "⚙️ Настройки системы";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.Location = new Point(10, y);
            title.Size = new Size(400, 35);
            this.Controls.Add(title);
            y += 50;

            // ===== ФОН РАБОЧЕГО СТОЛА =====
            Label lblWallpaper = new Label();
            lblWallpaper.Text = "Фон рабочего стола:";
            lblWallpaper.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblWallpaper.Location = new Point(10, y);
            lblWallpaper.Size = new Size(180, 25);
            this.Controls.Add(lblWallpaper);
            y += 30;

            wallpaperPanel = new FlowLayoutPanel();
            wallpaperPanel.Location = new Point(10, y);
            wallpaperPanel.Size = new Size(530, 180);
            wallpaperPanel.AutoScroll = true;
            wallpaperPanel.BackColor = Color.FromArgb(220, 220, 220);
            wallpaperPanel.Padding = new Padding(5);
            wallpaperPanel.FlowDirection = FlowDirection.LeftToRight;
            wallpaperPanel.WrapContents = true;
            this.Controls.Add(wallpaperPanel);

            for (int i = 0; i < wallpaperColors.Length; i++)
            {
                Button colorBtn = new Button();
                colorBtn.Size = new Size(45, 40);
                colorBtn.BackColor = wallpaperColors[i];
                colorBtn.FlatStyle = FlatStyle.Flat;
                colorBtn.FlatAppearance.BorderSize = 1;
                colorBtn.FlatAppearance.BorderColor = Color.Black;
                colorBtn.Cursor = Cursors.Hand;

                ToolTip toolTip = new ToolTip();
                toolTip.SetToolTip(colorBtn, wallpaperNames[i]);

                int index = i;
                colorBtn.Click += (s, e) => SetWallpaper(wallpaperColors[index]);

                wallpaperPanel.Controls.Add(colorBtn);
            }

            y += 190;

            Panel previewPanel = new Panel();
            previewPanel.Size = new Size(60, 40);
            previewPanel.Location = new Point(10, y);
            previewPanel.BackColor = selectedWallpaperColor;
            previewPanel.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(previewPanel);

            Button customColorBtn = new Button();
            customColorBtn.Text = "🎨 Свой цвет...";
            customColorBtn.Size = new Size(130, 40);
            customColorBtn.Location = new Point(80, y);
            customColorBtn.FlatStyle = FlatStyle.Flat;
            customColorBtn.BackColor = Color.FromArgb(0, 120, 215);
            customColorBtn.ForeColor = Color.White;
            customColorBtn.Cursor = Cursors.Hand;
            customColorBtn.Click += ChooseCustomWallpaper;
            this.Controls.Add(customColorBtn);

            y += 50;

            // ===== ФОН ПРИЛОЖЕНИЙ =====
            Label lblAppBg = new Label();
            lblAppBg.Text = "Фон приложений:";
            lblAppBg.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblAppBg.Location = new Point(10, y);
            lblAppBg.Size = new Size(180, 25);
            this.Controls.Add(lblAppBg);
            y += 30;

            Panel appPreviewPanel = new Panel();
            appPreviewPanel.Size = new Size(60, 40);
            appPreviewPanel.Location = new Point(10, y);
            appPreviewPanel.BackColor = selectedAppColor;
            appPreviewPanel.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(appPreviewPanel);

            Button appColorBtn = new Button();
            appColorBtn.Text = "Выбрать цвет...";
            appColorBtn.Size = new Size(130, 40);
            appColorBtn.Location = new Point(80, y);
            appColorBtn.FlatStyle = FlatStyle.Flat;
            appColorBtn.BackColor = Color.FromArgb(0, 150, 80);
            appColorBtn.ForeColor = Color.White;
            appColorBtn.Cursor = Cursors.Hand;
            appColorBtn.Click += ChooseAppBackground;
            this.Controls.Add(appColorBtn);

            y += 60;

            // ===== КНОПКИ ДЕЙСТВИЙ =====
            Button resetIconsBtn = new Button();
            resetIconsBtn.Text = "🔄 Вернуть иконки в стандартное положение";
            resetIconsBtn.Size = new Size(280, 40);
            resetIconsBtn.Location = new Point(10, y);
            resetIconsBtn.FlatStyle = FlatStyle.Flat;
            resetIconsBtn.BackColor = Color.FromArgb(255, 180, 0);
            resetIconsBtn.ForeColor = Color.Black;
            resetIconsBtn.Cursor = Cursors.Hand;
            resetIconsBtn.Click += ResetIcons;
            this.Controls.Add(resetIconsBtn);
            y += 50;

            Button resetAllBtn = new Button();
            resetAllBtn.Text = "⚠️ Сбросить все настройки";
            resetAllBtn.Size = new Size(280, 40);
            resetAllBtn.Location = new Point(10, y);
            resetAllBtn.FlatStyle = FlatStyle.Flat;
            resetAllBtn.BackColor = Color.FromArgb(200, 50, 50);
            resetAllBtn.ForeColor = Color.White;
            resetAllBtn.Cursor = Cursors.Hand;
            resetAllBtn.Click += ResetAll;
            this.Controls.Add(resetAllBtn);
            y += 50;

            // ===== НАСТРОЙКИ АККАУНТА =====
            Label lblAccount = new Label();
            lblAccount.Text = "👤 Аккаунт:";
            lblAccount.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblAccount.Location = new Point(10, y);
            lblAccount.Size = new Size(180, 25);
            this.Controls.Add(lblAccount);
            y += 30;

            // Текущий пользователь
            Label lblUser = new Label();
            var user = AccountManager.GetCurrentUser();
            lblUser.Text = $"Пользователь: {user?.UserName ?? "Гость"}";
            lblUser.Location = new Point(20, y);
            lblUser.Size = new Size(200, 25);
            this.Controls.Add(lblUser);
            y += 30;

            // Требовать пароль
            CheckBox chkRequirePassword = new CheckBox();
            chkRequirePassword.Text = "🔒 Требовать пароль при входе";
            chkRequirePassword.Location = new Point(20, y);
            chkRequirePassword.Size = new Size(220, 25);
            chkRequirePassword.Checked = AccountManager.RequirePassword;
            chkRequirePassword.CheckedChanged += (s, e) =>
            {
                AccountManager.RequirePassword = chkRequirePassword.Checked;
            };
            this.Controls.Add(chkRequirePassword);
            y += 30;

            // Автовход
            CheckBox chkAutoLogin = new CheckBox();
            chkAutoLogin.Text = "🚀 Автоматический вход";
            chkAutoLogin.Location = new Point(20, y);
            chkAutoLogin.Size = new Size(180, 25);
            chkAutoLogin.Checked = AccountManager.AutoLogin;
            chkAutoLogin.CheckedChanged += (s, e) =>
            {
                AccountManager.AutoLogin = chkAutoLogin.Checked;
            };
            this.Controls.Add(chkAutoLogin);
            y += 40;

            // Выход из аккаунта
            Button btnLogout = new Button();
            btnLogout.Text = "🚪 Выйти из аккаунта";
            btnLogout.Size = new Size(160, 35);
            btnLogout.Location = new Point(20, y);
            btnLogout.BackColor = Color.FromArgb(200, 80, 80);
            btnLogout.ForeColor = Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Click += (s, e) =>
            {
                DialogResult result = MessageBox.Show("Выйти из аккаунта?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    AccountManager.Logout();
                    Application.Restart();
                }
            };
            this.Controls.Add(btnLogout);
            y += 50;

            // Сменить пароль
            Button btnChangePassword = new Button();
            btnChangePassword.Text = "🔑 Сменить пароль";
            btnChangePassword.Size = new Size(160, 35);
            btnChangePassword.Location = new Point(20, y);
            btnChangePassword.BackColor = Color.FromArgb(255, 180, 0);
            btnChangePassword.ForeColor = Color.Black;
            btnChangePassword.FlatStyle = FlatStyle.Flat;
            btnChangePassword.Click += ChangePassword;
            this.Controls.Add(btnChangePassword);
            y += 50;

            // ===== СТАТУС =====
            lblStatus = new Label();
            lblStatus.Text = "Готово";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Location = new Point(10, y);
            lblStatus.Size = new Size(400, 25);
            lblStatus.ForeColor = Color.DarkGreen;
            this.Controls.Add(lblStatus);

            colorDialog = new ColorDialog();
        }

        // ===== МЕТОДЫ =====

        private void SetWallpaper(Color color)
        {
            if (mainForm != null)
            {
                mainForm.BackColor = color;
                mainForm.SaveWallpaperColor(color);
                selectedWallpaperColor = color;

                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Panel panel && panel.BorderStyle == BorderStyle.Fixed3D && panel.Size == new Size(60, 40))
                    {
                        if (panel.Location.Y < 100)
                        {
                            panel.BackColor = color;
                            break;
                        }
                    }
                }

                lblStatus.Text = $"Фон изменён на {GetColorName(color)}";
                lblStatus.ForeColor = Color.DarkGreen;
            }
        }

        private string GetColorName(Color color)
        {
            for (int i = 0; i < wallpaperColors.Length; i++)
            {
                if (wallpaperColors[i].ToArgb() == color.ToArgb())
                {
                    return wallpaperNames[i];
                }
            }
            return color.Name;
        }

        private void ChooseCustomWallpaper(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                SetWallpaper(colorDialog.Color);
            }
        }

        private void ChooseAppBackground(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                selectedAppColor = colorDialog.Color;

                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Panel panel && panel.BorderStyle == BorderStyle.Fixed3D && panel.Size == new Size(60, 40))
                    {
                        if (panel.Location.Y > 100)
                        {
                            panel.BackColor = selectedAppColor;
                            break;
                        }
                    }
                }

                CustomWindow.DefaultBackground = selectedAppColor;
                lblStatus.Text = "Цвет фона приложений изменён";
                lblStatus.ForeColor = Color.DarkGreen;
            }
        }

        private void ResetIcons(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Вернуть все иконки в стандартное положение?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                mainForm.ResetIconsToDefault();
                lblStatus.Text = "Иконки восстановлены!";
                lblStatus.ForeColor = Color.DarkGreen;
            }
        }

        private void ResetAll(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "⚠️ ВНИМАНИЕ!\n\nСбросить все настройки?",
                "Сброс всех настроек",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                mainForm.ResetAllSettings();
                selectedWallpaperColor = Color.FromArgb(0, 128, 128);
                selectedAppColor = Color.White;

                lblStatus.Text = "Все настройки сброшены!";
                lblStatus.ForeColor = Color.DarkRed;
            }
        }

        private void ChangePassword(object sender, EventArgs e)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Смена пароля";
                dialog.Size = new Size(350, 220);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                int y = 20;

                Label lblOld = new Label();
                lblOld.Text = "Старый пароль:";
                lblOld.Location = new Point(20, y);
                lblOld.Size = new Size(100, 25);
                dialog.Controls.Add(lblOld);

                TextBox txtOld = new TextBox();
                txtOld.Location = new Point(130, y);
                txtOld.Size = new Size(180, 25);
                txtOld.UseSystemPasswordChar = true;
                dialog.Controls.Add(txtOld);
                y += 40;

                Label lblNew = new Label();
                lblNew.Text = "Новый пароль:";
                lblNew.Location = new Point(20, y);
                lblNew.Size = new Size(100, 25);
                dialog.Controls.Add(lblNew);

                TextBox txtNew = new TextBox();
                txtNew.Location = new Point(130, y);
                txtNew.Size = new Size(180, 25);
                txtNew.UseSystemPasswordChar = true;
                dialog.Controls.Add(txtNew);
                y += 40;

                Label lblConfirm = new Label();
                lblConfirm.Text = "Подтвердите:";
                lblConfirm.Location = new Point(20, y);
                lblConfirm.Size = new Size(100, 25);
                dialog.Controls.Add(lblConfirm);

                TextBox txtConfirm = new TextBox();
                txtConfirm.Location = new Point(130, y);
                txtConfirm.Size = new Size(180, 25);
                txtConfirm.UseSystemPasswordChar = true;
                dialog.Controls.Add(txtConfirm);
                y += 40;

                Button btnOk = new Button();
                btnOk.Text = "Изменить";
                btnOk.Location = new Point(150, y);
                btnOk.Size = new Size(100, 30);
                btnOk.BackColor = Color.FromArgb(0, 120, 215);
                btnOk.ForeColor = Color.White;
                btnOk.FlatStyle = FlatStyle.Flat;
                btnOk.Click += (s, ev) =>
                {
                    string oldPass = txtOld.Text;
                    string newPass = txtNew.Text;
                    string confirm = txtConfirm.Text;

                    var user = AccountManager.GetCurrentUser();
                    if (user == null)
                    {
                        MessageBox.Show("Пользователь не найден!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Хэшируем старый пароль для сравнения
                    string oldHash = AccountManager.HashPassword(oldPass);
                    if (oldHash != user.PasswordHash)
                    {
                        MessageBox.Show("Неверный старый пароль!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (newPass != confirm)
                    {
                        MessageBox.Show("Пароли не совпадают!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (newPass.Length < 3)
                    {
                        MessageBox.Show("Пароль должен содержать минимум 3 символа!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var accounts = AccountManager.GetAllUsers();
                    var userToUpdate = accounts.Find(a => a.Login == user.Login);
                    if (userToUpdate != null)
                    {
                        userToUpdate.PasswordHash = AccountManager.HashPassword(newPass);
                        // Сохраняем изменения
                        System.IO.File.WriteAllText(
                            System.IO.Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "MyOS95", "Accounts", "accounts.json"),
                            System.Text.Json.JsonSerializer.Serialize(accounts,
                                new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
                        );
                        MessageBox.Show("Пароль успешно изменён!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dialog.Close();
                    }
                };
                dialog.Controls.Add(btnOk);

                Button btnCancel = new Button();
                btnCancel.Text = "Отмена";
                btnCancel.Location = new Point(260, y);
                btnCancel.Size = new Size(75, 30);
                btnCancel.FlatStyle = FlatStyle.Flat;
                btnCancel.Click += (s, ev) => dialog.Close();
                dialog.Controls.Add(btnCancel);

                dialog.ShowDialog();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (wallpaperPanel != null)
            {
                wallpaperPanel.Width = this.ClientSize.Width - 30;
            }
        }
    }
}