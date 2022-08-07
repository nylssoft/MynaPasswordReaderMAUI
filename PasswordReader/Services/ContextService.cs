using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

        private readonly ClientInfo _clientInfo = new() { Name = "MAUI Password", UUID = "969B87CC-9E1B-4E1F-96C6-02720DFC580E" };

        private async Task InitAsync()
        {
            if (!_restClientInit)
            {
                await RestClient.Init("https://www.stockfleth.eu", "de");
                _restClientInit = true;
            }
        }

        public async Task LoginAsync(string username, string password)
        {
            if (_loggedIn || _requires2FA) throw new ArgumentException("Du bist noch angemeldet.");
            await InitAsync();
            var authResult = await RestClient.Authenticate(username, password, _clientInfo, "de");
            _token = authResult.token;
            _loginToken = authResult.longLivedToken;
            _requires2FA = authResult.requiresPass2;
            if (!_requires2FA)
            {
                _userModel = await RestClient.GetUser(_token);
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
            var authResult = await RestClient.AuthenticatePass2(_token, securityCode);
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
            _userModel = await RestClient.GetUser(_token);
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
                var authResult = await RestClient.AuthenticateLLToken(_loginToken);
                _token = authResult.token;
                _userModel = await RestClient.GetUser(_token);
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

        public void Logout()
        {
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

        public async Task<List<PasswordItem>> DecodePasswordItemsAsync()
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            var encrypted = await RestClient.GetPasswordFile(_token);
            return DecryptPasswordItems(encrypted, encryptionKey, _userModel.passwordManagerSalt);
        }

        public async Task<string> DecodePasswordAsync(string password)
        {
            if (!_loggedIn) throw new ArgumentException("Du bist nicht angemeldet.");
            if (!_userModel.hasPasswordManagerFile) throw new ArgumentException("Es wurden keine Passwörter hochgeladen.");
            var encryptionKey = await GetEncryptionKeyAsync();
            if (string.IsNullOrEmpty(encryptionKey)) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
            return Encoding.UTF8.GetString(Decrypt(HexStringToByteArray(password), encryptionKey, _userModel.passwordManagerSalt));
        }

        // helpers

        private static List<PasswordItem> DecryptPasswordItems(string encrypted, string cryptKey, string salt)
        {
            var plainText = Decrypt(HexStringToByteArray(encrypted), cryptKey, salt);
            var pwdItems = JsonSerializer.Deserialize<List<PasswordItem>>(plainText);
            pwdItems.Sort((i1, i2) => i1.Name.CompareTo(i2.Name));
            return pwdItems;
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
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
                cipher.Decrypt(iv, chipherText, tag, plainText);
            }
#pragma warning restore CA1416 // Validate platform compatibility
            return plainText;
        }
    }
}
