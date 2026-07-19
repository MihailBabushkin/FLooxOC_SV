using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace FlooxOC
{
    public class MySQLManager
    {
        // ===== НАСТРОЙКИ ПОДКЛЮЧЕНИЯ =====
        // ЗАМЕНИТЕ ЭТИ ДАННЫЕ НА ВАШИ!
        private const string SERVER = "localhost";
        private const string DATABASE = "floox_oc";
        private const string USER = "root";
        private const string PASSWORD = "415454654544651021502004527400100651075060474065412042051074520101234562102120065604522635122142524632655858789798787895348655441";
        private const int PORT = 3306;

        private static string ConnectionString = $"Server={SERVER};Port={PORT};Database={DATABASE};Uid={USER};Pwd={PASSWORD};CharSet=utf8;";

        // ===== КЭШ =====
        private static List<ActivationCode> cachedCodes = null;
        private static DateTime cacheTime = DateTime.MinValue;
        private static readonly TimeSpan cacheLifetime = TimeSpan.FromMinutes(5);

        // ===== КЛАССЫ =====
        public class ActivationCode
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public bool IsUsed { get; set; }
            public string UsedBy { get; set; }
            public DateTime? UsedAt { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class CheckResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public ActivationCode CodeInfo { get; set; }
            public int TotalCodes { get; set; }
            public int UsedCodes { get; set; }
            public int FreeCodes { get; set; }
            public bool UpdateSuccess { get; set; }
        }

        // ====== ПРОВЕРКА ПОДКЛЮЧЕНИЯ ======
        public static async Task<bool> TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к MySQL:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ====== ПОЛУЧЕНИЕ ВСЕХ КОДОВ ======
        public static async Task<List<ActivationCode>> GetCodes()
        {
            var codes = new List<ActivationCode>();

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();

                    string query = "SELECT id, code, is_used, used_by, used_at, created_at FROM activation_codes";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            codes.Add(new ActivationCode
                            {
                                Id = reader.GetInt32(0),
                                Code = reader.GetString(1),
                                IsUsed = reader.GetBoolean(2),
                                UsedBy = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                UsedAt = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                CreatedAt = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCodes error: {ex.Message}");
            }

            return codes;
        }

        // ====== ПРОВЕРКА КОДА ======
        public static async Task<CheckResult> ValidateCode(string code)
        {
            var result = new CheckResult
            {
                Success = false,
                Message = "",
                TotalCodes = 0,
                UsedCodes = 0,
                FreeCodes = 0,
                UpdateSuccess = false
            };

            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    result.Message = "Введите код активации!";
                    return result;
                }

                if (cachedCodes == null || DateTime.Now - cacheTime > cacheLifetime)
                {
                    cachedCodes = await GetCodes();
                    cacheTime = DateTime.Now;
                }

                result.TotalCodes = cachedCodes.Count;
                result.UsedCodes = cachedCodes.FindAll(c => c.IsUsed).Count;
                result.FreeCodes = cachedCodes.FindAll(c => !c.IsUsed).Count;

                var foundCode = cachedCodes.Find(c => c.Code == code);

                if (foundCode == null)
                {
                    result.Message = "❌ Код не найден в базе!";
                    return result;
                }

                if (foundCode.IsUsed)
                {
                    result.Message = $"❌ Код уже использован!\nПользователь: {foundCode.UsedBy}";
                    result.CodeInfo = foundCode;
                    return result;
                }

                bool updateSuccess = await MarkCodeAsUsed(code, Environment.UserName);

                result.UpdateSuccess = updateSuccess;

                if (updateSuccess)
                {
                    foundCode.IsUsed = true;
                    foundCode.UsedBy = Environment.UserName;
                    foundCode.UsedAt = DateTime.Now;

                    result.Success = true;
                    result.Message = "✅ Код активирован!";
                    result.CodeInfo = foundCode;

                    cachedCodes = null;
                }
                else
                {
                    result.Message = "❌ Не удалось активировать код в базе данных!";
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"❌ Ошибка проверки: {ex.Message}";
                return result;
            }
        }

        // ====== ОТМЕТКА КОДА КАК ИСПОЛЬЗОВАННОГО ======
        public static async Task<bool> MarkCodeAsUsed(string code, string userName)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();

                    string query = @"UPDATE activation_codes 
                                    SET is_used = TRUE, 
                                        used_by = @userName, 
                                        used_at = @usedAt 
                                    WHERE code = @code AND is_used = FALSE";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userName", userName);
                        cmd.Parameters.AddWithValue("@usedAt", DateTime.Now);
                        cmd.Parameters.AddWithValue("@code", code);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            await LogActivation(code, userName, "activate");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MarkCodeAsUsed error: {ex.Message}");
            }

            return false;
        }

        // ====== ЛОГИРОВАНИЕ ======
        public static async Task LogActivation(string code, string userLogin, string action)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();

                    string query = @"INSERT INTO activation_logs (code, user_login, action, ip_address) 
                                    VALUES (@code, @userLogin, @action, @ip)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", code);
                        cmd.Parameters.AddWithValue("@userLogin", userLogin);
                        cmd.Parameters.AddWithValue("@action", action);
                        cmd.Parameters.AddWithValue("@ip", GetLocalIPAddress());

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LogActivation error: {ex.Message}");
            }
        }

        // ====== ПОЛУЧЕНИЕ IP АДРЕСА ======
        private static string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        // ====== ПОЛУЧЕНИЕ СТАТИСТИКИ ======
        public static async Task<string> GetStatistics()
        {
            try
            {
                var codes = await GetCodes();

                if (codes.Count == 0)
                    return "Нет данных о кодах";

                int total = codes.Count;
                int used = codes.FindAll(c => c.IsUsed).Count;
                int free = codes.FindAll(c => !c.IsUsed).Count;

                string lastActivations = "";
                try
                {
                    using (var conn = new MySqlConnection(ConnectionString))
                    {
                        await conn.OpenAsync();
                        string query = @"SELECT code, user_login, created_at FROM activation_logs 
                                        WHERE action = 'activate' 
                                        ORDER BY created_at DESC LIMIT 5";

                        using (var cmd = new MySqlCommand(query, conn))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                lastActivations = "\n\n📋 Последние активации:";
                                int count = 0;
                                while (await reader.ReadAsync() && count < 5)
                                {
                                    lastActivations += $"\n   {reader.GetString(0)} - {reader.GetString(1)} ({reader.GetDateTime(2):dd.MM.yyyy HH:mm})";
                                    count++;
                                }
                            }
                        }
                    }
                }
                catch { }

                return $"📊 Статистика кодов:\n" +
                       $"   Всего: {total}\n" +
                       $"   Использовано: {used}\n" +
                       $"   Свободно: {free}\n" +
                       $"   Процент использования: {(total > 0 ? (used * 100 / total) : 0)}%" +
                       lastActivations;
            }
            catch
            {
                return "❌ Не удалось получить статистику";
            }
        }

        // ====== ПОЛУЧЕНИЕ СВОБОДНЫХ КОДОВ ======
        public static async Task<List<string>> GetFreeCodes()
        {
            try
            {
                var codes = await GetCodes();
                var freeCodes = codes.FindAll(c => !c.IsUsed);
                List<string> result = new List<string>();
                foreach (var code in freeCodes)
                {
                    result.Add(code.Code);
                }
                return result;
            }
            catch
            {
                return new List<string>();
            }
        }

        // ====== ОБНОВЛЕНИЕ КЭША ======
        public static async Task RefreshCache()
        {
            cachedCodes = await GetCodes();
            cacheTime = DateTime.Now;
        }

        // ====== ПРОВЕРКА СВЯЗИ ======
        public static async Task<CheckResult> CheckConnection()
        {
            var result = new CheckResult
            {
                Success = false,
                Message = "",
                TotalCodes = 0,
                UsedCodes = 0,
                FreeCodes = 0
            };

            try
            {
                bool connected = await TestConnection();
                if (!connected)
                {
                    result.Message = "❌ Не удалось подключиться к MySQL серверу!";
                    return result;
                }

                var codes = await GetCodes();

                if (codes.Count > 0)
                {
                    result.Success = true;
                    result.TotalCodes = codes.Count;
                    result.UsedCodes = codes.FindAll(c => c.IsUsed).Count;
                    result.FreeCodes = codes.FindAll(c => !c.IsUsed).Count;
                    result.Message = $"✅ Связь с MySQL установлена! Найдено {codes.Count} кодов";

                    cachedCodes = codes;
                    cacheTime = DateTime.Now;
                }
                else
                {
                    result.Message = "❌ Таблица activation_codes пуста!";
                }
            }
            catch (Exception ex)
            {
                result.Message = $"❌ Ошибка подключения: {ex.Message}";
            }

            return result;
        }
    }
}