using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;

namespace FlooxOC
{
    public class GoogleSheetsManager
    {
        // ===== ПУБЛИЧНАЯ ССЫЛКА (CSV экспорт) =====
        private const string PUBLISHED_URL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQ0jAKRnpakTKSkRqDhvK94FGVrU3wLP9GqGw5hcdrsZJccWD5Y-el7jMc4r2dMHZoBmItfMASlcBTp/pub?gid=0&single=true&output=csv";

        private static readonly HttpClient client = new HttpClient();
        private static List<ActivationCode> cachedCodes = null;
        private static DateTime cacheTime = DateTime.MinValue;
        private static readonly TimeSpan cacheLifetime = TimeSpan.FromMinutes(5);
        private static string lastError = "";

        // ===== КЛАСС ДЛЯ ХРАНЕНИЯ КОДА =====
        public class ActivationCode
        {
            public string Code { get; set; }
            public bool IsUsed { get; set; }
            public string UserName { get; set; }

            public ActivationCode(string code, bool isUsed, string userName = "")
            {
                Code = code;
                IsUsed = isUsed;
                UserName = userName ?? "";
            }
        }

        // ===== РЕЗУЛЬТАТ ПРОВЕРКИ =====
        public class CheckResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public ActivationCode CodeInfo { get; set; }
            public int TotalCodes { get; set; }
            public int UsedCodes { get; set; }
            public int FreeCodes { get; set; }
        }

        static GoogleSheetsManager()
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            client.Timeout = TimeSpan.FromSeconds(15);
        }

        // ====== ПОЛУЧЕНИЕ КОДОВ ИЗ CSV ======
        public static async Task<List<ActivationCode>> GetCodes()
        {
            try
            {
                var response = await client.GetAsync(PUBLISHED_URL);

                if (response.IsSuccessStatusCode)
                {
                    string csv = await response.Content.ReadAsStringAsync();
                    var codes = new List<ActivationCode>();

                    // Разбиваем на строки
                    string[] lines = csv.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    bool isFirstRow = true;

                    foreach (string line in lines)
                    {
                        // Пропускаем заголовки
                        if (isFirstRow)
                        {
                            isFirstRow = false;
                            // Проверяем, есть ли заголовки
                            if (line.Contains("Код") || line.Contains("code") || line.Contains("использован"))
                                continue;
                        }

                        string[] parts = line.Split(',');

                        // Ожидаем минимум 2 колонки: код и статус
                        if (parts.Length >= 2)
                        {
                            string code = parts[0].Trim().Replace("\"", "");
                            string status = parts.Length > 1 ? parts[1].Trim().Replace("\"", "").ToUpper() : "FALSE";
                            string userName = parts.Length > 2 ? parts[2].Trim().Replace("\"", "") : "";

                            // Проверяем статус: FALSE или пусто = свободен
                            bool isUsed = (status != "FALSE" && !string.IsNullOrEmpty(status));

                            // Пропускаем пустые коды
                            if (!string.IsNullOrEmpty(code))
                            {
                                codes.Add(new ActivationCode(code, isUsed, userName));
                            }
                        }
                    }

                    Console.WriteLine($"[DEBUG] Найдено кодов: {codes.Count} (использовано: {codes.FindAll(c => c.IsUsed).Count}, свободно: {codes.FindAll(c => !c.IsUsed).Count})");
                    return codes;
                }
                else
                {
                    lastError = $"HTTP Error: {response.StatusCode}";
                    Console.WriteLine($"[DEBUG] Ошибка загрузки: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException)
            {
                lastError = "Превышено время ожидания (15 сек)";
                Console.WriteLine("[DEBUG] Таймаут при загрузке");
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                Console.WriteLine($"[DEBUG] Ошибка: {ex.Message}");
            }

            return new List<ActivationCode>();
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
                FreeCodes = 0
            };

            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    result.Message = "Введите код активации!";
                    return result;
                }

                // Обновляем кэш если нужно
                if (cachedCodes == null || DateTime.Now - cacheTime > cacheLifetime)
                {
                    cachedCodes = await GetCodes();
                    cacheTime = DateTime.Now;
                }

                result.TotalCodes = cachedCodes.Count;
                result.UsedCodes = cachedCodes.FindAll(c => c.IsUsed).Count;
                result.FreeCodes = cachedCodes.FindAll(c => !c.IsUsed).Count;

                // Ищем код
                var foundCode = cachedCodes.Find(c => c.Code == code);

                if (foundCode == null)
                {
                    result.Message = "❌ Код не найден в базе!";
                    return result;
                }

                if (foundCode.IsUsed)
                {
                    result.Message = $"❌ Код уже использован!\nПользователь: {foundCode.UserName}";
                    result.CodeInfo = foundCode;
                    return result;
                }

                // Код найден и свободен
                result.Success = true;
                result.Message = "✅ Код активирован!";
                result.CodeInfo = foundCode;

                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"❌ Ошибка проверки: {ex.Message}";
                return result;
            }
        }

        // ====== ПРОВЕРКА СВЯЗИ С ТАБЛИЦЕЙ ======
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
                // Пытаемся получить данные
                var codes = await GetCodes();

                if (codes.Count > 0)
                {
                    result.Success = true;
                    result.TotalCodes = codes.Count;
                    result.UsedCodes = codes.FindAll(c => c.IsUsed).Count;
                    result.FreeCodes = codes.FindAll(c => !c.IsUsed).Count;
                    result.Message = $"✅ Связь установлена! Найдено {codes.Count} кодов";

                    // Обновляем кэш
                    cachedCodes = codes;
                    cacheTime = DateTime.Now;
                }
                else
                {
                    result.Message = "❌ Таблица пуста или не содержит данных!\n" +
                        "Убедитесь, что таблица опубликована как CSV.";
                }
            }
            catch (Exception ex)
            {
                result.Message = $"❌ Ошибка подключения: {ex.Message}";
            }

            return result;
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

                return $"📊 Статистика кодов:\n" +
                       $"   Всего: {total}\n" +
                       $"   Использовано: {used}\n" +
                       $"   Свободно: {free}\n" +
                       $"   Процент использования: {(total > 0 ? (used * 100 / total) : 0)}%";
            }
            catch
            {
                return "❌ Не удалось получить статистику";
            }
        }

        // ====== ПРИНУДИТЕЛЬНОЕ ОБНОВЛЕНИЕ КЭША ======
        public static async Task RefreshCache()
        {
            cachedCodes = await GetCodes();
            cacheTime = DateTime.Now;
        }

        // ====== ПОЛУЧЕНИЕ ПОСЛЕДНЕЙ ОШИБКИ ======
        public static string GetLastError()
        {
            return lastError;
        }
    }
}