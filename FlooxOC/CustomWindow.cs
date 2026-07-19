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

                    value.BackColor = DefaultBackground;

                    Color textColor = ColorHelper.GetContrastTextColor(DefaultBackground);
                    SetControlForeColor(value, textColor);
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
            this.BackColor = DefaultBackground;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.DoubleBuffered = true;

            InitializeTitleBar(title);
            InitializeContent(content);
            SetDefaultPosition();
            this.Resize += OnResize;

            Color textColor = ColorHelper.GetContrastTextColor(DefaultBackground);
            SetControlForeColor(this, textColor);

            this.MouseDown += OnWindowMouseDown;
            this.Click += OnWindowClick;
            titleBar.MouseDown += OnWindowMouseDown;
        }

        // ====== СОБЫТИЯ ДЛЯ ПОДНЯТИЯ ОКНА ======
        private void OnWindowMouseDown(object sender, MouseEventArgs e)
        {
            BringToFront();
        }

        private void OnWindowClick(object sender, EventArgs e)
        {
            BringToFront();
        }

        // ====== ПЕРЕТАСКИВАНИЕ ======
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !isMaximized)
            {
                isDragging = true;
                dragOffset = new Point(e.X, e.Y);
                titleBar.Capture = true;
                BringToFront();
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

        // ====== РЕСАЙЗ ======
        private bool isResizing = false;
        private Point resizeStart;
        private Size resizeStartSize;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!isMaximized)
            {
                bool onRight = e.X >= this.Width - RESIZE_MARGIN;
                bool onBottom = e.Y >= this.Height - RESIZE_MARGIN;
                bool onLeft = e.X <= RESIZE_MARGIN;
                bool onTop = e.Y <= RESIZE_MARGIN;

                if (onRight && onBottom)
                    this.Cursor = Cursors.SizeNWSE;
                else if (onLeft && onBottom)
                    this.Cursor = Cursors.SizeNESW;
                else if (onRight && onTop)
                    this.Cursor = Cursors.SizeNESW;
                else if (onLeft && onTop)
                    this.Cursor = Cursors.SizeNWSE;
                else if (onRight || onLeft)
                    this.Cursor = Cursors.SizeWE;
                else if (onBottom || onTop)
                    this.Cursor = Cursors.SizeNS;
                else
                    this.Cursor = Cursors.Default;
            }

            if (isResizing && !isMaximized)
            {
                int deltaX = e.X - resizeStart.X;
                int deltaY = e.Y - resizeStart.Y;

                bool onRight = resizeStart.X >= this.Width - RESIZE_MARGIN;
                bool onBottom = resizeStart.Y >= this.Height - RESIZE_MARGIN;
                bool onLeft = resizeStart.X <= RESIZE_MARGIN;
                bool onTop = resizeStart.Y <= RESIZE_MARGIN;

                int newWidth = this.Width;
                int newHeight = this.Height;
                int newX = this.Location.X;
                int newY = this.Location.Y;

                if (onRight)
                {
                    newWidth = Math.Max(this.MinimumSize.Width, this.Width + deltaX);
                }
                if (onLeft)
                {
                    int delta = Math.Min(deltaX, this.Width - this.MinimumSize.Width);
                    newWidth = this.Width - delta;
                    newX = this.Location.X + delta;
                }
                if (onBottom)
                {
                    newHeight = Math.Max(this.MinimumSize.Height, this.Height + deltaY);
                }
                if (onTop)
                {
                    int delta = Math.Min(deltaY, this.Height - this.MinimumSize.Height);
                    newHeight = this.Height - delta;
                    newY = this.Location.Y + delta;
                }

                this.Bounds = new Rectangle(newX, newY, newWidth, newHeight);
                BringToFront();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!isMaximized)
            {
                bool onRight = e.X >= this.Width - RESIZE_MARGIN;
                bool onBottom = e.Y >= this.Height - RESIZE_MARGIN;
                bool onLeft = e.X <= RESIZE_MARGIN;
                bool onTop = e.Y <= RESIZE_MARGIN;

                if (onRight || onBottom || onLeft || onTop)
                {
                    isResizing = true;
                    resizeStart = new Point(e.X, e.Y);
                    resizeStartSize = this.Size;
                    this.Capture = true;
                    BringToFront();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (isResizing)
            {
                isResizing = false;
                this.Capture = false;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.Cursor = Cursors.Default;
        }

        // ====== WNDPROC ДЛЯ РЕСАЙЗА ======
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

        // ====== ИНИЦИАЛИЗАЦИЯ ======
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
                BackColor = DefaultBackground,
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
                    BackColor = DefaultBackground,
                    ForeColor = ColorHelper.GetContrastTextColor(DefaultBackground)
                };
                contentPanel.Controls.Add(label);
            }

            this.Controls.Add(contentPanel);
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

            if (contentPanel != null)
            {
                contentPanel.Size = new Size(
                    this.ClientSize.Width,
                    this.ClientSize.Height - titleBar.Height
                );
            }
        }

        // ====== УПРАВЛЕНИЕ ОКНОМ ======
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

        // ====== ПУБЛИЧНЫЙ МЕТОД ДЛЯ ВОССТАНОВЛЕНИЯ ======
        public void RestoreWindow()
        {
            this.Visible = true;
            this.BringToFront();
            if (isMaximized)
                ToggleMaximize();
        }

        // ====== РАБОТА С ЦВЕТАМИ ======
        private void SetControlForeColor(Control control, Color color)
        {
            foreach (Control child in control.Controls)
            {
                if (child is Label label)
                {
                    label.ForeColor = color;
                }
                else if (child is TextBox textBox)
                {
                    textBox.ForeColor = color;
                }
                else if (child is RichTextBox richTextBox)
                {
                    richTextBox.ForeColor = color;
                }
                else if (child is Button button)
                {
                    button.ForeColor = ColorHelper.GetContrastTextColor(button.BackColor);
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.ForeColor = color;
                }
                else if (child is Panel panel)
                {
                    panel.BackColor = DefaultBackground;
                    SetControlForeColor(child, color);
                }
                else
                {
                    SetControlForeColor(child, color);
                }
            }
        }

        public static void UpdateAllWindows()
        {
            foreach (Form form in Application.OpenForms)
            {
                foreach (Control ctrl in form.Controls)
                {
                    if (ctrl is CustomWindow window)
                    {
                        window.UpdateColors();
                    }
                }
            }
        }

        public void UpdateColors()
        {
            this.BackColor = DefaultBackground;

            if (contentPanel != null)
            {
                contentPanel.BackColor = DefaultBackground;
                Color textColor = ColorHelper.GetContrastTextColor(DefaultBackground);
                SetControlForeColor(contentPanel, textColor);
            }

            // Обновляем заголовок
            Color textColorTitle = ColorHelper.GetContrastTextColor(DefaultBackground);
            titleLabel.ForeColor = textColorTitle;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (titleBar != null) titleBar.Dispose();
                if (contentPanel != null) contentPanel.Dispose();
                if (closeBtn != null) closeBtn.Dispose();
                if (maxBtn != null) maxBtn.Dispose();
                if (minBtn != null) minBtn.Dispose();
                if (titleLabel != null) titleLabel.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}