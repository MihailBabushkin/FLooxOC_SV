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
            "MyOS95", "Accounts"
        );
        private static string AccountsFile = Path.Combine(DataPath, "accounts.json");
        private static string SettingsFile = Path.Combine(DataPath, "settings.json");

        // Сервер для проверки кодов активации (замени на свой)
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
            public string UserAvatar { get; set; } // base64
            public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        }

        public class AppSettings
        {
            public bool RequirePassword { get; set; } = true;
            public bool AutoLogin { get; set; } = false;
            public string LastUser { get; set; } = "";
            public bool FirstRun { get; set; } = true;
            public string ActivationCode { get; set; } = "";
        }

        private static AppSettings settings = new AppSettings();
        private static AccountData currentUser = null;

        static AccountManager()
        {
            Directory.CreateDirectory(DataPath);
            LoadSettings();
        }

        // ====== АКТИВАЦИЯ ======
        public static async Task<bool> ActivateCode(string code)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var data = new { code = code };
                    string json = JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(ACTIVATION_SERVER, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(result);

                        if (responseData != null && responseData.ContainsKey("success") && (bool)responseData["success"])
                        {
                            settings.ActivationCode = code;
                            settings.FirstRun = false;
                            SaveSettings();
                            return true;
                        }
                    }
                }

                // === ЛОКАЛЬНАЯ ПРОВЕРКА (для тестирования) ===
                // Если сервер недоступен, используем локальную проверку
                if (ValidateCodeLocally(code))
                {
                    settings.ActivationCode = code;
                    settings.FirstRun = false;
                    SaveSettings();
                    return true;
                }

                return false;
            }
            catch
            {
                // При ошибке соединения используем локальную проверку
                if (ValidateCodeLocally(code))
                {
                    settings.ActivationCode = code;
                    settings.FirstRun = false;
                    SaveSettings();
                    return true;
                }
                return false;
            }
        }

        // ====== РЕГИСТРАЦИЯ ======
        public static bool RegisterUser(string login, string password, string userName = "")
        {
            if (string.IsNullOrEmpty(login) || login.Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверяем, существует ли пользователь
            var accounts = LoadAccounts();
            if (accounts.Exists(a => a.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Если пароль включён, проверяем его
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
                IsActivated = true,
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

        // ====== ВХОД ======
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

        // ====== ВЫХОД ======
        public static void Logout()
        {
            currentUser = null;
        }

        // ====== ПОЛУЧЕНИЕ ТЕКУЩЕГО ПОЛЬЗОВАТЕЛЯ ======
        public static AccountData GetCurrentUser()
        {
            return currentUser;
        }

        public static bool IsLoggedIn()
        {
            return currentUser != null;
        }

        // ====== ПРОВЕРКА ПЕРВОГО ЗАПУСКА ======
        public static bool IsFirstRun()
        {
            return settings.FirstRun;
        }

        public static bool IsActivated()
        {
            return !string.IsNullOrEmpty(settings.ActivationCode);
        }

        // ====== НАСТРОЙКИ ======
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

        // ====== ХЭШИРОВАНИЕ ПАРОЛЯ ======
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // ====== ЗАГРУЗКА/СОХРАНЕНИЕ АККАУНТОВ ======
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

        // ====== ЗАГРУЗКА/СОХРАНЕНИЕ НАСТРОЕК ======
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

        // ====== ВАЛИДАЦИЯ КОДА (ЛОКАЛЬНАЯ) ======
        public static bool ValidateCodeLocally(string code)
        {
            // В реальном проекте здесь будет запрос к серверу
            // Сейчас это для тестирования
            string[] validCodes = { "DEMO-2024", "TEST-1234", "FREE-2024" };
            foreach (var valid in validCodes)
            {
                if (code == valid)
                    return true;
            }
            return false;
        }
    }
}