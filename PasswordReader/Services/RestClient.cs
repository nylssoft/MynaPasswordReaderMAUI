/*
    Myna Password Reader MAUI
    Copyright (C) 2022 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace PasswordReader.Services
{
    public class RestClient
    {
        private static HttpClient httpClient = null;

        private static Dictionary<string, string> translateMap = null;

        private const string ISO8601_DATEFORMAT = "yyyy-MM-dd'T'HH:mm:ss.fffK";

        public static async Task InitAsync(string cloudUrl, string locale)
        {
            if (httpClient == null || httpClient.BaseAddress != new Uri(cloudUrl))
            {
                httpClient = new HttpClient
                {
                    BaseAddress = new Uri(cloudUrl)
                };
            }
            if (!string.IsNullOrEmpty(locale))
            {
                var localeUrl = await httpClient.GetFromJsonAsync<string>($"api/pwdman/locale/url/{locale}");
                translateMap = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(localeUrl);
            }
        }

        public static string Translate(string symbol)
        {
            if (translateMap != null)
            {
                var arr = symbol.Split(':');
                if (arr.Length > 1)
                {
                    if (translateMap.TryGetValue(arr[0], out var fmt))
                    {
                        for (int idx = 1; idx < arr.Length; idx++)
                        {
                            fmt = fmt.Replace($"{{{idx - 1}}}", arr[idx]);
                        }
                        return fmt;
                    }
                }
                else if (translateMap.TryGetValue(symbol, out var txt))
                {
                    return txt;
                }
            }
            return symbol;
        }

        public static async Task<AuthenticationResult> AuthenticateAsync(
            string username, string password, ClientInfo clientInfo, string locale)
        {
            var url = "api/pwdman/auth";
            if (!string.IsNullOrEmpty(locale))
            {
                url += $"?locale={locale}";
            }
            var response = await httpClient.PostAsJsonAsync(url, new AuthenticationModel
            {
                Username = username,
                Password = password,
                ClientUUID = clientInfo?.UUID,
                ClientName = clientInfo?.Name
            });
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        }

        public static async Task<UserModel> GetUserAsync(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.GetAsync("api/pwdman/user");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }

        public static async Task<AuthenticationResult> AuthenticateLLTokenAsync(string lltoken)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", lltoken);
            var response = await httpClient.GetAsync("api/pwdman/auth/lltoken");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        }

        public static async Task<AuthenticationResult> AuthenticatePass2Async(string token, string totp)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.PostAsJsonAsync("api/pwdman/auth2", totp);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        }

        public static async Task<bool> LogoutAsync(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.GetAsync("api/pwdman/logout");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public static async Task<string> GetPasswordFileAsync(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.GetAsync("api/pwdman/file");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<string>();
        }

        public static async Task<List<Note>> GetNotesAsync(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.GetAsync("api/notes/note");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<List<Note>>();
        }

        public static async Task<Note> GetNoteAsync(string token, long id)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.GetAsync($"api/notes/note/{id}");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<Note>();
        }

        public static async Task<long> CreateNewNoteAsync(string token, string title)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.PostAsJsonAsync("api/notes/note",
                new {
                    Title = title
                });
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<long>();
        }

        public static async Task DeleteNoteAsync(string token, long id)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.DeleteAsync($"api/notes/note/{id}");
            await EnsureSuccessAsync(response);
        }

        public static async Task<DateTime> UpdateNoteAsync(string token, long id, string title, string content)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.PutAsJsonAsync("api/notes/note",
                new {
                    Id = id,
                    Title = title, 
                    Content = content});
            await EnsureSuccessAsync(response);
            var lastModifiedUtc = await response.Content.ReadFromJsonAsync<DateTime>();
            return lastModifiedUtc;
        }

        public static async Task<List<int>> GetDiaryDaysAsync(string token, int year, int month)
        {
            var dt = new DateTime(year, month, 1);
            var iso8601 = dt.ToString(ISO8601_DATEFORMAT, CultureInfo.InvariantCulture);
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.GetAsync($"api/diary/day?date={iso8601}");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<List<int>>();
        }

        public static async Task<Diary> GetDiaryAsync(string token, int year, int month, int day)
        {
            var dt = new DateTime(year, month, day);
            var iso8601 = dt.ToString(ISO8601_DATEFORMAT, CultureInfo.InvariantCulture);
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.GetAsync($"api/diary/entry?date={iso8601}");
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<Diary>();
        }

        public static async Task SaveDiaryAsync(string token, int year, int month, int day, string entry)
        {
            var dt = new DateTime(year, month, day);
            var iso8601 = dt.ToString(ISO8601_DATEFORMAT, CultureInfo.InvariantCulture);
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.PostAsJsonAsync("api/diary/entry",
                new
                {
                    Date = iso8601,
                    Entry = entry
                });
            await EnsureSuccessAsync(response);
        }

        public static async Task UploadPasswordFileAsync(string token, string content)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.PostAsync("api/pwdman/file", new StringContent(content, Encoding.UTF8, "application/json"));
            await EnsureSuccessAsync(response);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                var message = Translate(problemDetails.title);
                if (problemDetails.title == "ERROR_INVALID_TOKEN")
                {
                    throw new InvalidTokenException(message);
                }
                throw new ArgumentException(message);
            }
        }
    }
}
