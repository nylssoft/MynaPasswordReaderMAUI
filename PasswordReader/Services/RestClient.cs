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
using System.Net.Http.Json;

namespace PasswordReader.Services
{
    public class RestClient
    {
        private static HttpClient httpClient = null;

        private static Dictionary<string, string> translateMap = null;

        public static async Task Init(string cloudUrl, string locale)
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

        public static async Task<AuthenticationResult> Authenticate(
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
            await EnsureSuccess(response);
            return await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        }

        public static async Task<UserModel> GetUser(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            return await httpClient.GetFromJsonAsync<UserModel>("api/pwdman/user");
        }

        public static async Task<AuthenticationResult> AuthenticateLLToken(string lltoken)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", lltoken);
            return await httpClient.GetFromJsonAsync<AuthenticationResult>("api/pwdman/auth/lltoken");
        }

        public static async Task<AuthenticationResult> AuthenticatePass2(string token, string totp)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            var response = await httpClient.PostAsJsonAsync("api/pwdman/auth2", totp);
            await EnsureSuccess(response);
            return await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        }

        public static async Task<string> GetPasswordFile(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            return await httpClient.GetFromJsonAsync<string>("api/pwdman/file");
        }

        public static async Task<List<Note>> GetNotes(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            return await httpClient.GetFromJsonAsync<List<Note>>("api/notes/note");
        }

        public static async Task<Note> GetNote(string token, long id)
        {
            httpClient.DefaultRequestHeaders.Remove("token");
            httpClient.DefaultRequestHeaders.Add("token", token);
            return await httpClient.GetFromJsonAsync<Note>($"api/notes/note/{id}");
        }

        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                throw new ArgumentException(Translate(problemDetails.title));
            }
        }
    }
}
