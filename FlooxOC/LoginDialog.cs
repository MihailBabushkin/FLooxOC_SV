using FlooxOC;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class LoginDialog : Form
    {
        private TextBox txtLogin, txtPassword, txtUserName;
        private Button btnLogin, btnRegister;
        private CheckBox chkShowPassword;
        private Label lblStatus;
        private TabControl tabControl;
        private bool isLoggedIn = false;
        public bool IsLoggedIn => isLoggedIn;
        public string LoggedInUser { get; private set; } = "";

        public LoginDialog()
        {
            this.Text = "Вход в MyOS 95";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);

            InitializeComponents();

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
            tabControl = new TabControl();
            tabControl.Size = new Size(360, 270);
            tabControl.Location = new Point(15, 10);
            tabControl.Font = new Font("Segoe UI", 9);
            this.Controls.Add(tabControl);

            // ===== ВКЛАДКА ВХОДА =====
            TabPage loginPage = new TabPage("Вход");
            loginPage.BackColor = Color.FromArgb(192, 192, 192);
            tabControl.Controls.Add(loginPage);

            int y = 20;

            Label lblLogin = new Label();
            lblLogin.Text = "Логин:";
            lblLogin.Location = new Point(20, y);
            lblLogin.Size = new Size(80, 25);
            loginPage.Controls.Add(lblLogin);

            txtLogin = new TextBox();
            txtLogin.Location = new Point(110, y);
            txtLogin.Size = new Size(210, 25);
            txtLogin.Font = new Font("Segoe UI", 10);
            loginPage.Controls.Add(txtLogin);
            y += 40;

            Label lblPassword = new Label();
            lblPassword.Text = "Пароль:";
            lblPassword.Location = new Point(20, y);
            lblPassword.Size = new Size(80, 25);
            loginPage.Controls.Add(lblPassword);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(110, y);
            txtPassword.Size = new Size(210, 25);
            txtPassword.Font = new Font("Segoe UI", 10);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                    PerformLogin();
            };
            loginPage.Controls.Add(txtPassword);
            y += 35;

            chkShowPassword = new CheckBox();
            chkShowPassword.Text = "Показать пароль";
            chkShowPassword.Location = new Point(110, y);
            chkShowPassword.Size = new Size(120, 25);
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };
            loginPage.Controls.Add(chkShowPassword);
            y += 35;

            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Location = new Point(20, y);
            lblStatus.Size = new Size(300, 25);
            lblStatus.Font = new Font("Segoe UI", 9);
            loginPage.Controls.Add(lblStatus);
            y += 35;

            btnLogin = new Button();
            btnLogin.Text = "🚪 Войти";
            btnLogin.Size = new Size(100, 35);
            btnLogin.Location = new Point(110, y);
            btnLogin.BackColor = Color.FromArgb(0, 120, 215);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += (s, e) => PerformLogin();
            loginPage.Controls.Add(btnLogin);

            // ===== ВКЛАДКА РЕГИСТРАЦИИ =====
            TabPage registerPage = new TabPage("Регистрация");
            registerPage.BackColor = Color.FromArgb(192, 192, 192);
            tabControl.Controls.Add(registerPage);

            y = 20;

            Label lblRegLogin = new Label();
            lblRegLogin.Text = "Логин:";
            lblRegLogin.Location = new Point(20, y);
            lblRegLogin.Size = new Size(80, 25);
            registerPage.Controls.Add(lblRegLogin);

            TextBox txtRegLogin = new TextBox();
            txtRegLogin.Location = new Point(110, y);
            txtRegLogin.Size = new Size(210, 25);
            txtRegLogin.Font = new Font("Segoe UI", 10);
            registerPage.Controls.Add(txtRegLogin);
            y += 40;

            Label lblRegPassword = new Label();
            lblRegPassword.Text = "Пароль:";
            lblRegPassword.Location = new Point(20, y);
            lblRegPassword.Size = new Size(80, 25);
            registerPage.Controls.Add(lblRegPassword);

            TextBox txtRegPassword = new TextBox();
            txtRegPassword.Location = new Point(110, y);
            txtRegPassword.Size = new Size(210, 25);
            txtRegPassword.Font = new Font("Segoe UI", 10);
            txtRegPassword.UseSystemPasswordChar = true;
            registerPage.Controls.Add(txtRegPassword);
            y += 40;

            Label lblRegName = new Label();
            lblRegName.Text = "Имя:";
            lblRegName.Location = new Point(20, y);
            lblRegName.Size = new Size(80, 25);
            registerPage.Controls.Add(lblRegName);

            txtUserName = new TextBox();
            txtUserName.Location = new Point(110, y);
            txtUserName.Size = new Size(210, 25);
            txtUserName.Font = new Font("Segoe UI", 10);
            txtUserName.Text = "Пользователь";
            registerPage.Controls.Add(txtUserName);
            y += 35;

            Label lblRegStatus = new Label();
            lblRegStatus.Text = "";
            lblRegStatus.Location = new Point(20, y);
            lblRegStatus.Size = new Size(300, 25);
            lblRegStatus.Font = new Font("Segoe UI", 9);
            registerPage.Controls.Add(lblRegStatus);
            y += 35;

            btnRegister = new Button();
            btnRegister.Text = "📝 Зарегистрироваться";
            btnRegister.Size = new Size(160, 35);
            btnRegister.Location = new Point(110, y);
            btnRegister.BackColor = Color.FromArgb(0, 150, 80);
            btnRegister.ForeColor = Color.White;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Cursor = Cursors.Hand;
            btnRegister.Click += (s, e) =>
            {
                string login = txtRegLogin.Text.Trim();
                string password = txtRegPassword.Text.Trim();
                string userName = txtUserName.Text.Trim();

                if (AccountManager.RegisterUser(login, password, userName))
                {
                    isLoggedIn = true;
                    LoggedInUser = login;
                    lblRegStatus.Text = "✅ Регистрация успешна!";
                    lblRegStatus.ForeColor = Color.DarkGreen;
                    this.Close();
                }
                else
                {
                    lblRegStatus.Text = "❌ Ошибка регистрации!";
                    lblRegStatus.ForeColor = Color.DarkRed;
                }
            };
            registerPage.Controls.Add(btnRegister);
        }

        private void PerformLogin()
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login))
            {
                lblStatus.Text = "Введите логин!";
                lblStatus.ForeColor = Color.DarkRed;
                return;
            }

            if (AccountManager.RequirePassword && string.IsNullOrEmpty(password))
            {
                lblStatus.Text = "Введите пароль!";
                lblStatus.ForeColor = Color.DarkRed;
                return;
            }

            if (AccountManager.Login(login, password))
            {
                isLoggedIn = true;
                LoggedInUser = login;
                lblStatus.Text = "✅ Вход выполнен!";
                lblStatus.ForeColor = Color.DarkGreen;
                this.Close();
            }
            else
            {
                lblStatus.Text = "❌ Неверный логин или пароль!";
                lblStatus.ForeColor = Color.DarkRed;
            }
        }

        private void PerformAutoLogin()
        {
            string lastUser = AccountManager.GetLastUser();
            if (!string.IsNullOrEmpty(lastUser))
            {
                if (AccountManager.Login(lastUser, ""))
                {
                    isLoggedIn = true;
                    LoggedInUser = lastUser;
                    this.Close();
                }
            }
        }
    }
}