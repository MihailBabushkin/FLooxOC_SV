using FlooxOC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FlooxOC
{
    public class SettingsApp : Panel
    {
        // ===== ПОЛЯ =====
        private Form1 mainForm;
        private Color selectedWallpaperColor;
        private Color selectedAppColor;
        private ColorDialog colorDialog;

        // ===== КОМПОНЕНТЫ =====
        private Label labelStatus;
        private Label lblUserInfo;
        private TextBox txtNewName;
        private TextBox txtNewPassword;

        // ===== ЦВЕТА =====
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
            selectedAppColor = CustomWindow.DefaultBackground;
            colorDialog = new ColorDialog();

            InitializeComponents();
            ColorHelper.ApplyContrastToControls(this);
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

            // ===== РАЗДЕЛ 1: ПЕРСОНАЛИЗАЦИЯ =====
            Label sectionPersonalization = new Label();
            sectionPersonalization.Text = "👤 Персонализация";
            sectionPersonalization.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            sectionPersonalization.Location = new Point(10, y);
            sectionPersonalization.Size = new Size(400, 30);
            this.Controls.Add(sectionPersonalization);
            y += 40;

            // Информация о пользователе
            var user = AccountManager.GetCurrentUser();
            lblUserInfo = new Label();
            lblUserInfo.Text = $"Имя: {user?.UserName ?? "Гость"} | Логин: {user?.Login ?? "Нет"}";
            lblUserInfo.Font = new Font("Segoe UI", 10);
            lblUserInfo.Location = new Point(20, y);
            lblUserInfo.Size = new Size(400, 25);
            this.Controls.Add(lblUserInfo);
            y += 35;

            // Кнопка выбора цвета фона
            Button btnWallpaperColor = CreateStyledButton("🎨 Цвет фона рабочего стола", new Point(20, y), new Size(220, 35));
            btnWallpaperColor.Click += (s, e) => ShowColorPickerDialog("фон рабочего стола", selectedWallpaperColor, SetWallpaperColor);
            this.Controls.Add(btnWallpaperColor);

            // Кнопка выбора цвета приложений
            Button btnAppColor = CreateStyledButton("🎨 Цвет фона приложений", new Point(250, y), new Size(220, 35));
            btnAppColor.Click += (s, e) => ShowColorPickerDialog("фон приложений", selectedAppColor, SetAppColor);
            this.Controls.Add(btnAppColor);
            y += 50;

            // ===== РАЗДЕЛИТЕЛЬ =====
            this.Controls.Add(CreateSeparator(new Point(10, y), new Size(560, 2)));
            y += 20;

            // ===== РАЗДЕЛ 2: ПОЛЬЗОВАТЕЛЬ =====
            Label sectionUser = new Label();
            sectionUser.Text = "🔐 Пользователь";
            sectionUser.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            sectionUser.Location = new Point(10, y);
            sectionUser.Size = new Size(400, 30);
            this.Controls.Add(sectionUser);
            y += 40;

            // Смена имени
            Label lblName = new Label();
            lblName.Text = "Новое имя:";
            lblName.Font = new Font("Segoe UI", 10);
            lblName.Location = new Point(20, y);
            lblName.Size = new Size(100, 25);
            this.Controls.Add(lblName);

            txtNewName = new TextBox();
            txtNewName.Location = new Point(125, y);
            txtNewName.Size = new Size(200, 25);
            txtNewName.Font = new Font("Segoe UI", 10);
            txtNewName.Text = user?.UserName ?? "";
            this.Controls.Add(txtNewName);

            Button btnChangeName = CreateStyledButton("✅ Сменить имя", new Point(335, y), new Size(120, 25));
            btnChangeName.Click += (s, e) => ChangeUserName(txtNewName.Text);
            this.Controls.Add(btnChangeName);
            y += 40;

            // Кнопка смены пароля
            Button btnChangePassword = CreateStyledButton("🔑 Сменить пароль", new Point(20, y), new Size(200, 35));
            btnChangePassword.BackColor = Color.FromArgb(255, 180, 0);
            btnChangePassword.ForeColor = ColorHelper.GetContrastTextColor(btnChangePassword.BackColor);
            btnChangePassword.Click += (s, e) => ShowChangePasswordDialog();
            this.Controls.Add(btnChangePassword);
            y += 50;

            // ===== РАЗДЕЛИТЕЛЬ =====
            this.Controls.Add(CreateSeparator(new Point(10, y), new Size(560, 2)));
            y += 20;

            // ===== РАЗДЕЛ 3: СИСТЕМА =====
            Label sectionSystem = new Label();
            sectionSystem.Text = "🖥️ Система";
            sectionSystem.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            sectionSystem.Location = new Point(10, y);
            sectionSystem.Size = new Size(400, 30);
            this.Controls.Add(sectionSystem);
            y += 40;

            // Автоматическая расстановка иконок
            Button btnAutoArrange = CreateStyledButton("🔄 Автоматическая расстановка иконок", new Point(20, y), new Size(250, 40));
            btnAutoArrange.Click += (s, e) => AutoArrangeIcons();
            this.Controls.Add(btnAutoArrange);
            y += 55;

            // Цвета по умолчанию
            Button btnDefaultColors = CreateStyledButton("🎨 Цвета по умолчанию", new Point(20, y), new Size(250, 40));
            btnDefaultColors.BackColor = Color.FromArgb(0, 150, 80);
            btnDefaultColors.ForeColor = ColorHelper.GetContrastTextColor(btnDefaultColors.BackColor);
            btnDefaultColors.Click += (s, e) => ResetDefaultColors();
            this.Controls.Add(btnDefaultColors);
            y += 55;

            // Громкость
            Button btnVolume = CreateStyledButton("🔊 Громкость", new Point(20, y), new Size(250, 40));
            btnVolume.BackColor = Color.FromArgb(200, 180, 0);
            btnVolume.ForeColor = ColorHelper.GetContrastTextColor(btnVolume.BackColor);
            btnVolume.Click += (s, e) => ShowVolumeControl();
            this.Controls.Add(btnVolume);
            y += 55;

            // ===== НОВАЯ КНОПКА: ВХОД БЕЗ ПАРОЛЯ =====
            Button btnNoPasswordLogin = CreateStyledButton("🚪 Вход без пароля", new Point(20, y), new Size(250, 40));
            btnNoPasswordLogin.BackColor = Color.FromArgb(0, 120, 215);
            btnNoPasswordLogin.ForeColor = ColorHelper.GetContrastTextColor(btnNoPasswordLogin.BackColor);
            btnNoPasswordLogin.Click += (s, e) => ToggleNoPasswordLogin();
            this.Controls.Add(btnNoPasswordLogin);
            y += 55;

            // Очистка устройства
            Button btnClearDevice = CreateStyledButton("🗑️ Очистка устройства", new Point(20, y), new Size(250, 40));
            btnClearDevice.BackColor = Color.FromArgb(200, 50, 50);
            btnClearDevice.ForeColor = ColorHelper.GetContrastTextColor(btnClearDevice.BackColor);
            btnClearDevice.Click += (s, e) => ClearDevice();
            this.Controls.Add(btnClearDevice);
            y += 60;

            // ===== СТАТУС =====
            labelStatus = new Label();
            labelStatus.Text = "Готово";
            labelStatus.Font = new Font("Segoe UI", 9);
            labelStatus.Location = new Point(10, y);
            labelStatus.Size = new Size(400, 25);
            labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
            this.Controls.Add(labelStatus);

            // Применяем контрастные цвета
            ColorHelper.ApplyContrastToControls(this);
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====
        private Button CreateStyledButton(string text, Point location, Size size)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = ColorHelper.GetContrastTextColor(Color.FromArgb(0, 120, 215)),
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
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

        // ===== ДИАЛОГ ВЫБОРА ЦВЕТА (32 цвета плитками) =====
        private void ShowColorPickerDialog(string title, Color currentColor, Action<Color> onColorSelected)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = $"Выбор цвета для {title}";
                dialog.Size = new Size(520, 420);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                FlowLayoutPanel panel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10),
                    AutoScroll = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    BackColor = Color.FromArgb(220, 220, 220)
                };

                for (int i = 0; i < wallpaperColors.Length; i++)
                {
                    Button colorBtn = new Button();
                    colorBtn.Size = new Size(55, 50);
                    colorBtn.BackColor = wallpaperColors[i];
                    colorBtn.FlatStyle = FlatStyle.Flat;
                    colorBtn.FlatAppearance.BorderSize = 1;
                    colorBtn.FlatAppearance.BorderColor = Color.Black;
                    colorBtn.Cursor = Cursors.Hand;
                    colorBtn.Tag = wallpaperColors[i];

                    ToolTip toolTip = new ToolTip();
                    toolTip.SetToolTip(colorBtn, wallpaperNames[i]);

                    int index = i;
                    colorBtn.Click += (s, e) =>
                    {
                        onColorSelected(wallpaperColors[index]);
                        dialog.Close();
                    };

                    if (wallpaperColors[i].ToArgb() == currentColor.ToArgb())
                    {
                        colorBtn.FlatAppearance.BorderSize = 3;
                        colorBtn.FlatAppearance.BorderColor = Color.White;
                    }

                    panel.Controls.Add(colorBtn);
                }

                dialog.Controls.Add(panel);
                ColorHelper.ApplyContrastToControls(dialog);
                dialog.ShowDialog();
            }
        }

        // ===== УСТАНОВКА ЦВЕТА ФОНА =====
        private void SetWallpaperColor(Color color)
        {
            if (mainForm != null)
            {
                mainForm.BackColor = color;
                mainForm.SaveWallpaperColor(color);
                selectedWallpaperColor = color;

                ColorHelper.ApplyContrastToControls(mainForm);
                mainForm.Refresh();

                labelStatus.Text = $"Фон изменён на {GetColorName(color)}";
                labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
                ColorHelper.ApplyContrastToControls(this);
                this.Refresh();
            }
        }

        // ===== УСТАНОВКА ЦВЕТА ПРИЛОЖЕНИЙ =====
        private void SetAppColor(Color color)
        {
            selectedAppColor = color;
            CustomWindow.DefaultBackground = color;

            foreach (Form form in Application.OpenForms)
            {
                List<CustomWindow> windowsToUpdate = new List<CustomWindow>();
                foreach (Control ctrl in form.Controls)
                {
                    if (ctrl is CustomWindow window)
                    {
                        windowsToUpdate.Add(window);
                    }
                }

                foreach (var window in windowsToUpdate)
                {
                    window.BackColor = color;

                    if (window.ContentControl != null)
                    {
                        window.ContentControl.BackColor = color;
                        ColorHelper.ApplyContrastToControls(window.ContentControl);
                        window.ContentControl.Refresh();
                    }

                    foreach (Control ctrl in window.Controls)
                    {
                        if (ctrl is Panel panel && panel.Dock == DockStyle.Fill)
                        {
                            panel.BackColor = color;
                            ColorHelper.ApplyContrastToControls(panel);
                            panel.Refresh();
                        }
                    }

                    foreach (Control ctrl in window.Controls)
                    {
                        if (ctrl is Panel titleBar)
                        {
                            foreach (Control titleCtrl in titleBar.Controls)
                            {
                                if (titleCtrl is Label titleLabel)
                                {
                                    titleLabel.ForeColor = ColorHelper.GetContrastTextColor(color);
                                    titleLabel.Refresh();
                                }
                            }
                        }
                    }

                    window.Refresh();
                }
            }

            ColorHelper.ApplyContrastToControls(this);
            this.Refresh();

            labelStatus.Text = $"Цвет фона приложений изменён";
            labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
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

        // ===== СБРОС ЦВЕТОВ ПО УМОЛЧАНИЮ =====
        private void ResetDefaultColors()
        {
            DialogResult result = MessageBox.Show(
                "Сбросить цвета к стандартным?\n\n" +
                "Фон рабочего стола: Teal (0, 128, 128)\n" +
                "Фон приложений: Белый",
                "Сброс цветов",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Color defaultWallpaper = Color.FromArgb(0, 128, 128);
                SetWallpaperColor(defaultWallpaper);

                Color defaultAppColor = Color.White;
                SetAppColor(defaultAppColor);

                labelStatus.Text = "✅ Цвета сброшены к стандартным!";
                labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
            }
        }

        // ===== УПРАВЛЕНИЕ ГРОМКОСТЬЮ =====
        private void ShowVolumeControl()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "🔊 Управление громкостью";
                dialog.Size = new Size(350, 200);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                int y = 20;

                Label lblVolume = new Label();
                lblVolume.Text = "Громкость системы:";
                lblVolume.Font = new Font("Segoe UI", 10);
                lblVolume.Location = new Point(20, y);
                lblVolume.Size = new Size(150, 25);
                dialog.Controls.Add(lblVolume);

                TrackBar trackBar = new TrackBar();
                trackBar.Location = new Point(20, y + 30);
                trackBar.Size = new Size(300, 45);
                trackBar.Minimum = 0;
                trackBar.Maximum = 100;
                trackBar.Value = GetSystemVolume();
                trackBar.TickFrequency = 10;
                trackBar.TickStyle = TickStyle.Both;
                dialog.Controls.Add(trackBar);

                Label lblValue = new Label();
                lblValue.Text = $"{trackBar.Value}%";
                lblValue.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                lblValue.Location = new Point(260, y);
                lblValue.Size = new Size(60, 30);
                lblValue.TextAlign = ContentAlignment.MiddleRight;
                dialog.Controls.Add(lblValue);

                trackBar.Scroll += (s, e) =>
                {
                    lblValue.Text = $"{trackBar.Value}%";
                    SetSystemVolume(trackBar.Value);
                };

                y += 90;

                Button btnOk = new Button();
                btnOk.Text = "✅ Применить";
                btnOk.Size = new Size(120, 35);
                btnOk.Location = new Point(70, y);
                btnOk.BackColor = Color.FromArgb(0, 120, 215);
                btnOk.ForeColor = ColorHelper.GetContrastTextColor(btnOk.BackColor);
                btnOk.FlatStyle = FlatStyle.Flat;
                btnOk.Cursor = Cursors.Hand;
                btnOk.Click += (s, e) => dialog.Close();
                dialog.Controls.Add(btnOk);

                Button btnCancel = new Button();
                btnCancel.Text = "Отмена";
                btnCancel.Size = new Size(120, 35);
                btnCancel.Location = new Point(200, y);
                btnCancel.FlatStyle = FlatStyle.Flat;
                btnCancel.Cursor = Cursors.Hand;
                btnCancel.Click += (s, e) => dialog.Close();
                dialog.Controls.Add(btnCancel);

                ColorHelper.ApplyContrastToControls(dialog);
                dialog.ShowDialog();
            }
        }

        private int GetSystemVolume()
        {
            try
            {
                return 50;
            }
            catch
            {
                return 50;
            }
        }

        private void SetSystemVolume(int volume)
        {
            try
            {
                labelStatus.Text = $"🔊 Громкость установлена: {volume}%";
                labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка установки громкости: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== ВХОД БЕЗ ПАРОЛЯ (НОВАЯ ФУНКЦИЯ) =====
        private void ToggleNoPasswordLogin()
        {
            // Текущее состояние
            bool currentState = AccountManager.RequirePassword;

            DialogResult result = MessageBox.Show(
                currentState
                    ? "🔓 Включить вход без пароля?\n\n" +
                      "При следующем запуске система не будет запрашивать пароль.\n" +
                      "⚠️ ВНИМАНИЕ: Это снижает безопасность системы!"
                    : "🔒 Отключить вход без пароля?\n\n" +
                      "При следующем запуске система будет запрашивать пароль.",
                "Вход без пароля",
                MessageBoxButtons.YesNo,
                currentState ? MessageBoxIcon.Question : MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Переключаем состояние
                bool newState = !currentState;
                AccountManager.RequirePassword = newState;

                // Если включаем вход без пароля, отключаем требование пароля
                if (!newState)
                {
                    // Вход без пароля включен
                    AccountManager.AutoLogin = true;
                    labelStatus.Text = "✅ Вход без пароля ВКЛЮЧЕН!";
                    labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);

                    MessageBox.Show(
                        "🔓 Вход без пароля включен!\n\n" +
                        "Теперь при запуске системы пароль запрашиваться не будет.\n" +
                        "⚠️ Для отключения этой функции зайдите в настройки снова.",
                        "Вход без пароля",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    // Вход без пароля выключен - требуется пароль
                    AccountManager.AutoLogin = false;
                    labelStatus.Text = "🔒 Вход без пароля ОТКЛЮЧЕН!";
                    labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);

                    MessageBox.Show(
                        "🔒 Вход без пароля отключен!\n\n" +
                        "Теперь при запуске системы будет запрашиваться пароль.",
                        "Вход без пароля",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        // ===== СМЕНА ИМЕНИ =====
        private void ChangeUserName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Имя не может быть пустым!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var user = AccountManager.GetCurrentUser();
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var accounts = AccountManager.GetAllUsers();
                var userToUpdate = accounts.Find(a => a.Login == user.Login);
                if (userToUpdate != null)
                {
                    userToUpdate.UserName = newName;
                    user.UserName = newName;

                    string accountsPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Floox OC. Home Version", "Accounts", "accounts.json");

                    string json = System.Text.Json.JsonSerializer.Serialize(accounts,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(accountsPath, json);

                    labelStatus.Text = $"✅ Имя изменено на '{newName}'";
                    labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);

                    RefreshUserInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== ДИАЛОГ СМЕНЫ ПАРОЛЯ =====
        private void ShowChangePasswordDialog()
        {
            var user = AccountManager.GetCurrentUser();
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (Form dialog = new Form())
            {
                dialog.Text = "🔑 Смена пароля";
                dialog.Size = new Size(420, 280);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                int dlgY = 20;

                Label lblOld = new Label();
                lblOld.Text = "Текущий пароль:";
                lblOld.Font = new Font("Segoe UI", 10);
                lblOld.Location = new Point(20, dlgY);
                lblOld.Size = new Size(130, 25);
                dialog.Controls.Add(lblOld);

                TextBox txtOld = new TextBox();
                txtOld.Location = new Point(160, dlgY);
                txtOld.Size = new Size(230, 25);
                txtOld.Font = new Font("Segoe UI", 10);
                txtOld.UseSystemPasswordChar = true;
                dialog.Controls.Add(txtOld);
                dlgY += 45;

                Label lblNew = new Label();
                lblNew.Text = "Новый пароль:";
                lblNew.Font = new Font("Segoe UI", 10);
                lblNew.Location = new Point(20, dlgY);
                lblNew.Size = new Size(130, 25);
                dialog.Controls.Add(lblNew);

                TextBox txtNew = new TextBox();
                txtNew.Location = new Point(160, dlgY);
                txtNew.Size = new Size(230, 25);
                txtNew.Font = new Font("Segoe UI", 10);
                txtNew.UseSystemPasswordChar = true;
                dialog.Controls.Add(txtNew);
                dlgY += 45;

                Label lblConfirm = new Label();
                lblConfirm.Text = "Подтвердите:";
                lblConfirm.Font = new Font("Segoe UI", 10);
                lblConfirm.Location = new Point(20, dlgY);
                lblConfirm.Size = new Size(130, 25);
                dialog.Controls.Add(lblConfirm);

                TextBox txtConfirm = new TextBox();
                txtConfirm.Location = new Point(160, dlgY);
                txtConfirm.Size = new Size(230, 25);
                txtConfirm.Font = new Font("Segoe UI", 10);
                txtConfirm.UseSystemPasswordChar = true;
                dialog.Controls.Add(txtConfirm);
                dlgY += 50;

                Button btnOk = new Button();
                btnOk.Text = "✅ Подтвердить";
                btnOk.Size = new Size(120, 35);
                btnOk.Location = new Point(100, dlgY);
                btnOk.BackColor = Color.FromArgb(0, 150, 80);
                btnOk.ForeColor = ColorHelper.GetContrastTextColor(btnOk.BackColor);
                btnOk.FlatStyle = FlatStyle.Flat;
                btnOk.Cursor = Cursors.Hand;
                btnOk.Click += (s, ev) =>
                {
                    string oldPass = txtOld.Text;
                    string newPass = txtNew.Text;
                    string confirm = txtConfirm.Text;

                    if (string.IsNullOrEmpty(oldPass))
                    {
                        MessageBox.Show("Введите текущий пароль!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string oldHash = AccountManager.HashPassword(oldPass);
                    if (oldHash != user.PasswordHash)
                    {
                        MessageBox.Show("❌ Неверный текущий пароль!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtOld.Clear();
                        txtOld.Focus();
                        return;
                    }

                    if (string.IsNullOrEmpty(newPass) || newPass.Length < 3)
                    {
                        MessageBox.Show("Пароль должен содержать минимум 3 символа!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (newPass != confirm)
                    {
                        MessageBox.Show("❌ Пароли не совпадают!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtConfirm.Clear();
                        txtConfirm.Focus();
                        return;
                    }

                    try
                    {
                        var accounts = AccountManager.GetAllUsers();
                        var userToUpdate = accounts.Find(a => a.Login == user.Login);
                        if (userToUpdate != null)
                        {
                            userToUpdate.PasswordHash = AccountManager.HashPassword(newPass);

                            string accountsPath = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "Floox OC. Home Version", "Accounts", "accounts.json");

                            string json = System.Text.Json.JsonSerializer.Serialize(accounts,
                                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(accountsPath, json);

                            MessageBox.Show("✅ Пароль успешно изменён!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            labelStatus.Text = "✅ Пароль изменён!";
                            labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);

                            dialog.DialogResult = DialogResult.OK;
                            dialog.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                dialog.Controls.Add(btnOk);

                Button btnCancel = new Button();
                btnCancel.Text = "Отмена";
                btnCancel.Size = new Size(100, 35);
                btnCancel.Location = new Point(230, dlgY);
                btnCancel.FlatStyle = FlatStyle.Flat;
                btnCancel.Cursor = Cursors.Hand;
                btnCancel.Click += (s, ev) => dialog.Close();
                dialog.Controls.Add(btnCancel);

                ColorHelper.ApplyContrastToControls(dialog);

                txtConfirm.KeyPress += (s, ev) =>
                {
                    if (ev.KeyChar == (char)Keys.Enter)
                        btnOk.PerformClick();
                };

                dialog.ShowDialog();
            }
        }

        // ===== ОБНОВЛЕНИЕ ИНФОРМАЦИИ О ПОЛЬЗОВАТЕЛЕ =====
        private void RefreshUserInfo()
        {
            var user = AccountManager.GetCurrentUser();
            if (user != null && lblUserInfo != null)
            {
                lblUserInfo.Text = $"Имя: {user.UserName} | Логин: {user.Login}";
            }
        }

        // ===== АВТОМАТИЧЕСКАЯ РАССТАНОВКА ИКОНОК =====
        private void AutoArrangeIcons()
        {
            DialogResult result = MessageBox.Show(
                "Автоматически расставить иконки на рабочем столе?\n\n" +
                "⚠️ Иконки будут выровнены по сетке, но НЕ УДАЛЕНЫ.",
                "Расстановка иконок",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                mainForm.ResetIconsToDefault();
                labelStatus.Text = "✅ Иконки расставлены!";
                labelStatus.ForeColor = ColorHelper.GetContrastTextColor(this.BackColor);
            }
        }

        // ===== ОЧИСТКА УСТРОЙСТВА =====
        private void ClearDevice()
        {
            using (Form passwordDialog = new Form())
            {
                passwordDialog.Text = "🔐 Подтверждение очистки";
                passwordDialog.Size = new Size(380, 200);
                passwordDialog.StartPosition = FormStartPosition.CenterParent;
                passwordDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                passwordDialog.MaximizeBox = false;
                passwordDialog.MinimizeBox = false;
                passwordDialog.BackColor = Color.FromArgb(192, 192, 192);

                Label lblWarning = new Label();
                lblWarning.Text = "⚠️ ВНИМАНИЕ! Это действие удалит ВСЕ данные!\n" +
                                  "Введите пароль для подтверждения:";
                lblWarning.Font = new Font("Segoe UI", 9);
                lblWarning.Location = new Point(20, 15);
                lblWarning.Size = new Size(340, 40);
                passwordDialog.Controls.Add(lblWarning);

                TextBox txtPassword = new TextBox();
                txtPassword.Location = new Point(20, 65);
                txtPassword.Size = new Size(250, 25);
                txtPassword.UseSystemPasswordChar = true;
                txtPassword.KeyPress += (s, e) =>
                {
                    if (e.KeyChar == (char)Keys.Enter)
                    {
                        Button okBtn = passwordDialog.Controls["btnOk"] as Button;
                        okBtn?.PerformClick();
                    }
                };
                passwordDialog.Controls.Add(txtPassword);

                Button btnOk = new Button();
                btnOk.Name = "btnOk";
                btnOk.Text = "✅ Подтвердить";
                btnOk.Size = new Size(100, 30);
                btnOk.Location = new Point(80, 110);
                btnOk.BackColor = Color.FromArgb(200, 50, 50);
                btnOk.ForeColor = ColorHelper.GetContrastTextColor(btnOk.BackColor);
                btnOk.FlatStyle = FlatStyle.Flat;
                btnOk.Cursor = Cursors.Hand;
                btnOk.Click += (s, e) =>
                {
                    string enteredPassword = txtPassword.Text;
                    var user = AccountManager.GetCurrentUser();

                    if (user == null)
                    {
                        MessageBox.Show("Пользователь не найден!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string hash = AccountManager.HashPassword(enteredPassword);
                    if (hash == user.PasswordHash || enteredPassword == user.Login)
                    {
                        passwordDialog.DialogResult = DialogResult.OK;
                        passwordDialog.Close();
                    }
                    else
                    {
                        MessageBox.Show("❌ Неверный пароль!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtPassword.Clear();
                        txtPassword.Focus();
                    }
                };
                passwordDialog.Controls.Add(btnOk);

                Button btnCancel = new Button();
                btnCancel.Text = "Отмена";
                btnCancel.Size = new Size(100, 30);
                btnCancel.Location = new Point(190, 110);
                btnCancel.FlatStyle = FlatStyle.Flat;
                btnCancel.Cursor = Cursors.Hand;
                btnCancel.Click += (s, e) => passwordDialog.Close();
                passwordDialog.Controls.Add(btnCancel);

                ColorHelper.ApplyContrastToControls(passwordDialog);

                if (passwordDialog.ShowDialog() == DialogResult.OK)
                {
                    PerformClearDevice();
                }
            }
        }

        private void PerformClearDevice()
        {
            DialogResult finalConfirm = MessageBox.Show(
                "⚠️ ПОСЛЕДНЕЕ ПРЕДУПРЕЖДЕНИЕ!\n\n" +
                "Будут удалены ВСЕ данные:\n" +
                "• Все аккаунты и пароли\n" +
                "• Добавленные приложения\n" +
                "• Закладки\n" +
                "• Макеты рабочего стола\n" +
                "• Настройки системы\n\n" +
                "После очистки приложение будет перезапущено.\n\n" +
                "Продолжить?",
                "Полная очистка",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (finalConfirm == DialogResult.Yes)
            {
                try
                {
                    string appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Floox OC. Home Version");

                    if (Directory.Exists(appDataPath))
                    {
                        Directory.Delete(appDataPath, true);

                        MessageBox.Show(
                            "✅ Устройство очищено!\n\n" +
                            "Приложение будет перезапущено.",
                            "Успех",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    Application.Restart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка очистки:\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }
    }
}