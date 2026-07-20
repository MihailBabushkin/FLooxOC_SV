using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlooxOC
{
    public class MySQLManager
    {
        // ===== АДРЕС ВАШЕГО САЙТА (PUNYCODE) =====
        private const string API_URL = "http://xn----7sbqlbbht1aldw8d9eh.xn--p1ai/api.php";

        private static readonly HttpClient client;

        // ===== КЛАССЫ =====
        public class ActivationCode
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExpiresAt { get; set; }
        }

        public class ActivatedUser
        {
            public int Id { get; set; }
            public string Login { get; set; }
            public string ActivationCode { get; set; }
            public DateTime ActivatedAt { get; set; }
            public string IpAddress { get; set; }
            public string UserAgent { get; set; }
        }

        public class CheckResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public ActivationCode CodeInfo { get; set; }
            public int FreeCodes { get; set; }
            public int ActivatedUsers { get; set; }
            public int TotalCodes { get; set; }
            public bool UpdateSuccess { get; set; }
        }

        private class ApiResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
            public int free { get; set; }
            public int activated { get; set; }
            public int total { get; set; }
        }

        // ===== СТАТИЧЕСКИЙ КОНСТРУКТОР =====
        static MySQLManager()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls;

            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("User-Agent", "FlooxOC/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // ====== ПРОВЕРКА ПОДКЛЮЧЕНИЯ ======
        public static async Task<bool> TestConnection()
        {
            try
            {
                var data = new { action = "stats" };
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ====== ПРОВЕРКА СВЯЗИ ======
        public static async Task<CheckResult> CheckConnection()
        {
            var result = new CheckResult();

            try
            {
                bool connected = await TestConnection();
                if (!connected)
                {
                    result.Message = "❌ Не удалось подключиться к серверу!";
                    return result;
                }

                var data = new { action = "stats" };
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseJson);

                    if (apiResponse != null && apiResponse.success)
                    {
                        result.Success = true;
                        result.FreeCodes = apiResponse.free;
                        result.ActivatedUsers = apiResponse.activated;
                        result.TotalCodes = apiResponse.total;
                        result.Message = "✅ Связь с сервером установлена!";
                    }
                    else
                    {
                        result.Message = "❌ Ошибка получения данных с сервера";
                    }
                }
                else
                {
                    result.Message = $"❌ Ошибка сервера (HTTP {response.StatusCode})";
                }
            }
            catch (Exception ex)
            {
                result.Message = $"❌ Ошибка подключения: {ex.Message}";
            }

            return result;
        }

        // ====== ПРОВЕРКА КОДА ======
        public static async Task<CheckResult> ValidateCode(string code)
        {
            var result = new CheckResult();

            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    result.Message = "Введите код активации!";
                    return result;
                }

                var data = new
                {
                    action = "check",
                    code = code
                };

                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseJson);

                    if (apiResponse != null)
                    {
                        if (apiResponse.success)
                        {
                            result.Success = true;
                            result.Message = "✅ " + apiResponse.message;
                        }
                        else
                        {
                            result.Message = "❌ " + apiResponse.message;
                        }
                    }
                    else
                    {
                        result.Message = "❌ Не удалось обработать ответ сервера";
                    }
                }
                else
                {
                    result.Message = $"❌ Ошибка сервера (HTTP {response.StatusCode})";
                }
            }
            catch (Exception ex)
            {
                result.Message = $"❌ Ошибка: {ex.Message}";
            }

            return result;
        }

        // ====== АКТИВАЦИЯ КОДА ======
        public static async Task<bool> ActivateCode(string code, string username)
        {
            try
            {
                var data = new
                {
                    action = "activate",
                    code = code,
                    username = username
                };

                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseJson);

                    if (apiResponse != null && apiResponse.success)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ActivateCode error: {ex.Message}");
            }

            return false;
        }

        // ====== ПРОВЕРКА, АКТИВИРОВАН ЛИ ПОЛЬЗОВАТЕЛЬ ======
        public static async Task<bool> IsUserActivated(string login)
        {
            try
            {
                var data = new
                {
                    action = "is_activated",
                    login = login
                };

                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(responseJson))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("success", out var success) && success.GetBoolean())
                        {
                            if (root.TryGetProperty("is_activated", out var activated))
                            {
                                return activated.GetBoolean();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsUserActivated error: {ex.Message}");
            }

            return false;
        }

        // ====== СТАТИСТИКА ======
        public static async Task<string> GetStatistics()
        {
            try
            {
                var data = new { action = "stats" };
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseJson);

                    if (apiResponse != null && apiResponse.success)
                    {
                        return $"📊 Статистика:\n" +
                               $"   🆓 Свободных кодов: {apiResponse.free}\n" +
                               $"   ✅ Активировано: {apiResponse.activated}\n" +
                               $"   📦 Всего создано: {apiResponse.total}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetStatistics error: {ex.Message}");
            }

            return "❌ Не удалось получить статистику";
        }

        // ====== ПОЛУЧЕНИЕ ВСЕХ КОДОВ ======
        public static async Task<List<ActivationCode>> GetCodes()
        {
            var codes = new List<ActivationCode>();

            try
            {
                var data = new { action = "list_codes" };
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();

                    try
                    {
                        using (var doc = JsonDocument.Parse(responseJson))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
                            {
                                if (root.TryGetProperty("codes", out var codesArray))
                                {
                                    foreach (var item in codesArray.EnumerateArray())
                                    {
                                        var code = new ActivationCode();

                                        if (item.TryGetProperty("id", out var idProp))
                                            code.Id = idProp.GetInt32();

                                        if (item.TryGetProperty("code", out var codeProp))
                                            code.Code = codeProp.GetString();

                                        if (item.TryGetProperty("created_at", out var createdAtProp))
                                            code.CreatedAt = DateTime.Parse(createdAtProp.GetString());

                                        if (item.TryGetProperty("expires_at", out var expiresAtProp) && !string.IsNullOrEmpty(expiresAtProp.GetString()))
                                            code.ExpiresAt = DateTime.Parse(expiresAtProp.GetString());

                                        codes.Add(code);
                                    }
                                }
                            }
                        }
                    }
                    catch (JsonException je)
                    {
                        Console.WriteLine($"JSON parse error: {je.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCodes error: {ex.Message}");
            }

            return codes;
        }

        // ====== ПОЛУЧЕНИЕ АКТИВИРОВАННЫХ ПОЛЬЗОВАТЕЛЕЙ ======
        public static async Task<List<ActivatedUser>> GetActivatedUsers()
        {
            var users = new List<ActivatedUser>();

            try
            {
                var data = new { action = "list_activated" };
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();

                    try
                    {
                        using (var doc = JsonDocument.Parse(responseJson))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
                            {
                                if (root.TryGetProperty("users", out var usersArray))
                                {
                                    foreach (var item in usersArray.EnumerateArray())
                                    {
                                        var user = new ActivatedUser();

                                        if (item.TryGetProperty("id", out var idProp))
                                            user.Id = idProp.GetInt32();

                                        if (item.TryGetProperty("login", out var loginProp))
                                            user.Login = loginProp.GetString();

                                        if (item.TryGetProperty("activation_code", out var codeProp))
                                            user.ActivationCode = codeProp.GetString();

                                        if (item.TryGetProperty("activated_at", out var activatedAtProp))
                                            user.ActivatedAt = DateTime.Parse(activatedAtProp.GetString());

                                        if (item.TryGetProperty("ip_address", out var ipProp))
                                            user.IpAddress = ipProp.GetString();

                                        if (item.TryGetProperty("user_agent", out var uaProp))
                                            user.UserAgent = uaProp.GetString();

                                        users.Add(user);
                                    }
                                }
                            }
                        }
                    }
                    catch (JsonException je)
                    {
                        Console.WriteLine($"JSON parse error: {je.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetActivatedUsers error: {ex.Message}");
            }

            return users;
        }
    }
}