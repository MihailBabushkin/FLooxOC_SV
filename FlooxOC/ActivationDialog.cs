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
        private string activationCode = "";

        public bool IsActivated => isActivated;
        public string ActivationCode => activationCode;

        public ActivationDialog()
        {
            this.Text = "Активация Floox OC. Home Version";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);

            InitializeComponents();
            ColorHelper.ApplyContrastToControls(this);
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
            lblInfo.Text = "Введите код активации для получения полной версии.\n" +
                          "Код можно приобрести у разработчика.\n" +
                          "Или нажмите 'Пропустить' для использования ДЕМО-режима.";
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
            lblStatus.Size = new Size(400, 50);
            lblStatus.ForeColor = Color.DarkRed;
            this.Controls.Add(lblStatus);
            y += 60;

            btnActivate = new Button();
            btnActivate.Text = "✅ Активировать";
            btnActivate.Size = new Size(130, 35);
            btnActivate.Location = new Point(20, y);
            btnActivate.BackColor = Color.FromArgb(0, 120, 215);
            btnActivate.ForeColor = ColorHelper.GetContrastTextColor(btnActivate.BackColor);
            btnActivate.FlatStyle = FlatStyle.Flat;
            btnActivate.Cursor = Cursors.Hand;
            btnActivate.Click += async (s, e) => await ActivateAsync();
            this.Controls.Add(btnActivate);

            btnSkip = new Button();
            btnSkip.Text = "⏭ Пропустить (демо)";
            btnSkip.Size = new Size(130, 35);
            btnSkip.Location = new Point(160, y);
            btnSkip.FlatStyle = FlatStyle.Flat;
            btnSkip.ForeColor = ColorHelper.GetContrastTextColor(btnSkip.BackColor);
            btnSkip.Cursor = Cursors.Hand;
            btnSkip.Click += (s, e) =>
            {
                isActivated = true;
                this.Close();
            };
            this.Controls.Add(btnSkip);

            Button btnCheckConnection = new Button();
            btnCheckConnection.Text = "🔍 Проверить связь";
            btnCheckConnection.Size = new Size(130, 35);
            btnCheckConnection.Location = new Point(300, y);
            btnCheckConnection.FlatStyle = FlatStyle.Flat;
            btnCheckConnection.BackColor = Color.FromArgb(200, 180, 0);
            btnCheckConnection.ForeColor = ColorHelper.GetContrastTextColor(btnCheckConnection.BackColor);
            btnCheckConnection.Cursor = Cursors.Hand;
            btnCheckConnection.Click += async (s, e) => await CheckConnection();
            this.Controls.Add(btnCheckConnection);

            ColorHelper.ApplyContrastToControls(this);
        }

        private async Task ActivateAsync()
        {
            string code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                lblStatus.Text = "⚠️ Введите код активации!";
                lblStatus.ForeColor = Color.DarkRed;
                return;
            }

            btnActivate.Enabled = false;
            btnActivate.Text = "⏳ Проверка...";
            lblStatus.Text = "⏳ Проверка кода...";
            lblStatus.ForeColor = Color.DarkBlue;

            try
            {
                // Проверяем код на сервере
                var result = await MySQLManager.ValidateCode(code);

                if (result.Success)
                {
                    // Активируем код (удаляем из БД)
                    var activateResult = await MySQLManager.ActivateCode(code, Environment.UserName);

                    if (activateResult)
                    {
                        isActivated = true;
                        activationCode = code;
                        lblStatus.Text = "✅ Активация успешна!";
                        lblStatus.ForeColor = Color.DarkGreen;

                        AccountManager.SaveActivationCode(code);

                        await Task.Delay(500);
                        this.Close();
                    }
                    else
                    {
                        lblStatus.Text = "❌ Не удалось активировать код!";
                        lblStatus.ForeColor = Color.DarkRed;
                    }
                }
                else
                {
                    lblStatus.Text = result.Message;
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

        private async Task CheckConnection()
        {
            btnActivate.Enabled = false;
            lblStatus.Text = "⏳ Проверка связи с сервером...";
            lblStatus.ForeColor = Color.DarkBlue;

            try
            {
                bool connected = await MySQLManager.TestConnection();
                if (connected)
                {
                    var stats = await MySQLManager.GetStatistics();
                    lblStatus.Text = $"✅ Связь с сервером установлена!\n{stats}";
                    lblStatus.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblStatus.Text = "❌ Не удалось подключиться к серверу!";
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
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                txtCode?.Dispose();
                btnActivate?.Dispose();
                btnSkip?.Dispose();
                lblStatus?.Dispose();
                lblInfo?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}