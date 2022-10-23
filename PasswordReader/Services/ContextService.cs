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
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PasswordReader.Services
{
    public class ContextService : IContextService
    {
        private bool _loggedIn;

        private string _encryptionKey;

        private bool _requires2FA;

        private string _loginToken;

        private bool _restClientInit;

        private UserModel _userModel;

        private string _token;

        private bool _noteChanged = false;

        private bool _passwordChanged = false;

        private bool _diaryChanged = false;

        private string _startPage = null;

        public bool NoteChanged { get => _noteChanged; set => _noteChanged = value; }

        public bool PasswordChanged { get => _passwordChanged; set => _passwordChanged = value; }

        public bool DiaryChanged { get => _diaryChanged; set => _diaryChanged = value; }

        public string StartPage
        {
            get
            {
                if (_startPage == null)
                {
                    _startPage = Preferences.Default.Get(nameof(StartPage), "//passwordlist");
                }
                return _startPage;
            }
            set
            {
                if (_startPage == value) return;
                _startPage = value;
                Preferences.Default.Set(nameof(StartPage), _startPage);
            }
        }

        private async Task InitAsync()
        {
            if (!_restClientInit)
            {
                await RestClient.InitAsync("https://www.stockfleth.eu", "de");
                _restClientInit = true;
            }
        }

        public async Task LoginAsync(string username, string password)
        {
            if (_loggedIn || _requires2FA) throw new ArgumentException("Du bist noch angemeldet.");
            ClientInfo clientInfo;
            var clinfo = await SecureStorage.Default.GetAsync("clientinfo");
            if (clinfo == null)
            {
                clientInfo = new ClientInfo { Name = "Myna Password Reader MAUI", UUID = Guid.NewGuid().ToString() };
                await SecureStorage.SetAsync("clientinfo", JsonSerializer.Serialize(clientInfo));
            }
            else
            {
                clientInfo = JsonSerializer.Deserialize<ClientInfo>(clinfo);
            }
            await InitAsync();
            var authResult = await RestClient.AuthenticateAsync(username, password, clientInfo, "de");
            if (!authResult.requiresPass2)
            {
                _userModel = await RestClient.GetUserAsync(authResult.token);
                _loggedIn = true;
            }
            _token = authResult.token;
            _loginToken = authResult.longLivedToken;
            _requires2FA = authResult.requiresPass2;
            if (string.IsNullOrEmpty(_loginToken))
            {
                SecureStorage.Default.Remove("lltoken");
                _loginToken = "";
            }
            else
            {
                await SecureStorage.Default.SetAsync("lltoken", _loginToken);
            }
        }

        public async Task Login2FAAsync(string securityCode)
        {
            if (_loggedIn) throw new ArgumentException("Du bist noch angemeldet.");
            if (!_requires2FA) throw new ArgumentException("2-Faktor-Anmeldung ist nicht aktiviert.");
            await InitAsync();
            try
            {
                var authResult = await RestClient.AuthenticatePass2Async(_token, securityCode);
                _token = authResult.token;
                _loginToken = authResult.longLivedToken;
                if (string.IsNullOrEmpty(_loginToken))
                {
                    SecureStorage.Default.Remove("lltoken");
                    _loginToken = "";
                }
                else
                {
                    await SecureStorage.Default.SetAsync("lltoken", _loginToken);
                }
                _userModel = await RestClient.GetUserAsync(_token);
                _loggedIn = true;
                _requires2FA = false;
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task LoginWithTokenAsync()
        {
            if (_loggedIn || _requires2FA) throw new ArgumentException("Du bist noch angemeldet.");
            if (!await HasLoginTokenAsync()) throw new ArgumentException("Anmeldung mit Langzeit-Token nicht möglich.");
            await InitAsync();
            try
            {
                var authResult = await RestClient.AuthenticateLLTokenAsync(_loginToken);
                _userModel = await RestClient.GetUserAsync(authResult.token);
                _token = authResult.token;
                _loggedIn = true;
                _requires2FA = false;
                if (_loginToken != authResult.longLivedToken)
                {
                    _loginToken = authResult.longLivedToken;
                    await SecureStorage.Default.SetAsync("lltoken", _loginToken);
                }
            }
            catch
            {
                _loginToken = "";
                SecureStorage.Default.Remove("lltoken");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await RestClient.LogoutAsync(_token);
            }
            catch
            {
                // ignored
            }
            _loggedIn = false;
            _requires2FA = false;
            _token = null;
            _loginToken = "";
            _userModel = null;
            _encryptionKey = null;
            NoteChanged = false;
            PasswordChanged = false;
            DiaryChanged = false;
            SecureStorage.Default.Remove("lltoken");
            await App.ContextViewModel.InitAsync();
        }

        public bool IsLoggedIn()
        {
            return _loggedIn;
        }

        public bool Requires2FA()
        {
            return _requires2FA;
        }

        public bool IsLoggedOut()
        {
            return !_loggedIn && !_requires2FA;
        }

        public async Task<bool> HasLoginTokenAsync()
        {
            if (_loginToken == null)
            {
                _loginToken = await SecureStorage.Default.GetAsync("lltoken");
                if (_loginToken == null)
                {
                    _loginToken = "";
                }
            }
            return !string.IsNullOrEmpty(_loginToken);
        }

        public async Task<bool> HasEncryptionKeyAsync()
        {
            if (!_loggedIn) return false;
            if (_encryptionKey == null)
            {
                _encryptionKey = await SecureStorage.Default.GetAsync($"encryptkey-{_userModel.id}");
                if (_encryptionKey == null)
                {
                    _encryptionKey = "";
                }
            }
            return !string.IsNullOrEmpty(_encryptionKey);
        }

        public bool HasPasswordItems()
        {
            return _loggedIn && _userModel.hasPasswordManagerFile;
        }

        public async Task<string> GetEncryptionKeyAsync()
        {
            if (await HasEncryptionKeyAsync())
            {
                return _encryptionKey;
            }
            return "";
        }

        public async Task SetEncryptionKeyAsync(string encryptionKey)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (encryptionKey == null) throw new ArgumentException("Ungültiger Parameter.");
            var currentKey = await GetEncryptionKeyAsync();
            if (encryptionKey != currentKey)
            {
                _encryptionKey = encryptionKey;
                var storageKey = $"encryptkey-{_userModel.id}";
                if (string.IsNullOrEmpty(_encryptionKey))
                {
                    SecureStorage.Default.Remove(storageKey);
                }
                else
                {
                    await SecureStorage.Default.SetAsync(storageKey, _encryptionKey);
                }
            }
        }

        public async Task<List<PasswordItem>> GetPasswordItemsAsync()
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            try
            {
                var encrypted = await RestClient.GetPasswordFileAsync(_token);
                return DecryptPasswordItems(encrypted, encryptionKey, _userModel.passwordManagerSalt);
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task UploadPasswordItemsAsync(List<PasswordItem> items)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items));
            var encrypted = Encrypt(data, encryptionKey, _userModel.passwordManagerSalt);
            var content = JsonSerializer.Serialize(Convert.ToHexString(encrypted));
            try
            {
                await RestClient.UploadPasswordFileAsync(_token, content);
                if (!_userModel.hasPasswordManagerFile)
                {
                    _userModel.hasPasswordManagerFile = true;
                }
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task<string> DecodePasswordAsync(string encryptedPassword)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            return DecodeText(encryptedPassword, encryptionKey, _userModel.passwordManagerSalt);
        }

        public async Task<string> EncodePasswordAsync(string password)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            return EncodeText(password, encryptionKey, _userModel.passwordManagerSalt);
        }

        public async Task<List<Note>> GetNotesAsync()
        {
            List<Note> ret = new();
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            try
            {
                var notes = await RestClient.GetNotesAsync(_token);
                foreach (var note in notes)
                {
                    note.title = DecodeText(note.title, encryptionKey, _userModel.passwordManagerSalt);
                    ret.Add(note);
                }
                return ret;
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task<Note> GetNoteAsync(long id)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            try
            {
                var note = await RestClient.GetNoteAsync(_token, id);
                note.title = DecodeText(note.title, encryptionKey, _userModel.passwordManagerSalt);
                note.content = DecodeText(note.content, encryptionKey, _userModel.passwordManagerSalt);
                return note;
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task<long> CreateNoteAsync()
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            string encryptedTitle = EncodeText("Neue Notiz", encryptionKey, _userModel.passwordManagerSalt);
            try
            {
                var noteid = await RestClient.CreateNewNoteAsync(_token, encryptedTitle);
                return noteid;
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task DeleteNoteAsync(long id)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            try
            {
                await RestClient.DeleteNoteAsync(_token, id);
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task<DateTime> UpdateNoteAsync(long id, string title, string content)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var encryptedTitle = EncodeText(title, encryptionKey, _userModel.passwordManagerSalt);
            var encryptedContent = EncodeText(content, encryptionKey, _userModel.passwordManagerSalt);
            try
            {
                return await RestClient.UpdateNoteAsync(_token, id, encryptedTitle, encryptedContent);
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task<List<int>> GetDiaryDaysAsync(int year, int month)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            try
            {
                return await RestClient.GetDiaryDaysAsync(_token, year, month);
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task<Diary> GetDiaryAsync(int year, int month, int day)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            try
            {
                var diary = await RestClient.GetDiaryAsync(_token, year, month, day);
                diary.entry = DecodeText(diary.entry, encryptionKey, _userModel.passwordManagerSalt);
                return diary;
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task SaveDiaryAsync(int year, int month, int day, string entry)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            string encryptedEntry = "";
            if (!string.IsNullOrEmpty(entry))
            {
                encryptedEntry = EncodeText(entry, encryptionKey, _userModel.passwordManagerSalt);
            }
            try
            {
                await RestClient.SaveDiaryAsync(_token, year, month, day, encryptedEntry);
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task<List<DocumentItem>> GetDocumentItemsAsync(long? currentId)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            try
            {
                return await RestClient.GetDocumentItemsAsync(_token, currentId);
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public async Task<byte[]> DownloadDocumentAsync(long id)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            try
            {
                var encryptedData = await RestClient.DownloadDocumentAsync(_token, id);
                return Decrypt(encryptedData, encryptionKey, _userModel.passwordManagerSalt);
            }
            catch (InvalidTokenException ex)
            {
                await LogoutAsync();
                throw ex;
            }
        }

        public string GetUsername()
        {
            if (_loggedIn)
            {
                return _userModel.name;
            }
            return "";
        }

        public string GetEmailAddress()
        {
            if  (_loggedIn)
            {
                return _userModel.email;
            }
            return "";
        }

        public string GetLastLogin()
        {
            if (_loggedIn && _userModel.lastLoginUtc.HasValue)
            {
                return _userModel.lastLoginUtc.Value.ToLocalTime().ToString("g", new CultureInfo("de-DE"));
            }
            return "";
        }

        public string GetUserPhotoUrl()
        {
            if (_loggedIn && !string.IsNullOrEmpty(_userModel.photo))
            {
                return $"https://www.stockfleth.eu/{_userModel.photo}";
            }
            return "eyedark.png";
        }

        // helpers

        private static string DecodeText(string encrypted, string cryptKey, string salt)
        {
            if (string.IsNullOrEmpty(encrypted))
            {
                return "";
            }
            var decoded = Decrypt(Convert.FromHexString(encrypted), cryptKey, salt);
            return Encoding.UTF8.GetString(decoded);
        }

        private static string EncodeText(string text, string cryptKey, string salt)
        {
            return Convert.ToHexString(Encrypt(Encoding.UTF8.GetBytes(text), cryptKey, salt));
        }

        private static List<PasswordItem> DecryptPasswordItems(string encrypted, string cryptKey, string salt)
        {
            var plainText = Decrypt(Convert.FromHexString(encrypted), cryptKey, salt);
            var pwdItems = JsonSerializer.Deserialize<List<PasswordItem>>(plainText);
            pwdItems.Sort((i1, i2) => i1.Name.CompareTo(i2.Name));
            return pwdItems;
        }

        private static byte[] Encrypt(byte[] data, string cryptKey, string salt)
        {
            var iv = new byte[12];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            var key = Rfc2898DeriveBytes.Pbkdf2(
                cryptKey,
                Encoding.UTF8.GetBytes(salt),
                1000, HashAlgorithmName.SHA256,
                256 / 8);
            var encoded = new byte[data.Length];
            var tag = new byte[16];
            using (var cipher = new AesGcm(key))
            {
                cipher.Encrypt(iv, data, encoded, tag);
            }
            var ret = new byte[iv.Length + encoded.Length + tag.Length];
            iv.CopyTo(ret, 0);
            encoded.CopyTo(ret, iv.Length);
            tag.CopyTo(ret, iv.Length + encoded.Length);
            return ret;
        }

        private static byte[] Decrypt(byte[] data, string cryptKey, string salt)
        {
            byte[] iv = data[0..12];
            byte[] chipherText = data[12..^16];
            byte[] tag = data[^16..];
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                cryptKey,
                Encoding.UTF8.GetBytes(salt),
                1000,
                HashAlgorithmName.SHA256,
                256 / 8);
            byte[] plainText = new byte[chipherText.Length];
            using (var cipher = new AesGcm(key))
            {
                try
                {
                    cipher.Decrypt(iv, chipherText, tag, plainText);
                }
                catch
                {
                    throw new ArgumentException("Der Schlüssel ist ungültig.");
                }
            }
            return plainText;
        }
    }
}
