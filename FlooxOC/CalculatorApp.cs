using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public class CalculatorApp : Panel
    {
        private TextBox display;
        private string currentInput = "";
        private string operation = "";
        private double firstNumber = 0;
        private bool isNewEntry = true;
        private bool isResultShown = false;
        private Label historyLabel;

        public CalculatorApp()
        {
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(5);
            this.AutoScroll = true;

            InitializeDisplay();
            InitializeHistory();
            InitializeButtons();

            // Применяем контрастные цвета
            ColorHelper.ApplyContrastToControls(this);
        }

        private void InitializeDisplay()
        {
            display = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(250, 45),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right,
                BackColor = Color.White,
                ReadOnly = true,
                Text = "0"
            };
            // Применяем контрастный цвет текста
            display.ForeColor = ColorHelper.GetContrastTextColor(display.BackColor);
            this.Controls.Add(display);
        }

        private void InitializeHistory()
        {
            historyLabel = new Label
            {
                Location = new Point(5, 55),
                Size = new Size(250, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = ColorHelper.GetContrastTextColor(this.BackColor),
                Text = "",
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(historyLabel);
        }

        private void InitializeButtons()
        {
            string[,] buttons = {
                { "C", "⌫", "%", "÷" },
                { "7", "8", "9", "×" },
                { "4", "5", "6", "-" },
                { "1", "2", "3", "+" },
                { "±", "0", ",", "=" }
            };

            int x = 5, y = 80;
            for (int row = 0; row < buttons.GetLength(0); row++)
            {
                for (int col = 0; col < buttons.GetLength(1); col++)
                {
                    Button btn = new Button
                    {
                        Text = buttons[row, col],
                        Size = new Size(70, 55),
                        Location = new Point(x + col * 75, y + row * 60),
                        Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = GetButtonColor(buttons[row, col]),
                        Cursor = Cursors.Hand
                    };
                    // Применяем контрастный цвет текста для кнопки
                    btn.ForeColor = ColorHelper.GetContrastTextColor(btn.BackColor);
                    btn.FlatAppearance.BorderSize = 1;
                    btn.Click += Button_Click;
                    this.Controls.Add(btn);
                }
            }
        }

        private Color GetButtonColor(string text)
        {
            if (text == "=")
                return Color.FromArgb(0, 120, 215);
            if (text == "C" || text == "⌫")
                return Color.FromArgb(200, 80, 80);
            if (text == "÷" || text == "×" || text == "-" || text == "+")
                return Color.FromArgb(220, 220, 220);
            return Color.FromArgb(240, 240, 240);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string text = btn.Text;

            switch (text)
            {
                case "C": ClearAll(); break;
                case "⌫": Backspace(); break;
                case "=": CalculateResult(); break;
                case "+": case "-": case "×": case "÷": SetOperation(text); break;
                case "±": ToggleSign(); break;
                case "%": Percentage(); break;
                case ",": AddDecimal(); break;
                default: AddDigit(text); break;
            }
        }

        private void ClearAll()
        {
            currentInput = "";
            firstNumber = 0;
            operation = "";
            isNewEntry = true;
            isResultShown = false;
            display.Text = "0";
            historyLabel.Text = "";
        }

        private void Backspace()
        {
            if (!isNewEntry && currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                display.Text = currentInput == "" ? "0" : currentInput;
            }
        }

        private void AddDigit(string digit)
        {
            if (isResultShown)
            {
                currentInput = "";
                isResultShown = false;
            }

            if (isNewEntry)
            {
                currentInput = digit;
                isNewEntry = false;
            }
            else
            {
                if (currentInput.Length < 15)
                    currentInput += digit;
            }
            display.Text = currentInput;
        }

        private void AddDecimal()
        {
            if (isResultShown)
            {
                currentInput = "0,";
                isResultShown = false;
                isNewEntry = false;
                display.Text = currentInput;
                return;
            }

            if (isNewEntry)
            {
                currentInput = "0,";
                isNewEntry = false;
            }
            else if (!currentInput.Contains(","))
            {
                currentInput += ",";
            }
            display.Text = currentInput;
        }

        private void ToggleSign()
        {
            if (currentInput != "" && currentInput != "0")
            {
                if (currentInput.StartsWith("-"))
                    currentInput = currentInput.Substring(1);
                else
                    currentInput = "-" + currentInput;
                display.Text = currentInput;
            }
        }

        private void Percentage()
        {
            if (double.TryParse(currentInput, out double num))
            {
                num /= 100;
                currentInput = num.ToString();
                display.Text = currentInput;
                isResultShown = true;
            }
        }

        private void SetOperation(string op)
        {
            if (currentInput != "")
            {
                if (!isNewEntry)
                {
                    if (operation != "")
                        CalculateResult();
                    else
                        firstNumber = double.Parse(currentInput);
                }
                operation = op;
                historyLabel.Text = $"{firstNumber} {operation}";
                isNewEntry = true;
                isResultShown = false;
            }
        }

        private void CalculateResult()
        {
            if (operation == "" || isNewEntry)
                return;

            double secondNumber = double.Parse(currentInput);
            double result = 0;

            switch (operation)
            {
                case "+": result = firstNumber + secondNumber; break;
                case "-": result = firstNumber - secondNumber; break;
                case "×": result = firstNumber * secondNumber; break;
                case "÷":
                    if (secondNumber != 0)
                        result = firstNumber / secondNumber;
                    else
                    {
                        display.Text = "Ошибка";
                        historyLabel.Text = "";
                        return;
                    }
                    break;
            }

            historyLabel.Text = $"{firstNumber} {operation} {secondNumber} =";
            currentInput = result.ToString();
            display.Text = currentInput;
            operation = "";
            isNewEntry = true;
            isResultShown = true;
            firstNumber = result;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            display.Width = this.ClientSize.Width - 15;
            historyLabel.Width = this.ClientSize.Width - 15;
        }
    }
}