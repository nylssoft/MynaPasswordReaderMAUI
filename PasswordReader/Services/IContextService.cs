using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordReader.Services
{
    public interface IContextService
    {
        Task LoginAsync(string username, string password);

        Task Login2FAAsync(string securityCode);

        Task LoginWithTokenAsync();

        Task<List<PasswordItem>> DecodePasswordItemsAsync();

        Task<string> DecodePasswordAsync(string password);

        void Logout();

        bool IsLoggedIn();
        
        bool Requires2FA();

        bool HasPasswordItems();

        bool IsLoggedOut();

        Task<bool> HasLoginTokenAsync();

        Task<bool> HasEncryptionKeyAsync();

        Task<string> GetEncryptionKeyAsync();

        Task SetEncryptionKeyAsync(string encryptionKey);
    }
}
