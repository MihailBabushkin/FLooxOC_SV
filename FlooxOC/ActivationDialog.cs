using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlooxOC
{
    public class ActivationDialog : Form
    {
        private TextBox txtCode;
        private Button btnActivate;
        private Button btnSkip;
        private Label lblStatus;
        private Label lblInfo;
        private bool isActivated = false;

        public bool IsActivated => isActivated;

        public ActivationDialog()
        {
            this.Text = "Активация MyOS 95";
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            int y = 20;

            Label title = new Label();
            title.Text = "🔑 Активация системы";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.Location = new Point(20, y);
            title.Size = new Size(400, 30);
            this.Controls.Add(title);
            y += 45;

            lblInfo = new Label();
            lblInfo.Text = "Введите код активации из Google Sheets.\n" +
                          "Код будет проверен и помечен как использованный.\n" +
                          "Для теста: DEMO-2024, TEST-1234, FREE-2024";
            lblInfo.Font = new Font("Segoe UI", 9);
            lblInfo.Location = new Point(20, y);
            lblInfo.Size = new Size(400, 45);
            this.Controls.Add(lblInfo);
            y += 55;

            Label lblCode = new Label();
            lblCode.Text = "Код активации:";
            lblCode.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblCode.Location = new Point(20, y);
            lblCode.Size = new Size(120, 25);
            this.Controls.Add(lblCode);

            txtCode = new TextBox();
            txtCode.Location = new Point(145, y);
            txtCode.Size = new Size(250, 25);
            txtCode.Font = new Font("Segoe UI", 10);
            txtCode.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                    ActivateAsync();
            };
            this.Controls.Add(txtCode);
            y += 40;

            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Location = new Point(20, y);
            lblStatus.Size = new Size(400, 25);
            lblStatus.ForeColor = Color.DarkRed;
            this.Controls.Add(lblStatus);
            y += 35;

            btnActivate = new Button();
            btnActivate.Text = "✅ Активировать";
            btnActivate.Size = new Size(130, 35);
            btnActivate.Location = new Point(20, y);
            btnActivate.BackColor = Color.FromArgb(0, 120, 215);
            btnActivate.ForeColor = Color.White;
            btnActivate.FlatStyle = FlatStyle.Flat;
            btnActivate.Cursor = Cursors.Hand;
            btnActivate.Click += async (s, e) => await ActivateAsync();
            this.Controls.Add(btnActivate);

            btnSkip = new Button();
            btnSkip.Text = "⏭ Пропустить (демо)";
            btnSkip.Size = new Size(130, 35);
            btnSkip.Location = new Point(160, y);
            btnSkip.FlatStyle = FlatStyle.Flat;
            btnSkip.Cursor = Cursors.Hand;
            btnSkip.Click += (s, e) =>
            {
                isActivated = true;
                this.Close();
            };
            this.Controls.Add(btnSkip);
        }

        private async Task ActivateAsync()
        {
            string code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                lblStatus.Text = "Введите код активации!";
                lblStatus.ForeColor = Color.DarkRed;
                return;
            }

            btnActivate.Enabled = false;
            btnActivate.Text = "⏳ Проверка...";
            lblStatus.Text = "⏳ Проверка кода в Google Sheets...";
            lblStatus.ForeColor = Color.DarkBlue;

            try
            {
                bool isValid = await GoogleSheetsManager.ValidateCode(code);

                if (isValid)
                {
                    isActivated = true;
                    lblStatus.Text = "✅ Активация успешна!";
                    lblStatus.ForeColor = Color.DarkGreen;
                    await Task.Delay(500);
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "❌ Неверный или уже использованный код!";
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
                btnActivate.Enabled = true;
                btnActivate.Text = "✅ Активировать";
            }
        }
    }
}