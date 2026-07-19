using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlooxOC
{
    public static class ColorHelper
    {
        /// <summary>
        /// Определяет, является ли цвет тёмным
        /// </summary>
        public static bool CheckIsDarkColor(Color color)
        {
            double brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            return brightness < 0.5;
        }

        /// <summary>
        /// Возвращает контрастный цвет (белый для тёмного фона, чёрный для светлого)
        /// </summary>
        public static Color GetContrastTextColor(Color backgroundColor)
        {
            return CheckIsDarkColor(backgroundColor) ? Color.White : Color.Black;
        }

        /// <summary>
        /// Применяет контрастные цвета ко всем элементам управления
        /// </summary>
        public static void ApplyContrastToControls(Control parentControl)
        {
            if (parentControl == null) return;

            Color backgroundColor = parentControl.BackColor;
            Color textColor = GetContrastTextColor(backgroundColor);

            ApplyColorsRecursive(parentControl, backgroundColor, textColor);
        }

        /// <summary>
        /// Рекурсивно применяет цвета ко всем контролам
        /// </summary>
        private static void ApplyColorsRecursive(Control control, Color backgroundColor, Color textColor)
        {
            if (control == null) return;

            Color currentBackgroundColor = control.BackColor;
            Color currentTextColor = GetContrastTextColor(currentBackgroundColor);

            // Применяем цвета в зависимости от типа контрола
            if (control is Label label)
            {
                label.ForeColor = currentTextColor;
            }
            else if (control is TextBox textBox)
            {
                textBox.ForeColor = currentTextColor;
                textBox.BackColor = currentTextColor == Color.White ? Color.FromArgb(40, 40, 40) : Color.White;
            }
            else if (control is RichTextBox richTextBox)
            {
                richTextBox.ForeColor = currentTextColor;
                richTextBox.BackColor = currentTextColor == Color.White ? Color.FromArgb(40, 40, 40) : Color.White;
            }
            else if (control is Button button)
            {
                button.ForeColor = GetContrastTextColor(button.BackColor);
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.ForeColor = currentTextColor;
            }
            else if (control is RadioButton radioButton)
            {
                radioButton.ForeColor = currentTextColor;
            }
            else if (control is ListBox listBox)
            {
                listBox.ForeColor = currentTextColor;
                listBox.BackColor = currentTextColor == Color.White ? Color.FromArgb(40, 40, 40) : Color.White;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.ForeColor = currentTextColor;
                comboBox.BackColor = currentTextColor == Color.White ? Color.FromArgb(40, 40, 40) : Color.White;
            }
            else if (control is NumericUpDown numericUpDown)
            {
                numericUpDown.ForeColor = currentTextColor;
                numericUpDown.BackColor = currentTextColor == Color.White ? Color.FromArgb(40, 40, 40) : Color.White;
            }
            else if (control is GroupBox groupBox)
            {
                groupBox.ForeColor = currentTextColor;
            }
            // Panel пропускаем, но обрабатываем дочерние

            // Рекурсивно обрабатываем все дочерние контролы
            foreach (Control childControl in control.Controls)
            {
                ApplyColorsRecursive(childControl, backgroundColor, textColor);
            }
        }

        /// <summary>
        /// Обновляет цвета всех открытых окон
        /// </summary>
        public static void RefreshAllWindowsColors()
        {
            foreach (Form form in Application.OpenForms)
            {
                ApplyContrastToControls(form);
                form.Refresh();
            }
        }

        /// <summary>
        /// Создаёт цвет из строки HTML (#RRGGBB или RRGGBB)
        /// </summary>
        public static Color CreateColorFromHex(string hexCode)
        {
            try
            {
                hexCode = hexCode.TrimStart('#');
                int red = Convert.ToInt32(hexCode.Substring(0, 2), 16);
                int green = Convert.ToInt32(hexCode.Substring(2, 2), 16);
                int blue = Convert.ToInt32(hexCode.Substring(4, 2), 16);
                return Color.FromArgb(red, green, blue);
            }
            catch
            {
                return Color.Gray;
            }
        }

        /// <summary>
        /// Преобразует цвет в HTML строку (#RRGGBB)
        /// </summary>
        public static string ConvertColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Возвращает читаемый цвет на указанном фоне
        /// </summary>
        public static Color GetReadableTextColor(Color backgroundColor, Color preferredColor)
        {
            double contrastRatio = CalculateContrastRatio(backgroundColor, preferredColor);
            if (contrastRatio < 4.5) // WCAG стандарт для обычного текста
            {
                return GetContrastTextColor(backgroundColor);
            }
            return preferredColor;
        }

        /// <summary>
        /// Вычисляет коэффициент контрастности между двумя цветами (WCAG)
        /// </summary>
        public static double CalculateContrastRatio(Color color1, Color color2)
        {
            double luminance1 = CalculateRelativeLuminance(color1);
            double luminance2 = CalculateRelativeLuminance(color2);
            return (Math.Max(luminance1, luminance2) + 0.05) / (Math.Min(luminance1, luminance2) + 0.05);
        }

        /// <summary>
        /// Вычисляет относительную яркость цвета (WCAG)
        /// </summary>
        private static double CalculateRelativeLuminance(Color color)
        {
            double red = color.R / 255.0;
            double green = color.G / 255.0;
            double blue = color.B / 255.0;

            red = red <= 0.03928 ? red / 12.92 : Math.Pow((red + 0.055) / 1.055, 2.4);
            green = green <= 0.03928 ? green / 12.92 : Math.Pow((green + 0.055) / 1.055, 2.4);
            blue = blue <= 0.03928 ? blue / 12.92 : Math.Pow((blue + 0.055) / 1.055, 2.4);

            return 0.2126 * red + 0.7152 * green + 0.0722 * blue;
        }

        /// <summary>
        /// Проверяет, достаточно ли контрастны два цвета (WCAG уровень AA)
        /// </summary>
        public static bool IsContrastSufficientForAccessibility(Color foregroundColor, Color backgroundColor)
        {
            return CalculateContrastRatio(foregroundColor, backgroundColor) >= 4.5;
        }

        /// <summary>
        /// Затемняет цвет на указанный процент
        /// </summary>
        public static Color DarkenColor(Color color, int percent)
        {
            int factor = 100 - Math.Min(100, percent);
            return Color.FromArgb(
                Math.Min(255, color.R * factor / 100),
                Math.Min(255, color.G * factor / 100),
                Math.Min(255, color.B * factor / 100)
            );
        }

        /// <summary>
        /// Осветляет цвет на указанный процент
        /// </summary>
        public static Color LightenColor(Color color, int percent)
        {
            int factor = 100 + Math.Min(100, percent);
            return Color.FromArgb(
                Math.Min(255, color.R * factor / 100),
                Math.Min(255, color.G * factor / 100),
                Math.Min(255, color.B * factor / 100)
            );
        }

        /// <summary>
        /// Преобразует цвет в оттенки серого
        /// </summary>
        public static Color ConvertToGrayscale(Color color)
        {
            int grayValue = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
            return Color.FromArgb(grayValue, grayValue, grayValue);
        }

        /// <summary>
        /// Получает цвет для стандартного фона приложений
        /// </summary>
        public static Color GetDefaultAppBackgroundColor()
        {
            return Color.White;
        }

        /// <summary>
        /// Получает цвет для стандартного текста приложений
        /// </summary>
        public static Color GetDefaultAppTextColor()
        {
            return Color.Black;
        }

        /// <summary>
        /// Проверяет, является ли цвет светлым
        /// </summary>
        public static bool CheckIsLightColor(Color color)
        {
            return !CheckIsDarkColor(color);
        }

        /// <summary>
        /// Возвращает инвертированный цвет
        /// </summary>
        public static Color GetInvertedColor(Color color)
        {
            return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
        }
    }
}