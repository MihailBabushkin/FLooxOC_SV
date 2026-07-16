using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class BrowserApp : Panel
    {
        private TextBox urlBox;
        private WebBrowser webBrowser;
        private Button goButton, backButton, forwardButton, refreshButton;

        public BrowserApp()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            InitializeToolbar();
            InitializeBrowser();
        }

        private void InitializeToolbar()
        {
            Panel toolbar = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(192, 192, 192),
                Padding = new Padding(5)
            };

            // Кнопка Назад
            backButton = new Button
            {
                Text = "◄",
                Size = new Size(35, 28),
                Location = new Point(5, 5),
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (s, e) => webBrowser.GoBack();
            toolbar.Controls.Add(backButton);

            // Кнопка Вперёд
            forwardButton = new Button
            {
                Text = "►",
                Size = new Size(35, 28),
                Location = new Point(45, 5),
                FlatStyle = FlatStyle.Flat
            };
            forwardButton.Click += (s, e) => webBrowser.GoForward();
            toolbar.Controls.Add(forwardButton);

            // Кнопка Обновить
            refreshButton = new Button
            {
                Text = "⟳",
                Size = new Size(35, 28),
                Location = new Point(85, 5),
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.Click += (s, e) => webBrowser.Refresh();
            toolbar.Controls.Add(refreshButton);

            // Адресная строка
            urlBox = new TextBox
            {
                Location = new Point(125, 6),
                Size = new Size(400, 26),
                Font = new Font("Segoe UI", 10),
                Text = "https://www.google.com"
            };
            urlBox.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                    Navigate();
            };
            toolbar.Controls.Add(urlBox);

            // Кнопка Перейти
            goButton = new Button
            {
                Text = "Перейти",
                Size = new Size(70, 28),
                Location = new Point(530, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            goButton.Click += (s, e) => Navigate();
            toolbar.Controls.Add(goButton);

            this.Controls.Add(toolbar);
        }

        private void InitializeBrowser()
        {
            webBrowser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true
            };

            webBrowser.Navigated += (s, e) =>
            {
                urlBox.Text = webBrowser.Url.ToString();
                backButton.Enabled = webBrowser.CanGoBack;
                forwardButton.Enabled = webBrowser.CanGoForward;
            };

            // Загружаем стартовую страницу
            webBrowser.Navigate("https://www.google.com");

            this.Controls.Add(webBrowser);
        }

        private void Navigate()
        {
            string url = urlBox.Text.Trim();
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            try
            {
                webBrowser.Navigate(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}