using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace FlooxOC
{
    public class ModernBrowserApp : Panel
    {
        private WebView2 webView;
        private TextBox urlBox;
        private Button goButton, backButton, forwardButton, refreshButton, homeButton;
        private Panel toolbar;
        private Label statusLabel;
        private bool isInitialized = false;
        private string pendingUrl = "";

        public event EventHandler UrlChanged;

        public ModernBrowserApp()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            InitializeToolbar();
            InitializeStatusBar();
            InitializeBrowser();
        }

        public void NavigateTo(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            url = url.Trim();

            // Проверяем, является ли URL IP-адресом или localhost
            bool isIpAddress = Regex.IsMatch(url, @"^(\d+\.\d+\.\d+\.\d+)(:\d+)?$");
            bool isLocalhost = url.StartsWith("localhost") || url.StartsWith("127.0.0.1");

            // Если это IP-адрес или localhost - добавляем http://
            if (isIpAddress || isLocalhost)
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
            }
            else if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                // Если это домен (содержит точку) - добавляем https://
                if (url.Contains("."))
                {
                    url = "https://" + url;
                }
                else
                {
                    // Поисковый запрос
                    url = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
                }
            }

            urlBox.Text = url;
            pendingUrl = url;

            // Если браузер уже инициализирован - сразу переходим
            if (isInitialized && webView != null && webView.CoreWebView2 != null)
            {
                Navigate();
            }
        }

        private void InitializeToolbar()
        {
            toolbar = new Panel
            {
                Height = 45,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(192, 192, 192),
                Padding = new Padding(5)
            };

            backButton = new Button
            {
                Text = "◄",
                Size = new Size(35, 28),
                Location = new Point(5, 8),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            backButton.Click += (s, e) =>
            {
                if (webView != null && webView.CoreWebView2 != null && webView.CanGoBack)
                    webView.GoBack();
            };
            toolbar.Controls.Add(backButton);

            forwardButton = new Button
            {
                Text = "►",
                Size = new Size(35, 28),
                Location = new Point(45, 8),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            forwardButton.Click += (s, e) =>
            {
                if (webView != null && webView.CoreWebView2 != null && webView.CanGoForward)
                    webView.GoForward();
            };
            toolbar.Controls.Add(forwardButton);

            refreshButton = new Button
            {
                Text = "⟳",
                Size = new Size(35, 28),
                Location = new Point(85, 8),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            refreshButton.Click += (s, e) =>
            {
                if (webView != null && webView.CoreWebView2 != null)
                    webView.Reload();
            };
            toolbar.Controls.Add(refreshButton);

            homeButton = new Button
            {
                Text = "🏠",
                Size = new Size(35, 28),
                Location = new Point(125, 8),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            homeButton.Click += (s, e) => NavigateTo("https://www.google.com");
            toolbar.Controls.Add(homeButton);

            urlBox = new TextBox
            {
                Location = new Point(165, 9),
                Size = new Size(370, 26),
                Font = new Font("Segoe UI", 10),
                Text = "https://www.google.com"
            };
            urlBox.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                    NavigateTo(urlBox.Text);
            };
            toolbar.Controls.Add(urlBox);

            goButton = new Button
            {
                Text = "Перейти",
                Size = new Size(70, 26),
                Location = new Point(540, 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            goButton.Click += (s, e) => NavigateTo(urlBox.Text);
            toolbar.Controls.Add(goButton);

            this.Controls.Add(toolbar);
        }

        private void InitializeStatusBar()
        {
            statusLabel = new Label
            {
                Text = "Готово",
                Dock = DockStyle.Bottom,
                Height = 22,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 8),
                Padding = new Padding(5, 2, 0, 0)
            };
            this.Controls.Add(statusLabel);
        }

        private async void InitializeBrowser()
        {
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            try
            {
                // Создаём папку для данных браузера
                string userDataFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MyOS95", "WebView2Data");
                System.IO.Directory.CreateDirectory(userDataFolder);

                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                    userDataFolder: userDataFolder,
                    options: new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions
                    {
                        // Разрешаем HTTP и IP адреса
                        AdditionalBrowserArguments = "--ignore-certificate-errors --allow-insecure-localhost"
                    });

                await webView.EnsureCoreWebView2Async(env);
                isInitialized = true;

                webView.CoreWebView2.NavigationStarting += (s, e) =>
                {
                    statusLabel.Text = $"Загрузка: {e.Uri}...";
                };

                webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    statusLabel.Text = "Готово";
                    if (webView.CoreWebView2 != null && !string.IsNullOrEmpty(webView.CoreWebView2.Source))
                    {
                        urlBox.Text = webView.CoreWebView2.Source;
                        backButton.Enabled = webView.CanGoBack;
                        forwardButton.Enabled = webView.CanGoForward;
                        UrlChanged?.Invoke(this, EventArgs.Empty);
                    }
                };

                // Загружаем стартовую страницу
                if (!string.IsNullOrEmpty(pendingUrl))
                {
                    webView.CoreWebView2.Navigate(pendingUrl);
                }
                else
                {
                    webView.CoreWebView2.Navigate("https://www.google.com");
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Ошибка: " + ex.Message;
                MessageBox.Show($"Ошибка инициализации браузера: {ex.Message}\n\n" +
                    "Убедитесь, что установлен Microsoft Edge WebView2.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.Controls.Add(webView);
        }

        private void Navigate()
        {
            if (!isInitialized || webView == null || webView.CoreWebView2 == null)
                return;

            string url = urlBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
                return;

            // Проверяем, является ли URL IP-адресом или localhost
            bool isIpAddress = Regex.IsMatch(url, @"^(\d+\.\d+\.\d+\.\d+)(:\d+)?$");
            bool isLocalhost = url.StartsWith("localhost") || url.StartsWith("127.0.0.1");

            if (isIpAddress || isLocalhost)
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
            }
            else if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                if (url.Contains("."))
                {
                    url = "https://" + url;
                }
                else
                {
                    url = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
                }
            }

            try
            {
                webView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка навигации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для обновления закладок из Form1
        public void OpenUrl(string url)
        {
            NavigateTo(url);
        }
    }
}