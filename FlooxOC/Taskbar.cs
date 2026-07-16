using System;
using System.Drawing;
using System.Windows.Forms;
namespace FlooxOC { 
    public class Taskbar : Panel
    {
        private Button startButton;
        private Panel taskbarButtons;
        private Label clockLabel;
        private System.Windows.Forms.Timer clockTimer;

        public event EventHandler StartButtonClicked;
        public bool ShowClock { get; set; } = true;

        public Taskbar()
        {
            this.Height = 40;
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Dock = DockStyle.Bottom;
            this.BorderStyle = BorderStyle.Fixed3D;

            InitializeStartButton();
            InitializeTaskbarButtons();
            InitializeClock();
        }

        private void InitializeStartButton()
        {
            startButton = new Button
            {
                Text = " Пуск ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(192, 192, 192),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(70, 30),
                Location = new Point(5, 5)
            };
            startButton.FlatAppearance.BorderSize = 1;
            startButton.Click += (s, e) => StartButtonClicked?.Invoke(this, EventArgs.Empty);
            this.Controls.Add(startButton);
        }

        private void InitializeTaskbarButtons()
        {
            taskbarButtons = new Panel
            {
                Location = new Point(80, 5),
                Size = new Size(this.Width - 180, 30),
                BackColor = Color.FromArgb(192, 192, 192)
            };
            this.Controls.Add(taskbarButtons);
        }

        private void InitializeClock()
        {
            clockLabel = new Label
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(192, 192, 192),
                AutoSize = false,
                Size = new Size(70, 30),
                Location = new Point(this.Width - 80, 5),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(clockLabel);

            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) =>
            {
                if (ShowClock)
                    clockLabel.Text = DateTime.Now.ToString("HH:mm");
            };
            clockTimer.Start();
        }

        // 👇 ТОЛЬКО ОДИН РАЗ! Убери дубликат, если он есть
        public void AddWindowButton(string title, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = title,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(192, 192, 192),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 26),
                Margin = new Padding(2)
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += onClick;
            taskbarButtons.Controls.Add(btn);
        }

        public void RemoveWindowButton(Button btn)
        {
            taskbarButtons.Controls.Remove(btn);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (clockLabel != null)
                clockLabel.Location = new Point(this.Width - 80, 5);
            if (taskbarButtons != null)
                taskbarButtons.Width = this.Width - 180;
        }
    }
}