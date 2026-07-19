using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlooxOC
{
    public class GoogleSheetsManager
    {
        // ===== ТВОЯ ПУБЛИЧНАЯ ССЫЛКА =====
        private const string PUBLISHED_URL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQ0jAKRnpakTKSkRqDhvK94FGVrU3wLP9GqGw5hcdrsZJccWD5Y-el7jMc4r2dMHZoBmItfMASlcBTp/pubhtml?gid=0&single=true";

        private static readonly HttpClient client = new HttpClient();
        private static List<string> cachedCodes = null;
        private static DateTime cacheTime = DateTime.MinValue;
        private static readonly TimeSpan cacheLifetime = TimeSpan.FromMinutes(5);

        // ====== ПОЛУЧЕНИЕ КОДОВ ИЗ ПУБЛИЧНОЙ ТАБЛИЦЫ ======
        public static async Task<List<string>> GetCodes()
        {
            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var response = await client.GetAsync(PUBLISHED_URL);

                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync();

                    // Парсим HTML таблицы
                    var codes = new List<string>();

                    // Ищем все строки таблицы
                    string pattern = @"<td[^>]*>([^<]*)</td>";
                    var matches = Regex.Matches(html, pattern);

                    bool isFirstRow = true;
                    string code = "";
                    bool isCodeColumn = true;
                    int codeCount = 0;

                    foreach (Match match in matches)
                    {
                        string cellValue = match.Groups[1].Value.Trim();

                        // Пропускаем пустые ячейки
                        if (string.IsNullOrEmpty(cellValue))
                            continue;

                        // Пропускаем заголовки (первая строка)
                        if (isFirstRow)
                        {
                            isFirstRow = false;
                            continue;
                        }

                        // Чётные ячейки — коды, нечётные — статус
                        if (isCodeColumn)
                        {
                            code = cellValue;
                            isCodeColumn = false;
                        }
                        else
                        {
                            string status = cellValue.ToUpper();
                            isCodeColumn = true;

                            // Если статус FALSE или пустой — код активен
                            if (status == "FALSE" || string.IsNullOrEmpty(status))
                            {
                                if (!string.IsNullOrEmpty(code) && !codes.Contains(code))
                                {
                                    codes.Add(code);
                                    codeCount++;
                                }
                            }
                            code = "";
                        }
                    }

                    Console.WriteLine($"[DEBUG] Найдено активных кодов: {codeCount}");
                    return codes;
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Ошибка загрузки: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка подключения к Google Sheets:\n{ex.Message}\n\n" +
                    "Проверьте интернет-соединение.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Если не удалось получить с Google, используем локальные коды (для теста)
            return new List<string> { "DEMO-2024", "TEST-1234", "FREE-2024" };
        }

        // ====== ПРОВЕРКА КОДА (С КЭШИРОВАНИЕМ) ======
        public static async Task<bool> ValidateCode(string code)
        {
            try
            {
                // Если кэш устарел или пуст — обновляем
                if (cachedCodes == null || DateTime.Now - cacheTime > cacheLifetime)
                {
                    cachedCodes = await GetCodes();
                    cacheTime = DateTime.Now;
                }

                return cachedCodes.Contains(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки кода: {ex.Message}");
                return false;
            }
        }

        // ====== ПРИНУДИТЕЛЬНОЕ ОБНОВЛЕНИЕ КЭША ======
        public static async Task RefreshCache()
        {
            cachedCodes = await GetCodes();
            cacheTime = DateTime.Now;
        }
    }
}