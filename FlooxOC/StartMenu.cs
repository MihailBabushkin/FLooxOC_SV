using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class StartMenu : Form
    {
        public string SelectedApp { get; private set; } = "";

        public StartMenu()
        {
            this.Text = "Floox OC. Home version - Меню Пуск";
            this.Size = new Size(300, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var header = new Label
            {
                Text = "Floox OC. Home version",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Navy,
                ForeColor = Color.White
            };
            panel.Controls.Add(header);

            int y = 50;
            foreach (var app in new[] { "Калькулятор", "Блокнот", "Браузер", "Выключить" })
            {
                var btn = new Button
                {
                    Text = app,
                    Font = new Font("Segoe UI", 10),
                    Size = new Size(250, 35),
                    Location = new Point(20, y),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(192, 192, 192)
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, e) =>
                {
                    SelectedApp = app;
                    this.Close();
                };
                panel.Controls.Add(btn);
                y += 45;
            }

            this.Controls.Add(panel);
        }
    }
}