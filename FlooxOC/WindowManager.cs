using FlooxOC;
using System;
using System.Windows.Forms;

namespace FlooxOC
{
    public class WindowManager
    {
        private Form parentForm;
        private Taskbar taskbar;

        public WindowManager(Form parent, Taskbar taskbar)
        {
            this.parentForm = parent;
            this.taskbar = taskbar;
        }

        public CustomWindow CreateWindow(string title, string content = "")
        {
            CustomWindow window = new CustomWindow(title, content);

            window.Closed += (s, e) =>
            {
                foreach (Control ctrl in taskbar.Controls)
                {
                    if (ctrl is Button btn && btn.Text == title)
                    {
                        taskbar.Controls.Remove(btn);
                        break;
                    }
                }
            };

            window.Minimized += (s, e) =>
            {
                taskbar.AddWindowButton(title, (sender, args) =>
                {
                    window.RestoreWindow();
                    if (sender is Button btn)
                        taskbar.Controls.Remove(btn);
                });
            };

            parentForm.Controls.Add(window);
            window.BringToFront();

            return window;
        }
    }
}