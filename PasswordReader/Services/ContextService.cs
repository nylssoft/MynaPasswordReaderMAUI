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

        private const string UNKNOWN = "???????";

        public bool NoteChanged { get => _noteChanged; set => _noteChanged = value; }

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
            _token = authResult.token;
            _loginToken = authResult.longLivedToken;
            _requires2FA = authResult.requiresPass2;
            if (!_requires2FA)
            {
                _userModel = await RestClient.GetUserAsync(_token);
                _loggedIn = true;
            }
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

        public async Task LoginWithTokenAsync()
        {
            if (_loggedIn || _requires2FA) throw new ArgumentException("Du bist noch angemeldet.");
            if (!await HasLoginTokenAsync()) throw new ArgumentException("Anmeldung mit Langzeit-Token nicht möglich.");
            await InitAsync();
            try
            {
                var authResult = await RestClient.AuthenticateLLTokenAsync(_loginToken);
                _token = authResult.token;
                _userModel = await RestClient.GetUserAsync(_token);
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
            SecureStorage.Default.Remove("lltoken");
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
                await SecureStorage.Default.SetAsync($"encryptkey-{_userModel.id}", _encryptionKey);
            }
        }

        public async Task<List<PasswordItem>> GetPasswordItemsAsync()
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var encrypted = await RestClient.GetPasswordFileAsync(_token);
            return DecryptPasswordItems(encrypted, encryptionKey, _userModel.passwordManagerSalt);
        }

        public async Task<string> DecodePasswordAsync(string password)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            return Encoding.UTF8.GetString(Decrypt(Convert.FromHexString(password), encryptionKey, _userModel.passwordManagerSalt));
        }

        public async Task<List<Note>> GetNotesAsync()
        {
            List<Note> ret = new();
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var notes = await RestClient.GetNotesAsync(_token);
            foreach (var note in notes)
            {
                try
                {
                    note.title = DecodeText(note.title, encryptionKey, _userModel.passwordManagerSalt);
                }
                catch
                {
                    note.title = UNKNOWN;
                }
                ret.Add(note);
            }
            return ret;
        }

        public async Task<Note> GetNoteAsync(long id)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var note = await RestClient.GetNoteAsync(_token, id);
            try
            {
                note.title = DecodeText(note.title, encryptionKey, _userModel.passwordManagerSalt);
                note.content = DecodeText(note.content, encryptionKey, _userModel.passwordManagerSalt);
            }
            catch
            {
                note.title = UNKNOWN;
                note.content = "";
            }
            return note;
        }

        public async Task<long> CreateNoteAsync()
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            string encryptedTitle = EncodeText("Neue Notiz", encryptionKey, _userModel.passwordManagerSalt);
            var noteid = await RestClient.CreateNewNoteAsync(_token, encryptedTitle);
            return noteid;
        }

        public async Task DeleteNoteAsync(long id)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            await RestClient.DeleteNoteAsync(_token, id);
        }

        public async Task<DateTime> UpdateNoteAsync(long id, string title, string content)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var encryptedTitle = EncodeText(title, encryptionKey, _userModel.passwordManagerSalt);
            var encryptedContent = EncodeText(content, encryptionKey, _userModel.passwordManagerSalt);
            return await RestClient.UpdateNoteAsync(_token, id, encryptedTitle, encryptedContent);
        }

        public string GetUsername()
        {
            if (_loggedIn)
            {
                return _userModel.name;
            }
            return "";
        }

        public string GetUserPhotoUrl()
        {
            if (_loggedIn && !string.IsNullOrEmpty(_userModel.photo))
            {
                return $"https://www.stockfleth.eu/{_userModel.photo}";
            }
            return "https://www.stockfleth.eu/images/skat/profiles/default1.png";
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
#pragma warning disable CA1416 // Validate platform compatibility
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
#pragma warning restore CA1416 // Validate platform compatibility
            return plainText;
        }
    }
}
