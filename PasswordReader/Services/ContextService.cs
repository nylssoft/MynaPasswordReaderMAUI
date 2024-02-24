/*
    Myna Password Reader MAUI
    Copyright (C) 2022-2023 Niels Stockfleth

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

        private byte[] _cryptoKey;

        private bool _requires2FA;

        private bool _requiresPin;

        private string _loginToken;

        private bool _restClientInit;

        private UserModel _userModel;

        private string _token;

        private bool _noteChanged = false;

        private bool _passwordChanged = false;

        private bool _diaryChanged = false;

        private bool _contactChanged = false;

        private string _startPage = null;

        public bool NoteChanged { get => _noteChanged; set => _noteChanged = value; }

        public bool PasswordChanged { get => _passwordChanged; set => _passwordChanged = value; }

        public bool DiaryChanged { get => _diaryChanged; set => _diaryChanged = value; }

        public bool ContactChanged { get => _contactChanged; set => _contactChanged = value; }

        public string StartPage
        {
            get
            {
                _startPage ??= Preferences.Default.Get(nameof(StartPage), "//passwordlist");
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
            _requiresPin = false;
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
                _requiresPin = false;
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task LoginWithTokenAsync()
        {
            if (_loggedIn || _requires2FA) throw new ArgumentException("Du bist noch angemeldet.");
            if (!await HasLoginTokenAsync()) throw new ArgumentException("Anmeldung mit Langzeit-Token nicht möglich.");
            await InitAsync();
            try
            {
                string clientUUID = "";
                var clinfo = await SecureStorage.Default.GetAsync("clientinfo");
                if (clinfo != null)
                {
                    var clientInfo = JsonSerializer.Deserialize<ClientInfo>(clinfo);
                    clientUUID = clientInfo.UUID;
                }
                var authResult = await RestClient.AuthenticateLLTokenAsync(_loginToken, clientUUID);
                if (authResult.requiresPin)
                {
                    _requiresPin = true;
                }
                else
                {
                    _userModel = await RestClient.GetUserAsync(authResult.token);
                    _token = authResult.token;
                    _loggedIn = true;
                    _requires2FA = false;
                    _requiresPin = false;
                    if (_loginToken != authResult.longLivedToken)
                    {
                        _loginToken = authResult.longLivedToken;
                        await SecureStorage.Default.SetAsync("lltoken", _loginToken);
                    }
                }
            }
            catch
            {
                _loginToken = "";
                SecureStorage.Default.Remove("lltoken");
                throw;
            }
        }

        public async Task LoginWithPinAsync(string pin)
        {
            if (_loggedIn || _requires2FA) throw new ArgumentException("Du bist noch angemeldet.");
            if (!_requiresPin) throw new ArgumentException("Anmeldung mit PIN nicht möglich.");
            if (!await HasLoginTokenAsync()) throw new ArgumentException("Anmeldung mit Langzeit-Token nicht möglich.");
            await InitAsync();
            var authResult = await RestClient.AuthenticatePin(_loginToken, pin);
            _userModel = await RestClient.GetUserAsync(authResult.token);
            _token = authResult.token;
            _loggedIn = true;
            _requires2FA = false;
            _requiresPin = false;
            if (_loginToken != authResult.longLivedToken)
            {
                _loginToken = authResult.longLivedToken;
                await SecureStorage.Default.SetAsync("lltoken", _loginToken);
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
            _requiresPin = false;
            _token = null;
            _loginToken = "";
            _userModel = null;
            _encryptionKey = null;
            _cryptoKey = null;
            NoteChanged = false;
            PasswordChanged = false;
            DiaryChanged = false;
            ContactChanged = false;
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

        public bool RequiresPin()
        {
            return _requiresPin;
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
                _loginToken ??= "";
            }
            return !string.IsNullOrEmpty(_loginToken);
        }

        public async Task<bool> HasEncryptionKeyAsync()
        {
            if (!_loggedIn) return false;
            if (_encryptionKey == null)
            {
                var storageValue = await SecureStorage.Default.GetAsync(StorageKey);
                storageValue ??= await MigrateEncryptionKeyAsync();
                if (storageValue == null)
                {
                    _encryptionKey = "";
                    _cryptoKey = null;
                }
                else
                {
                    try
                    {
                        // decrypt storage value with secKey
                        var key = Convert.FromHexString(_userModel.secKey);
                        var data = Convert.FromHexString(storageValue);
                        var decryptedData = DecryptData(data, key);
                        _encryptionKey = Encoding.UTF8.GetString(decryptedData);
                        // update derived crypto key (PKBDF2)
                        _cryptoKey = GenerateKey(_encryptionKey, _userModel.passwordManagerSalt);
                    }
                    catch
                    {
                        _encryptionKey = "";
                        _cryptoKey = null;
                    }
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
                if (string.IsNullOrEmpty(_encryptionKey))
                {
                    _cryptoKey = null;
                    SecureStorage.Default.Remove(StorageKey);
                }
                else
                {
                    // update derived crypto key (PKBDF2)
                    _cryptoKey = GenerateKey(_encryptionKey, _userModel.passwordManagerSalt);
                    // encrypt storage value with secKey
                    var key = Convert.FromHexString(_userModel.secKey);
                    var data = Encoding.UTF8.GetBytes(_encryptionKey);
                    var encryptedData = EncryptData(data, key);
                    var storageValue = Convert.ToHexString(encryptedData);
                    await SecureStorage.Default.SetAsync(StorageKey, storageValue);
                }
            }
        }

        public async Task<List<ContactItem>> GetContactItemsAsync()
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            try
            {
                var encrypted = await RestClient.GetContactsAsync(_token);
                if (!string.IsNullOrEmpty(encrypted))
                {
                    var json = DecodeText(encrypted);
                    var contactData = JsonSerializer.Deserialize<ContactData>(json);
                    return contactData.items;
                }
                return [];
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task UploadContactItemsAsync(List<ContactItem> contactItems)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            long nextId = 1;
            if (contactItems.Count != 0)
            {
                nextId = contactItems.Max(x => x.id) + 1;
            }
            var contactData = new ContactData
            {
                version = 1,
                nextId = nextId,
                items = [.. contactItems.OrderBy(x => x.id)]
            };
            try
            {
                var json = JsonSerializer.Serialize(contactData);
                var encoded = EncodeText(json);
                await RestClient.SetContactsAsync(_token, encoded);
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
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
                return DecryptPasswordItems(encrypted);
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task UploadPasswordItemsAsync(List<PasswordItem> items)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items));
            var encrypted = EncryptData(data, _cryptoKey);
            var content = JsonSerializer.Serialize(Convert.ToHexString(encrypted));
            try
            {
                await RestClient.UploadPasswordFileAsync(_token, content);
                if (!_userModel.hasPasswordManagerFile)
                {
                    _userModel.hasPasswordManagerFile = true;
                }
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task<string> DecodePasswordAsync(string encryptedPassword)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            return DecodeText(encryptedPassword);
        }

        public async Task<string> EncodePasswordAsync(string password)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            return EncodeText(password);
        }

        public async Task<List<Note>> GetNotesAsync()
        {
            List<Note> ret = [];
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            try
            {
                var notes = await RestClient.GetNotesAsync(_token);
                foreach (var note in notes)
                {
                    note.title = DecodeText(note.title);
                    ret.Add(note);
                }
                return ret;
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
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
                note.title = DecodeText(note.title);
                note.content = DecodeText(note.content);
                return note;
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task<long> CreateNoteAsync()
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            string encryptedTitle = EncodeText("Neue Notiz");
            try
            {
                var noteid = await RestClient.CreateNewNoteAsync(_token, encryptedTitle);
                return noteid;
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task DeleteNoteAsync(long id)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            try
            {
                await RestClient.DeleteNoteAsync(_token, id);
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task<DateTime> UpdateNoteAsync(long id, string title, string content)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var encryptedTitle = EncodeText(title);
            var encryptedContent = EncodeText(content);
            try
            {
                return await RestClient.UpdateNoteAsync(_token, id, encryptedTitle, encryptedContent);
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task<List<int>> GetDiaryDaysAsync(int year, int month)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            try
            {
                return await RestClient.GetDiaryDaysAsync(_token, year, month);
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
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
                diary.entry = DecodeText(diary.entry);
                return diary;
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
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
                encryptedEntry = EncodeText(entry);
            }
            try
            {
                await RestClient.SaveDiaryAsync(_token, year, month, day, encryptedEntry);
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
            }
        }

        public async Task<List<DocumentItem>> GetDocumentItemsAsync(long? currentId)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            try
            {
                return await RestClient.GetDocumentItemsAsync(_token, currentId);
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
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
                return DecryptData(encryptedData, _cryptoKey);
            }
            catch (InvalidTokenException)
            {
                await LogoutAsync();
                throw;
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

        private string DecodeText(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted))
            {
                return "";
            }
            var decoded = DecryptData(Convert.FromHexString(encrypted), _cryptoKey);
            return Encoding.UTF8.GetString(decoded);
        }

        private string EncodeText(string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            var encrypted = EncryptData(data, _cryptoKey);
            return Convert.ToHexString(encrypted);
        }

        private List<PasswordItem> DecryptPasswordItems(string encrypted)
        {
            var data = DecryptData(Convert.FromHexString(encrypted), _cryptoKey);
            var plainText = Encoding.UTF8.GetString(data);
            var pwdItems = JsonSerializer.Deserialize<List<PasswordItem>>(plainText);
            pwdItems.Sort((i1, i2) => i1.Name.CompareTo(i2.Name));
            return pwdItems;
        }


        private static byte[] GenerateKey(string cryptKey, string salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                cryptKey,
                Encoding.UTF8.GetBytes(salt),
                1000, HashAlgorithmName.SHA256,
                256 / 8);
        }

        private static byte[] EncryptData(byte[] data, byte[] key)
        {
            var iv = new byte[12];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            var encoded = new byte[data.Length];
            var tag = new byte[16];
            using (var cipher = new AesGcm(key, tag.Length))
            {
                cipher.Encrypt(iv, data, encoded, tag);
            }
            var ret = new byte[iv.Length + encoded.Length + tag.Length];
            iv.CopyTo(ret, 0);
            encoded.CopyTo(ret, iv.Length);
            tag.CopyTo(ret, iv.Length + encoded.Length);
            return ret;
        }

        private static byte[] DecryptData(byte[] data, byte [] key)
        {
            byte[] iv = data[0..12];
            byte[] chipherText = data[12..^16];
            byte[] tag = data[^16..];
            byte[] plainText = new byte[chipherText.Length];
            using (var cipher = new AesGcm(key, tag.Length))
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

        private string StorageKey { get => $"encryptkey-{_userModel.id}-secure"; }

        private async Task<string> MigrateEncryptionKeyAsync()
        {
            var oldStorageKey = $"encryptkey-{_userModel.id}";
            var oldStorageValue = await SecureStorage.Default.GetAsync(oldStorageKey);
            if (oldStorageValue != null)
            {
                SecureStorage.Default.Remove(oldStorageKey);
                // encrypt old storage value with secKey
                var data = Encoding.UTF8.GetBytes(oldStorageValue);
                var key = Convert.FromHexString(_userModel.secKey);
                var encryptedData = EncryptData(data, key);
                var newStorageValue = Convert.ToHexString(encryptedData);
                await SecureStorage.Default.SetAsync(StorageKey, newStorageValue);
                return newStorageValue;
            }
            return null;
        }
    }
}
