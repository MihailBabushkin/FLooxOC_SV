using FlooxOC;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class RegisterDialog : Form
    {
        private TextBox txtLogin, txtPassword, txtConfirm, txtUserName;
        private Button btnRegister, btnCancel;
        private Label lblStatus;
        private bool isRegistered = false;
        public bool IsRegistered => isRegistered;
        public string RegisteredLogin { get; private set; } = "";

        public RegisterDialog()
        {
            this.Text = "📝 Регистрация";
            this.Size = new Size(560, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);

            InitializeComponents();
            ColorHelper.ApplyContrastToControls(this);

            // Фокус на поле логина при открытии
            this.Shown += (s, e) => txtLogin.Focus();
        }

        private void InitializeComponents()
        {
            int y = 25;

            // ===== ЗАГОЛОВОК =====
            Label title = new Label();
            title.Text = "📝 Создание аккаунта";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.Location = new Point(30, y);
            title.Size = new Size(250, 35);
            this.Controls.Add(title);
            y += 50;

            // ===== ЛОГИН =====
            Label lblLogin = new Label();
            lblLogin.Text = "Логин:";
            lblLogin.Font = new Font("Segoe UI", 10);
            lblLogin.Location = new Point(30, y);
            lblLogin.Size = new Size(110, 30);
            this.Controls.Add(lblLogin);

            txtLogin = new TextBox();
            txtLogin.Location = new Point(150, y);
            txtLogin.Size = new Size(220, 30);
            txtLogin.Font = new Font("Segoe UI", 11);
            txtLogin.TextChanged += (s, e) => { if (lblStatus.Text.Contains("Ошибка")) lblStatus.Text = ""; };
            this.Controls.Add(txtLogin);
            y += 45;

            // ===== ИМЯ =====
            Label lblName = new Label();
            lblName.Text = "Имя:";
            lblName.Font = new Font("Segoe UI", 10);
            lblName.Location = new Point(30, y);
            lblName.Size = new Size(110, 30);
            this.Controls.Add(lblName);

            txtUserName = new TextBox();
            txtUserName.Location = new Point(150, y);
            txtUserName.Size = new Size(220, 30);
            txtUserName.Font = new Font("Segoe UI", 11);
            txtUserName.Text = "Пользователь";
            txtUserName.TextChanged += (s, e) => { if (lblStatus.Text.Contains("Ошибка")) lblStatus.Text = ""; };
            this.Controls.Add(txtUserName);
            y += 45;

            // ===== ПАРОЛЬ =====
            Label lblPassword = new Label();
            lblPassword.Text = "Пароль:";
            lblPassword.Font = new Font("Segoe UI", 10);
            lblPassword.Location = new Point(30, y);
            lblPassword.Size = new Size(110, 30);
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(150, y);
            txtPassword.Size = new Size(220, 30);
            txtPassword.Font = new Font("Segoe UI", 11);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.TextChanged += (s, e) => { if (lblStatus.Text.Contains("Ошибка")) lblStatus.Text = ""; };
            this.Controls.Add(txtPassword);
            y += 45;

            // ===== ПОДТВЕРЖДЕНИЕ ПАРОЛЯ =====
            Label lblConfirm = new Label();
            lblConfirm.Text = "Подтвердите:";
            lblConfirm.Font = new Font("Segoe UI", 10);
            lblConfirm.Location = new Point(30, y);
            lblConfirm.Size = new Size(110, 30);
            this.Controls.Add(lblConfirm);

            txtConfirm = new TextBox();
            txtConfirm.Location = new Point(150, y);
            txtConfirm.Size = new Size(220, 30);
            txtConfirm.Font = new Font("Segoe UI", 11);
            txtConfirm.UseSystemPasswordChar = true;
            txtConfirm.TextChanged += (s, e) => { if (lblStatus.Text.Contains("Ошибка")) lblStatus.Text = ""; };
            this.Controls.Add(txtConfirm);
            y += 45;

            // ===== СТАТУС =====
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Location = new Point(30, y);
            lblStatus.Size = new Size(340, 30);
            lblStatus.Font = new Font("Segoe UI", 10);
            this.Controls.Add(lblStatus);
            y += 45;

            // ===== КНОПКИ =====
            btnRegister = new Button();
            btnRegister.Text = "✅ Зарегистрироваться";
            btnRegister.Size = new Size(180, 45);
            btnRegister.Location = new Point(30, y);
            btnRegister.BackColor = Color.FromArgb(0, 150, 80);
            btnRegister.ForeColor = ColorHelper.GetContrastTextColor(btnRegister.BackColor);
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 11);
            btnRegister.Cursor = Cursors.Hand;
            btnRegister.Click += (s, e) =>
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Text.Trim();
                string confirm = txtConfirm.Text.Trim();
                string userName = txtUserName.Text.Trim();

                // Проверка логина
                if (string.IsNullOrEmpty(login) || login.Length < 3)
                {
                    lblStatus.Text = "❌ Логин должен содержать минимум 3 символа!";
                    lblStatus.ForeColor = Color.DarkRed;
                    txtLogin.Focus();
                    return;
                }

                // Проверка пароля (если требуется)
                if (AccountManager.RequirePassword)
                {
                    if (string.IsNullOrEmpty(password) || password.Length < 3)
                    {
                        lblStatus.Text = "❌ Пароль должен содержать минимум 3 символа!";
                        lblStatus.ForeColor = Color.DarkRed;
                        txtPassword.Focus();
                        return;
                    }

                    if (password != confirm)
                    {
                        lblStatus.Text = "❌ Пароли не совпадают!";
                        lblStatus.ForeColor = Color.DarkRed;
                        txtConfirm.Clear();
                        txtConfirm.Focus();
                        return;
                    }
                }

                // Проверка имени
                if (string.IsNullOrEmpty(userName))
                {
                    userName = login;
                }

                // Регистрация
                if (AccountManager.RegisterUser(login, password, userName))
                {
                    isRegistered = true;
                    RegisteredLogin = login;
                    lblStatus.Text = "✅ Регистрация успешна!";
                    lblStatus.ForeColor = Color.DarkGreen;
                    this.DialogResult = DialogResult.OK;

                    // Небольшая задержка перед закрытием
                    System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
                    {
                        this.BeginInvoke((Action)this.Close);
                    });
                }
                else
                {
                    // Ошибка уже показана в AccountManager
                    lblStatus.Text = "❌ Ошибка регистрации!";
                    lblStatus.ForeColor = Color.DarkRed;
                }
            };
            this.Controls.Add(btnRegister);

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Size = new Size(120, 45);
            btnCancel.Location = new Point(220, y);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 11);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            // Нажатие Enter для быстрой регистрации
            txtConfirm.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                    btnRegister.PerformClick();
            };

            ColorHelper.ApplyContrastToControls(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    txtLogin?.Dispose();
                    txtPassword?.Dispose();
                    txtConfirm?.Dispose();
                    txtUserName?.Dispose();
                    btnRegister?.Dispose();
                    btnCancel?.Dispose();
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}