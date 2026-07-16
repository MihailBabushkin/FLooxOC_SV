using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class CustomWindow : Panel
    {
        public static Color DefaultBackground = Color.White;
        private Panel titleBar;
        private Label titleLabel;
        private Button closeBtn, maxBtn, minBtn;
        private Panel contentPanel;
        private bool isDragging = false;
        private Point dragOffset;
        private bool isMaximized = false;
        private Rectangle normalBounds;
        private Point normalLocation;
        private const int RESIZE_MARGIN = 8;

        public event EventHandler Closed;
        public event EventHandler Minimized;

        public string Title
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        public Control ContentControl
        {
            get => contentPanel.Controls.Count > 0 ? contentPanel.Controls[0] : null;
            set
            {
                contentPanel.Controls.Clear();
                if (value != null)
                {
                    value.Dock = DockStyle.Fill;
                    value.Location = new Point(0, 0);
                    contentPanel.Controls.Add(value);
                }
            }
        }

        public CustomWindow(string title) : this(title, "")
        {
        }

        public CustomWindow(string title, string content)
        {
            this.Size = new Size(500, 400);
            this.MinimumSize = new Size(200, 150);
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.DoubleBuffered = true;

            InitializeTitleBar(title);
            InitializeContent(content);
            SetDefaultPosition();
            this.Resize += OnResize;
        }

        private void InitializeTitleBar(string title)
        {
            titleBar = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(0, 0, 128),
                Margin = new Padding(0)
            };

            titleLabel = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(5, 6),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            closeBtn = new Button
            {
                Text = "✕",
                Size = new Size(22, 22),
                Location = new Point(titleBar.Width - 27, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 1;
            closeBtn.Click += (s, e) => CloseWindow();

            maxBtn = new Button
            {
                Text = "□",
                Size = new Size(22, 22),
                Location = new Point(titleBar.Width - 52, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            maxBtn.FlatAppearance.BorderSize = 1;
            maxBtn.Click += (s, e) => ToggleMaximize();

            minBtn = new Button
            {
                Text = "─",
                Size = new Size(22, 22),
                Location = new Point(titleBar.Width - 77, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            minBtn.FlatAppearance.BorderSize = 1;
            minBtn.Click += (s, e) => MinimizeWindow();

            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.MouseMove += TitleBar_MouseMove;
            titleBar.MouseUp += TitleBar_MouseUp;
            titleBar.MouseDoubleClick += (s, e) => ToggleMaximize();

            titleBar.Controls.Add(titleLabel);
            titleBar.Controls.Add(closeBtn);
            titleBar.Controls.Add(maxBtn);
            titleBar.Controls.Add(minBtn);
            this.Controls.Add(titleBar);

            titleBar.Resize += (s, e) =>
            {
                closeBtn.Location = new Point(titleBar.Width - 27, 4);
                maxBtn.Location = new Point(titleBar.Width - 52, 4);
                minBtn.Location = new Point(titleBar.Width - 77, 4);
            };
        }

        private void InitializeContent(string text)
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(5),
                AutoScroll = true
            };

            if (!string.IsNullOrEmpty(text))
            {
                Label label = new Label
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10),
                    BackColor = Color.White
                };
                contentPanel.Controls.Add(label);
            }

            this.Controls.Add(contentPanel);

            // Важно: контент должен быть под шапкой
            contentPanel.BringToFront();
        }

        private void SetDefaultPosition()
        {
            Random rnd = new Random();
            this.Location = new Point(
                rnd.Next(50, 300),
                rnd.Next(50, 200)
            );
            this.normalLocation = this.Location;
            this.normalBounds = this.Bounds;
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (titleBar != null && closeBtn != null)
            {
                closeBtn.Location = new Point(titleBar.Width - 27, 4);
                maxBtn.Location = new Point(titleBar.Width - 52, 4);
                minBtn.Location = new Point(titleBar.Width - 77, 4);
            }

            // Обновляем контент
            if (contentPanel != null)
            {
                contentPanel.Size = new Size(
                    this.ClientSize.Width,
                    this.ClientSize.Height - titleBar.Height
                );
            }
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !isMaximized)
            {
                isDragging = true;
                dragOffset = new Point(e.X, e.Y);
                titleBar.Capture = true;
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && !isMaximized)
            {
                Point newLocation = this.PointToScreen(new Point(e.X, e.Y));
                newLocation.Offset(-dragOffset.X, -dragOffset.Y);
                this.Location = newLocation;
            }
        }

        private void TitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                titleBar.Capture = false;
            }
        }

        private void CloseWindow()
        {
            Closed?.Invoke(this, EventArgs.Empty);
            if (this.Parent != null)
                this.Parent.Controls.Remove(this);
            this.Dispose();
        }

        private void ToggleMaximize()
        {
            if (!isMaximized)
            {
                normalBounds = this.Bounds;
                normalLocation = this.Location;

                Screen screen = Screen.FromControl(this);
                int taskbarHeight = 40;
                this.Bounds = new Rectangle(
                    0, 0,
                    screen.WorkingArea.Width,
                    screen.WorkingArea.Height - taskbarHeight
                );
                isMaximized = true;
                maxBtn.Text = "❐";
            }
            else
            {
                this.Bounds = normalBounds;
                this.Location = normalLocation;
                isMaximized = false;
                maxBtn.Text = "□";
            }
        }

        private void MinimizeWindow()
        {
            this.Visible = false;
            Minimized?.Invoke(this, EventArgs.Empty);
        }

        public void RestoreWindow()
        {
            this.Visible = true;
            this.BringToFront();
            if (isMaximized)
                ToggleMaximize();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST && !isMaximized)
            {
                Point pos = this.PointToClient(new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16));

                if (pos.Y <= titleBar.Height)
                {
                    base.WndProc(ref m);
                    return;
                }

                if (pos.X >= this.Width - RESIZE_MARGIN && pos.Y >= this.Height - RESIZE_MARGIN)
                {
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                    return;
                }
                else if (pos.X <= RESIZE_MARGIN && pos.Y >= this.Height - RESIZE_MARGIN)
                {
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                    return;
                }
                else if (pos.X >= this.Width - RESIZE_MARGIN && pos.Y <= RESIZE_MARGIN)
                {
                    m.Result = (IntPtr)HTTOPRIGHT;
                    return;
                }
                else if (pos.X <= RESIZE_MARGIN && pos.Y <= RESIZE_MARGIN)
                {
                    m.Result = (IntPtr)HTTOPLEFT;
                    return;
                }
                else if (pos.X >= this.Width - RESIZE_MARGIN)
                {
                    m.Result = (IntPtr)HTRIGHT;
                    return;
                }
                else if (pos.Y >= this.Height - RESIZE_MARGIN)
                {
                    m.Result = (IntPtr)HTBOTTOM;
                    return;
                }
                else if (pos.X <= RESIZE_MARGIN)
                {
                    m.Result = (IntPtr)HTLEFT;
                    return;
                }
                else if (pos.Y <= RESIZE_MARGIN)
                {
                    m.Result = (IntPtr)HTTOP;
                    return;
                }
            }

            base.WndProc(ref m);
        }
    }
}