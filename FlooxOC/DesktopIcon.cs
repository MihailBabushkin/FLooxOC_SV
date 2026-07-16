using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class DesktopIcon : Button
    {
        private string displayName;
        private Image iconImage;

        public string AppId { get; set; }
        public string Type { get; set; }

        public event EventHandler IconClick;
        public event EventHandler IconDelete;
        public event EventHandler<Point> IconMoved;

        public DesktopIcon(string text, Image icon = null, string type = "app")
        {
            AppId = "";
            Type = type;
            displayName = text;

            this.Size = new Size(80, 75);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.TextAlign = ContentAlignment.BottomCenter;
            this.Text = text;
            this.Font = new Font("Segoe UI", 8);
            this.ForeColor = Color.White;

            if (icon != null)
            {
                iconImage = (Image)icon.Clone();
                this.Image = iconImage;
                this.ImageAlign = ContentAlignment.TopCenter;
            }
            else
            {
                this.Text = "📦\n" + text;
            }

            // === КЛИК ===
            this.Click += (s, e) =>
            {
                IconClick?.Invoke(this, EventArgs.Empty);
            };

            // === ПЕРЕТАСКИВАНИЕ ===
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Capture = true;
                }
            };

            this.MouseMove += (s, e) =>
            {
                if (this.Capture && e.Button == MouseButtons.Left)
                {
                    int dx = e.X - this.Width / 2;
                    int dy = e.Y - this.Height / 2;
                    this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
                    IconMoved?.Invoke(this, this.Location);
                }
            };

            this.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Capture = false;
                }
            };

            // === КОНТЕКСТНОЕ МЕНЮ ===
            ContextMenuStrip menu = new ContextMenuStrip();

            if (Type != "system")
            {
                ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Удалить");
                deleteItem.Click += (s, e) => IconDelete?.Invoke(this, EventArgs.Empty);
                menu.Items.Add(deleteItem);
            }

            if (Type == "bookmark" || Type == "app")
            {
                ToolStripMenuItem renameItem = new ToolStripMenuItem("✏️ Переименовать");
                renameItem.Click += (s, e) => StartRename();
                menu.Items.Add(renameItem);
            }

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem propertiesItem = new ToolStripMenuItem("📋 Свойства");
            propertiesItem.Click += (s, e) => ShowProperties();
            menu.Items.Add(propertiesItem);

            this.ContextMenuStrip = menu;
        }

        private void StartRename()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Переименовать";
                dialog.Size = new Size(350, 120);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.FromArgb(192, 192, 192);

                Label labelName = new Label();
                labelName.Text = "Новое название:";
                labelName.Location = new Point(10, 10);
                labelName.Size = new Size(100, 20);
                dialog.Controls.Add(labelName);

                TextBox textBox = new TextBox();
                textBox.Text = GetText();
                textBox.Location = new Point(120, 10);
                textBox.Size = new Size(200, 20);
                dialog.Controls.Add(textBox);

                Button okBtn = new Button();
                okBtn.Text = "OK";
                okBtn.Location = new Point(150, 45);
                okBtn.Size = new Size(75, 25);
                okBtn.DialogResult = DialogResult.OK;
                dialog.Controls.Add(okBtn);

                Button cancelBtn = new Button();
                cancelBtn.Text = "Отмена";
                cancelBtn.Location = new Point(235, 45);
                cancelBtn.Size = new Size(75, 25);
                cancelBtn.DialogResult = DialogResult.Cancel;
                dialog.Controls.Add(cancelBtn);

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(textBox.Text))
                {
                    string newName = textBox.Text;
                    displayName = newName;
                    this.Text = newName;
                }
            }
        }

        private void ShowProperties()
        {
            string info = "Название: " + GetText() + "\n" +
                          "Тип: " + Type + "\n" +
                          "ID: " + AppId + "\n" +
                          "Позиция: " + this.Location.ToString();

            MessageBox.Show(info, "Свойства", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetIcon(Image icon)
        {
            if (icon != null)
            {
                if (iconImage != null) iconImage.Dispose();
                iconImage = (Image)icon.Clone();
                this.Image = iconImage;
                this.ImageAlign = ContentAlignment.TopCenter;
                this.Text = displayName;
            }
        }

        public void SetText(string text)
        {
            displayName = text;
            this.Text = text;
        }

        public string GetText()
        {
            return displayName;
        }

        public void SetPosition(Point position)
        {
            this.Location = position;
        }

        public Point GetPosition()
        {
            return this.Location;
        }

        public void UpdateIcon(Image newIcon)
        {
            SetIcon(newIcon);
        }

        public void UpdateText(string newText)
        {
            SetText(newText);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (iconImage != null) iconImage.Dispose();
                if (this.ContextMenuStrip != null) this.ContextMenuStrip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}