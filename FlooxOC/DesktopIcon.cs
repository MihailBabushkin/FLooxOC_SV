using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class DesktopIcon : Button
    {
        private string displayName;
        private Image iconImage;
        private bool isDragging = false;
        private Point dragOffset;
        private bool isLeftMouseDown = false;
        private bool isRightMouseDown = false;
        private Timer leftClickTimer;
        private Timer rightClickTimer;
        private bool rightClickHeld = false;
        private bool hasMoved = false;
        private bool contextMenuShown = false;
        private bool isLeftClickCompleted = false;
        private DateTime lastClickTime;
        private Timer clickCooldownTimer;

        public string AppId { get; set; }
        public string Type { get; set; }

        public event EventHandler OnDelete;

        public DesktopIcon(string text, Image icon = null, string type = "app")
        {
            AppId = "";
            Type = type;
            displayName = text;
            lastClickTime = DateTime.MinValue;

            // ===== УВЕЛИЧЕННЫЙ РАЗМЕР ДЛЯ ЛУЧШЕГО КЛИКА =====
            this.Size = new Size(85, 80);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.TextAlign = ContentAlignment.BottomCenter;
            this.Font = new Font("Segoe UI", 8);
            this.ForeColor = Color.White;

            if (icon != null)
            {
                iconImage = (Image)icon.Clone();
                this.Image = iconImage;
                this.ImageAlign = ContentAlignment.TopCenter;
                this.Text = text;
            }
            else
            {
                this.Text = "📦\n" + text;
            }

            leftClickTimer = new Timer();
            leftClickTimer.Interval = 1000;
            leftClickTimer.Tick += LeftClickTimer_Tick;

            rightClickTimer = new Timer();
            rightClickTimer.Interval = 1800;
            rightClickTimer.Tick += RightClickTimer_Tick;

            clickCooldownTimer = new Timer();
            clickCooldownTimer.Interval = 1000;
            clickCooldownTimer.Tick += (s, e) =>
            {
                clickCooldownTimer.Stop();
                this.Enabled = true;
            };

            ContextMenuStrip menu = new ContextMenuStrip();

            if (Type != "system")
            {
                ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Удалить");
                deleteItem.Click += (s, e) => OnDelete?.Invoke(this, EventArgs.Empty);
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

        private void LeftClickTimer_Tick(object sender, EventArgs e)
        {
            leftClickTimer.Stop();
            if (isLeftMouseDown && !isDragging && !hasMoved)
            {
                isLeftClickCompleted = true;
                ExecuteSafeClick();
            }
        }

        private void RightClickTimer_Tick(object sender, EventArgs e)
        {
            rightClickTimer.Stop();
            rightClickHeld = true;
            if (isRightMouseDown && !isDragging)
            {
                isDragging = true;
                dragOffset = new Point(this.Width / 2, this.Height / 2);
                this.Cursor = Cursors.SizeAll;
                this.Capture = true;
            }
        }

        private void ExecuteSafeClick()
        {
            TimeSpan timeSinceLastClick = DateTime.Now - lastClickTime;
            if (timeSinceLastClick.TotalMilliseconds < 1000)
            {
                return;
            }

            this.Enabled = false;
            clickCooldownTimer.Start();
            lastClickTime = DateTime.Now;

            this.OnClick(EventArgs.Empty);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isLeftMouseDown = true;
                isLeftClickCompleted = false;
                hasMoved = false;
                this.Capture = true;
                this.Cursor = Cursors.Hand;
                leftClickTimer.Start();
            }
            else if (e.Button == MouseButtons.Right)
            {
                isRightMouseDown = true;
                rightClickHeld = false;
                contextMenuShown = false;
                hasMoved = false;
                this.Cursor = Cursors.Hand;
                rightClickTimer.Start();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging && isRightMouseDown && this.Capture)
            {
                Point newLocation = this.Parent.PointToClient(this.PointToScreen(new Point(e.X, e.Y)));
                newLocation.Offset(-dragOffset.X, -dragOffset.Y);
                this.Location = newLocation;
                hasMoved = true;
            }
            else if (isLeftMouseDown && this.Capture)
            {
                if (Math.Abs(e.X - this.Width / 2) > 3 || Math.Abs(e.Y - this.Height / 2) > 3)
                {
                    hasMoved = true;
                    leftClickTimer.Stop();
                }
            }
            else if (isRightMouseDown && !isDragging && this.Capture)
            {
                if (Math.Abs(e.X - this.Width / 2) > 5 || Math.Abs(e.Y - this.Height / 2) > 5)
                {
                    rightClickTimer.Stop();
                    isDragging = true;
                    dragOffset = new Point(this.Width / 2, this.Height / 2);
                    this.Cursor = Cursors.SizeAll;
                    hasMoved = true;
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isLeftMouseDown = false;
                leftClickTimer.Stop();
                this.Cursor = Cursors.Hand;

                if (!isLeftClickCompleted)
                {
                    if (!isDragging && !hasMoved)
                    {
                        ExecuteSafeClick();
                    }
                }

                this.Capture = false;
                isLeftClickCompleted = false;
                hasMoved = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                isRightMouseDown = false;
                rightClickTimer.Stop();
                this.Cursor = Cursors.Hand;

                if (isDragging)
                {
                    isDragging = false;
                    this.Capture = false;
                }
                else if (!rightClickHeld && !hasMoved && !contextMenuShown)
                {
                    contextMenuShown = true;
                    if (this.ContextMenuStrip != null)
                    {
                        this.ContextMenuStrip.Show(this, new Point(0, this.Height));
                    }
                }

                rightClickHeld = false;
                hasMoved = false;
                this.Capture = false;
            }

            base.OnMouseUp(e);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_CONTEXTMENU = 0x007B;
            if (m.Msg == WM_CONTEXTMENU)
            {
                if (!isDragging && this.ContextMenuStrip != null)
                {
                    this.ContextMenuStrip.Show(this, new Point(0, this.Height));
                }
                return;
            }
            base.WndProc(ref m);
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
                if (leftClickTimer != null)
                {
                    leftClickTimer.Stop();
                    leftClickTimer.Dispose();
                }
                if (rightClickTimer != null)
                {
                    rightClickTimer.Stop();
                    rightClickTimer.Dispose();
                }
                if (clickCooldownTimer != null)
                {
                    clickCooldownTimer.Stop();
                    clickCooldownTimer.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}