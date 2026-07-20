using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlooxOC
{
    public class MySQLManager
    {
        // ===== АДРЕС ВАШЕГО САЙТА =====
        private const string API_URL = "http://шуйская-империя.рф/api.php";
        // ИЛИ используйте IP: private const string API_URL = "http://37.140.192.155/api.php";

        private static readonly HttpClient client = new HttpClient();

        // ===== КЛАССЫ =====
        public class ActivationCode
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public bool IsUsed { get; set; }
            public string UsedBy { get; set; }
            public DateTime? UsedAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExpiresAt { get; set; }
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

        private class ApiResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
            public int total { get; set; }
            public int used { get; set; }
            public int free { get; set; }
        }

        static MySQLManager()
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
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

        // ====== АКТИВАЦИЯ КОДА (УДАЛЕНИЕ ИЗ БД) ======
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
                        return $"📊 Статистика кодов:\n   Всего: {apiResponse.total}\n   Использовано: {apiResponse.used}\n   Свободно: {apiResponse.free}";
                    }
                }
            }
            catch { }

            return "❌ Не удалось получить статистику";
        }
    }
}