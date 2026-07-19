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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // === ПРОВЕРКА АКТИВАЦИИ ===
                if (AccountManager.IsFirstRun())
                {
                    // Если первый запуск и нет кода - запускаем демо
                    DialogResult result = MessageBox.Show(
                        "🆓 Добро пожаловать!\n\n" +
                        "У вас есть возможность:\n" +
                        "1. ✅ Активировать систему (ввести код)\n" +
                        "2. 🆓 Начать ДЕМО-режим (45 минут)\n\n" +
                        "💡 Тестовый код: DEMO-2024\n\n" +
                        "Выберите вариант:",
                        "Активация",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Активация
                        using (var activationDialog = new ActivationDialog())
                        {
                            activationDialog.ShowDialog();
                            if (!activationDialog.IsActivated)
                            {
                                MessageBox.Show("Для использования системы необходима активация!\n" +
                                    "Или выберите ДЕМО-режим.",
                                    "Ошибка",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Демо-режим
                        AccountManager.StartDemoMode(new Form1());
                    }
                }

                // === ПРОВЕРКА ДЕМО-РЕЖИМА ===
                if (AccountManager.IsDemoMode && AccountManager.IsDemoExpired)
                {
                    MessageBox.Show("⏰ Демо-режим истёк!\n" +
                        "Для продолжения работы активируйте систему.",
                        "Демо-режим завершён",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // === ПРОВЕРКА ВХОДА ===
                bool loggedIn = false;

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
                else if (!AccountManager.IsDemoMode)
                {
                    AccountManager.RegisterUser("guest", "", "Гость");
                    AccountManager.Login("guest", "");
                }
                else
                {
                    // В демо-режиме создаём временного пользователя
                    AccountManager.RegisterUser("demo", "", "Демо-пользователь");
                    AccountManager.Login("demo", "");
                }

                // === ЗАПУСК ОС ===
                var mainForm = new Form1();

                // Если демо-режим, проверяем статус
                if (AccountManager.IsDemoMode)
                {
                    mainForm.Load += (s, e) => mainForm.CheckDemoStatus();
                }

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: {ex.Message}\n\n{ex.StackTrace}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}