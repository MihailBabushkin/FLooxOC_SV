using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FlooxOC
{
    public class NotepadApp : Panel
    {
        private RichTextBox textBox;
        private string currentFile = "";
        private MenuStrip menuStrip;
        private ToolStripStatusLabel statusLabel;
        private StatusStrip statusStrip;

        public NotepadApp()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            InitializeMenu();
            InitializeTextBox();
            InitializeStatusBar();

            // Устанавливаем фокус на текстовое поле
            this.ParentChanged += (s, e) =>
            {
                if (this.Parent != null)
                    textBox.Focus();
            };
        }

        private void InitializeMenu()
        {
            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(192, 192, 192),
                Dock = DockStyle.Top,
                Height = 28
            };

            // Файл
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Новый", null, (s, e) => NewFile());
            fileMenu.DropDownItems.Add("Открыть...", null, (s, e) => OpenFile());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Сохранить", null, (s, e) => SaveFile());
            fileMenu.DropDownItems.Add("Сохранить как...", null, (s, e) => SaveFileAs());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Выход", null, (s, e) => this.Parent?.Controls.Remove(this));

            // Правка
            ToolStripMenuItem editMenu = new ToolStripMenuItem("Правка");
            editMenu.DropDownItems.Add("Отменить", null, (s, e) => textBox.Undo());
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Вырезать", null, (s, e) => textBox.Cut());
            editMenu.DropDownItems.Add("Копировать", null, (s, e) => textBox.Copy());
            editMenu.DropDownItems.Add("Вставить", null, (s, e) => textBox.Paste());
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Выделить всё", null, (s, e) => textBox.SelectAll());

            // Вид
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("Вид");
            ToolStripMenuItem wrapItem = new ToolStripMenuItem("Перенос по словам");
            wrapItem.Checked = true;
            wrapItem.Click += (s, e) =>
            {
                wrapItem.Checked = !wrapItem.Checked;
                textBox.WordWrap = wrapItem.Checked;
            };
            viewMenu.DropDownItems.Add(wrapItem);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(viewMenu);
            this.Controls.Add(menuStrip);
        }

        private void InitializeTextBox()
        {
            textBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Courier New", 11),
                BackColor = Color.White,
                ForeColor = Color.Black,
                WordWrap = true,
                DetectUrls = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Обновляем статус при изменении текста
            textBox.TextChanged += (s, e) =>
            {
                UpdateStatus();
            };

            textBox.SelectionChanged += (s, e) =>
            {
                UpdateStatus();
            };

            this.Controls.Add(textBox);
        }

        private void InitializeStatusBar()
        {
            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(192, 192, 192)
            };

            statusLabel = new ToolStripStatusLabel
            {
                Text = "Готово",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);
        }

        private void UpdateStatus()
        {
            if (textBox != null && statusLabel != null)
            {
                int lineCount = textBox.Lines.Length;
                int charCount = textBox.Text.Length;
                int currentLine = textBox.GetLineFromCharIndex(textBox.SelectionStart) + 1;
                int currentPos = textBox.SelectionStart - textBox.GetFirstCharIndexOfCurrentLine() + 1;

                string fileName = string.IsNullOrEmpty(currentFile) ? "Без названия" : Path.GetFileName(currentFile);
                statusLabel.Text = $"{fileName}  |  Строк: {lineCount}  |  Символов: {charCount}  |  Строка {currentLine}, позиция {currentPos}";
            }
        }

        private void NewFile()
        {
            if (textBox.Text.Length > 0)
            {
                DialogResult result = MessageBox.Show("Сохранить изменения?", "Блокнот",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    SaveFile();
                else if (result == DialogResult.Cancel)
                    return;
            }
            textBox.Clear();
            currentFile = "";
            if (this.Parent != null)
                this.Parent.Text = "Блокнот - Без названия";
            UpdateStatus();
        }

        private void OpenFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        textBox.Text = File.ReadAllText(openFileDialog.FileName);
                        currentFile = openFileDialog.FileName;
                        if (this.Parent != null)
                            this.Parent.Text = $"Блокнот - {Path.GetFileName(currentFile)}";
                        UpdateStatus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка открытия файла: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(currentFile))
            {
                SaveFileAs();
            }
            else
            {
                try
                {
                    File.WriteAllText(currentFile, textBox.Text);
                    UpdateStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveFileAs()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, textBox.Text);
                        currentFile = saveFileDialog.FileName;
                        if (this.Parent != null)
                            this.Parent.Text = $"Блокнот - {Path.GetFileName(currentFile)}";
                        UpdateStatus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}