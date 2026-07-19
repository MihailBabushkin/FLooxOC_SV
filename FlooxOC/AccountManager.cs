using FlooxOC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlooxOC
{
    public class AccountManager
    {
        private static string DataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Floox OC. Home Version", "Accounts"
);
        private static string AccountsFile = Path.Combine(DataPath, "accounts.json");
        private static string SettingsFile = Path.Combine(DataPath, "settings.json");

        private const string ACTIVATION_SERVER = "https://your-server.com/api/activate";

        public class AccountData
        {
            public string Login { get; set; }
            public string PasswordHash { get; set; }
            public string ActivationCode { get; set; }
            public bool IsActivated { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime LastLogin { get; set; }
            public string UserName { get; set; }
            public string UserAvatar { get; set; }
            public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        }

        public class AppSettings
        {
            public bool RequirePassword { get; set; } = true;
            public bool AutoLogin { get; set; } = false;
            public string LastUser { get; set; } = "";
            public bool FirstRun { get; set; } = true;
            public string ActivationCode { get; set; } = "";
            public string MachineId { get; set; } = "";
            public int MaxLoginAttempts { get; set; } = 3;

            // === ДЕМО-РЕЖИМ ===
            public bool IsDemoMode { get; set; } = false;
            public DateTime DemoStartTime { get; set; } = DateTime.MinValue;
            public int DemoMinutesLimit { get; set; } = 45;
            public bool IsDemoExpired { get; set; } = false;
        }

        private static AppSettings settings = new AppSettings();
        private static AccountData currentUser = null;
        private static readonly HttpClient httpClient = new HttpClient();
        private static Timer demoWarningTimer;
        private static Timer demoExpireTimer;
        private static Form1 mainForm;

        public static bool RequirePassword
        {
            get => settings.RequirePassword;
            set
            {
                settings.RequirePassword = value;
                SaveSettings();
            }
        }

        public static bool AutoLogin
        {
            get => settings.AutoLogin;
            set
            {
                settings.AutoLogin = value;
                SaveSettings();
            }
        }

        public static int MaxLoginAttempts
        {
            get => settings.MaxLoginAttempts;
            set
            {
                settings.MaxLoginAttempts = value;
                SaveSettings();
            }
        }

        public static bool IsDemoMode
        {
            get => settings.IsDemoMode;
            set
            {
                settings.IsDemoMode = value;
                SaveSettings();
            }
        }

        public static bool IsDemoExpired
        {
            get => settings.IsDemoExpired;
            set
            {
                settings.IsDemoExpired = value;
                SaveSettings();
            }
        }

        static AccountManager()
        {
            Directory.CreateDirectory(DataPath);
            LoadSettings();

            if (string.IsNullOrEmpty(settings.MachineId))
            {
                settings.MachineId = GetMachineId();
                SaveSettings();
            }
        }

        private static string GetMachineId()
        {
            try
            {
                string combined = Environment.MachineName + Environment.UserName + Environment.OSVersion.Platform.ToString();

                try
                {
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (drive.IsReady)
                        {
                            combined += drive.TotalSize.ToString();
                            break;
                        }
                    }
                }
                catch { }

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    return Convert.ToBase64String(hash).Substring(0, 20);
                }
            }
            catch
            {
                return Environment.MachineName + "-" + Guid.NewGuid().ToString().Substring(0, 8);
            }
        }

        public static void SaveActivationCode(string code)
        {
            settings.ActivationCode = code;
            settings.IsDemoMode = false;
            settings.IsDemoExpired = false;
            SaveSettings();
        }

        public static async Task<bool> ActivateCode(string code)
        {
            try
            {
                var data = new
                {
                    code = code,
                    machine_id = settings.MachineId
                };
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(ACTIVATION_SERVER, content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(result);

                    if (responseData != null && responseData.ContainsKey("success") && (bool)responseData["success"])
                    {
                        settings.ActivationCode = code;
                        settings.FirstRun = false;
                        settings.IsDemoMode = false;
                        settings.IsDemoExpired = false;
                        settings.DemoStartTime = DateTime.MinValue;
                        SaveSettings();

                        StopDemoTimers();

                        return true;
                    }
                    else
                    {
                        string message = responseData.ContainsKey("message") ? responseData["message"].ToString() : "Неизвестная ошибка";
                        MessageBox.Show($"❌ {message}", "Ошибка активации",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show($"❌ Ошибка соединения с сервером! (Код: {response.StatusCode})", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch
            {
                if (ValidateCodeLocally(code))
                {
                    settings.ActivationCode = code;
                    settings.FirstRun = false;
                    settings.IsDemoMode = false;
                    settings.IsDemoExpired = false;
                    settings.DemoStartTime = DateTime.MinValue;
                    SaveSettings();

                    StopDemoTimers();

                    return true;
                }
                return false;
            }
        }

        public static bool ValidateCodeLocally(string code)
        {
            string[] validCodes = { "DEMO-2024", "TEST-1234", "FREE-2024" };
            foreach (var valid in validCodes)
            {
                if (code == valid)
                    return true;
            }
            return false;
        }

        public static bool RegisterUser(string login, string password, string userName = "")
        {
            if (string.IsNullOrEmpty(login) || login.Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var accounts = LoadAccounts();
            if (accounts.Exists(a => a.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (settings.RequirePassword)
            {
                if (string.IsNullOrEmpty(password) || password.Length < 3)
                {
                    MessageBox.Show("Пароль должен содержать минимум 3 символа!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            var newUser = new AccountData
            {
                Login = login,
                PasswordHash = settings.RequirePassword ? HashPassword(password) : "",
                ActivationCode = settings.ActivationCode,
                IsActivated = !settings.IsDemoMode,
                CreatedDate = DateTime.Now,
                LastLogin = DateTime.Now,
                UserName = string.IsNullOrEmpty(userName) ? login : userName
            };

            accounts.Add(newUser);
            SaveAccounts(accounts);

            currentUser = newUser;
            settings.LastUser = login;
            SaveSettings();

            return true;
        }

        public static bool Login(string login, string password)
        {
            var accounts = LoadAccounts();
            var user = accounts.Find(a => a.Login == login);

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (settings.RequirePassword)
            {
                if (HashPassword(password) != user.PasswordHash)
                {
                    MessageBox.Show("Неверный пароль!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            currentUser = user;
            user.LastLogin = DateTime.Now;
            SaveAccounts(accounts);
            settings.LastUser = login;
            SaveSettings();

            return true;
        }

        public static void Logout()
        {
            currentUser = null;
        }

        public static AccountData GetCurrentUser()
        {
            return currentUser;
        }

        public static bool IsLoggedIn()
        {
            return currentUser != null;
        }

        public static bool IsFirstRun()
        {
            return settings.FirstRun;
        }

        public static bool IsActivated()
        {
            return !string.IsNullOrEmpty(settings.ActivationCode) && !settings.IsDemoMode;
        }

        public static string GetLastUser()
        {
            return settings.LastUser;
        }

        public static List<AccountData> GetAllUsers()
        {
            return LoadAccounts();
        }

        public static void DeleteUser(string login)
        {
            var accounts = LoadAccounts();
            accounts.RemoveAll(a => a.Login == login);
            SaveAccounts(accounts);

            if (currentUser?.Login == login)
                currentUser = null;
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // ====== ДЕМО-РЕЖИМ ======
        public static void StartDemoMode(Form1 form)
        {
            mainForm = form;
            settings.IsDemoMode = true;
            settings.DemoStartTime = DateTime.Now;
            settings.IsDemoExpired = false;
            SaveSettings();

            // Таймер для предупреждения за 5 минут до окончания
            int warningMinutes = settings.DemoMinutesLimit - 5;
            if (warningMinutes > 0)
            {
                demoWarningTimer = new Timer();
                demoWarningTimer.Interval = warningMinutes * 60 * 1000;
                demoWarningTimer.Tick += (s, e) =>
                {
                    demoWarningTimer.Stop();
                    ShowDemoWarning();
                };
                demoWarningTimer.Start();
            }

            // Таймер для завершения демо
            demoExpireTimer = new Timer();
            demoExpireTimer.Interval = settings.DemoMinutesLimit * 60 * 1000;
            demoExpireTimer.Tick += (s, e) =>
            {
                demoExpireTimer.Stop();
                ExpireDemo();
            };
            demoExpireTimer.Start();

            // Показываем информацию о демо-режиме
            ShowDemoInfo();
        }

        private static void ShowDemoInfo()
        {
            MessageBox.Show(
                $"🆓 ДЕМО-РЕЖИМ\n\n" +
                $"Время работы: {settings.DemoMinutesLimit} минут\n" +
                $"Осталось: {settings.DemoMinutesLimit} минут\n\n" +
                $"⚠️ Для полноценной работы активируйте систему.\n" +
                $"Код активации можно получить у разработчика.\n\n" +
                $"💡 Тестовый код: DEMO-2024",
                "Демо-режим",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static void ShowDemoWarning()
        {
            if (mainForm == null || mainForm.IsDisposed) return;

            DialogResult result = MessageBox.Show(
                $"⚠️ ВНИМАНИЕ!\n\n" +
                $"До окончания демо-режима осталось 5 минут!\n\n" +
                $"Вы можете:\n" +
                $"1. Активировать систему (код активации)\n" +
                $"2. Продолжить работу в демо-режиме\n\n" +
                $"💡 Тестовый код: DEMO-2024\n" +
                $"📝 Введите код в настройках аккаунта.",
                "Демо-режим заканчивается",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Открываем окно активации
                mainForm.ShowActivationDialog();
            }
        }

        private static void ExpireDemo()
        {
            if (mainForm == null || mainForm.IsDisposed) return;

            settings.IsDemoExpired = true;
            SaveSettings();

            // Блокируем все окна
            mainForm.BeginInvoke((Action)(() =>
            {
                var result = MessageBox.Show(
                    "⏰ ДЕМО-РЕЖИМ ЗАВЕРШЁН!\n\n" +
                    "Время работы в демо-режиме истекло.\n" +
                    "Для продолжения работы необходимо активировать систему.\n\n" +
                    "💡 Тестовый код: DEMO-2024\n" +
                    "📝 Введите код активации для продолжения.\n\n" +
                    "Нажмите OK для ввода кода активации.",
                    "Демо-режим завершён",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.OK)
                {
                    // Показываем окно активации
                    mainForm.ShowActivationDialog();
                }
                else
                {
                    // Если пользователь закрыл окно, завершаем приложение
                    Application.Exit();
                }
            }));
        }

        public static void StopDemoTimers()
        {
            if (demoWarningTimer != null)
            {
                demoWarningTimer.Stop();
                demoWarningTimer.Dispose();
                demoWarningTimer = null;
            }

            if (demoExpireTimer != null)
            {
                demoExpireTimer.Stop();
                demoExpireTimer.Dispose();
                demoExpireTimer = null;
            }
        }

        public static string GetDemoTimeRemaining()
        {
            if (!settings.IsDemoMode || settings.IsDemoExpired)
                return "0 мин";

            var elapsed = DateTime.Now - settings.DemoStartTime;
            var remaining = TimeSpan.FromMinutes(settings.DemoMinutesLimit) - elapsed;

            if (remaining.TotalSeconds <= 0)
                return "0 мин";

            return $"{(int)remaining.TotalMinutes} мин {(int)remaining.Seconds} сек";
        }

        private static List<AccountData> LoadAccounts()
        {
            if (!File.Exists(AccountsFile))
                return new List<AccountData>();

            try
            {
                string json = File.ReadAllText(AccountsFile);
                return JsonSerializer.Deserialize<List<AccountData>>(json) ?? new List<AccountData>();
            }
            catch
            {
                return new List<AccountData>();
            }
        }

        private static void SaveAccounts(List<AccountData> accounts)
        {
            try
            {
                string json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(AccountsFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения аккаунтов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void LoadSettings()
        {
            if (!File.Exists(SettingsFile))
            {
                settings = new AppSettings();
                SaveSettings();
                return;
            }

            try
            {
                string json = File.ReadAllText(SettingsFile);
                settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                settings = new AppSettings();
            }
        }

        private static void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            }
            catch { }
        }
    }
}