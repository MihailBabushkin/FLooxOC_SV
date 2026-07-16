using FlooxOC;
using System;
using System.Windows.Forms;

namespace FlooxOC
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // === ПРОВЕРКА АКТИВАЦИИ ===
                if (AccountManager.IsFirstRun())
                {
                    using (var activationDialog = new ActivationDialog())
                    {
                        activationDialog.ShowDialog();
                        if (!activationDialog.IsActivated)
                        {
                            MessageBox.Show("Для использования системы необходима активация!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                // === ПРОВЕРКА ВХОДА ===
                bool loggedIn = false;

                // Если есть пользователи или требуется пароль
                if (AccountManager.GetAllUsers().Count > 0 || AccountManager.RequirePassword)
                {
                    using (var loginDialog = new LoginDialog())
                    {
                        loginDialog.ShowDialog();
                        loggedIn = loginDialog.IsLoggedIn;
                    }

                    if (!loggedIn)
                    {
                        Application.Exit();
                        return;
                    }
                }
                else
                {
                    // Если нет пользователей и пароль не требуется - создаём гостя
                    AccountManager.RegisterUser("guest", "", "Гость");
                    AccountManager.Login("guest", "");
                }

                // === ЗАПУСК ОС ===
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: {ex.Message}\n\n{ex.StackTrace}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}