using FlooxOC;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class LoginDialog : Form
    {
        private TextBox txtLogin, txtPassword;
        private Button btnLogin;
        private CheckBox chkShowPassword;
        private Label lblStatus;
        private bool isLoggedIn = false;
        private bool isClosed = false;
        private bool isLoggingIn = false;
        private bool autoLoginAttempted = false;
        private int attemptCount = 0;
        public bool IsLoggedIn => isLoggedIn;
        public string LoggedInUser { get; private set; } = "";

        public LoginDialog()
        {
            this.Text = "Вход в Floox OC. Home Version";
            this.Size = new Size(440, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);

            InitializeComponents();

            // Применяем контрастные цвета
            ColorHelper.ApplyContrastToControls(this);

            this.Shown += LoginDialog_Shown;
        }

        private void LoginDialog_Shown(object sender, EventArgs e)
        {
            if (autoLoginAttempted) return;
            autoLoginAttempted = true;

            string lastUser = AccountManager.GetLastUser();
            if (!string.IsNullOrEmpty(lastUser))
            {
                txtLogin.Text = lastUser;

                if (AccountManager.AutoLogin && !AccountManager.RequirePassword)
                {
                    PerformAutoLogin();
                }
            }
        }

        private void InitializeComponents()
        {
            int y = 25;

            // ===== ЗАГОЛОВОК =====
            Label title = new Label();
            title.Text = "🔐 Вход в систему";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.Location = new Point(30, y);
            title.Size = new Size(250, 30);
            this.Controls.Add(title);
            y += 50;

            // ===== ЛОГИН =====
            Label lblLogin = new Label();
            lblLogin.Text = "Логин:";
            lblLogin.Font = new Font("Segoe UI", 10);
            lblLogin.Location = new Point(30, y);
            lblLogin.Size = new Size(100, 40);
            this.Controls.Add(lblLogin);

            txtLogin = new TextBox();
            txtLogin.Location = new Point(140, y);
            txtLogin.Size = new Size(230, 40);
            txtLogin.Font = new Font("Segoe UI", 11);
            this.Controls.Add(txtLogin);
            y += 45;

            // ===== ПАРОЛЬ =====
            Label lblPassword = new Label();
            lblPassword.Text = "Пароль:";
            lblPassword.Font = new Font("Segoe UI", 10);
            lblPassword.Location = new Point(30, y);
            lblPassword.Size = new Size(100, 40);
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(140, y);
            txtPassword.Size = new Size(230, 40);
            txtPassword.Font = new Font("Segoe UI", 11);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                    PerformLogin();
            };
            this.Controls.Add(txtPassword);
            y += 40;

            // ===== ПОКАЗАТЬ ПАРОЛЬ =====
            chkShowPassword = new CheckBox();
            chkShowPassword.Text = "👁 Показать пароль";
            chkShowPassword.Font = new Font("Segoe UI", 9);
            chkShowPassword.Location = new Point(140, y);
            chkShowPassword.Size = new Size(140, 35);
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };
            this.Controls.Add(chkShowPassword);
            y += 40;

            // ===== СТАТУС =====
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Location = new Point(30, y);
            lblStatus.Size = new Size(340, 40);
            lblStatus.Font = new Font("Segoe UI", 10);
            this.Controls.Add(lblStatus);
            y += 40;

            // ===== КНОПКИ =====
            btnLogin = new Button();
            btnLogin.Text = "🚪 Войти";
            btnLogin.Size = new Size(130, 55);
            btnLogin.Location = new Point(140, y);
            btnLogin.BackColor = Color.FromArgb(0, 120, 215);
            btnLogin.ForeColor = ColorHelper.GetContrastTextColor(btnLogin.BackColor);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 11);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += (s, e) => PerformLogin();
            this.Controls.Add(btnLogin);

            Button btnRegister = new Button();
            btnRegister.Text = "📝 Регистрация";
            btnRegister.Size = new Size(130, 55);
            btnRegister.Location = new Point(280, y);
            btnRegister.BackColor = Color.FromArgb(0, 150, 80);
            btnRegister.ForeColor = ColorHelper.GetContrastTextColor(btnRegister.BackColor);
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 11);
            btnRegister.Cursor = Cursors.Hand;
            btnRegister.Click += (s, e) =>
            {
                using (var regDialog = new RegisterDialog())
                {
                    regDialog.ShowDialog();
                    if (regDialog.IsRegistered)
                    {
                        txtLogin.Text = regDialog.RegisteredLogin;
                        PerformLogin();
                    }
                }
            };
            this.Controls.Add(btnRegister);

            // Применяем контрастные цвета ко всем контролам
            ColorHelper.ApplyContrastToControls(this);
        }

        private void PerformLogin()
        {
            if (isClosed || isLoggingIn) return;

            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login))
            {
                lblStatus.Text = "⚠️ Введите логин!";
                lblStatus.ForeColor = Color.DarkRed;
                return;
            }

            if (AccountManager.RequirePassword)
            {
                if (string.IsNullOrEmpty(password))
                {
                    lblStatus.Text = "⚠️ Введите пароль!";
                    lblStatus.ForeColor = Color.DarkRed;
                    return;
                }

                if (attemptCount >= AccountManager.MaxLoginAttempts)
                {
                    lblStatus.Text = $"⚠️ Превышено количество попыток ({AccountManager.MaxLoginAttempts})!";
                    lblStatus.ForeColor = Color.DarkRed;
                    btnLogin.Enabled = false;
                    return;
                }
            }

            isLoggingIn = true;
            btnLogin.Enabled = false;
            lblStatus.Text = "⏳ Проверка...";
            lblStatus.ForeColor = Color.DarkBlue;

            try
            {
                if (AccountManager.Login(login, password))
                {
                    isLoggedIn = true;
                    LoggedInUser = login;
                    lblStatus.Text = "✅ Вход выполнен!";
                    lblStatus.ForeColor = Color.DarkGreen;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    attemptCount++;
                    int remaining = AccountManager.MaxLoginAttempts - attemptCount;
                    if (remaining > 0)
                    {
                        lblStatus.Text = $"❌ Неверный логин или пароль! Осталось попыток: {remaining}";
                    }
                    else
                    {
                        lblStatus.Text = $"❌ Неверный логин или пароль! Попытки закончились.";
                        btnLogin.Enabled = false;
                    }
                    lblStatus.ForeColor = Color.DarkRed;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Ошибка: {ex.Message}";
                lblStatus.ForeColor = Color.DarkRed;
            }
            finally
            {
                isLoggingIn = false;
                btnLogin.Enabled = true;
            }
        }

        private void PerformAutoLogin()
        {
            if (isClosed) return;

            string lastUser = AccountManager.GetLastUser();
            if (!string.IsNullOrEmpty(lastUser))
            {
                try
                {
                    if (AccountManager.Login(lastUser, ""))
                    {
                        isLoggedIn = true;
                        LoggedInUser = lastUser;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch
                {
                    // Ошибка автовхода — ничего не делаем
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isClosed = true;
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (txtLogin != null) txtLogin.Dispose();
                    if (txtPassword != null) txtPassword.Dispose();
                    if (btnLogin != null) btnLogin.Dispose();
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}