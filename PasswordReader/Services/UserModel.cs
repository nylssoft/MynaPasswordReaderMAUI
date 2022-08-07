using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordReader.Services
{
    public class UserModel
    {
        public long id { get; set; }

        public string name { get; set; }

        public string email { get; set; }

        public bool requires2FA { get; set; }

        public bool useLongLivedToken { get; set; }

        public bool allowResetPassword { get; set; }

        public DateTime? lastLoginUtc { get; set; }

        public DateTime? registeredUtc { get; set; }

        public List<string> roles { get; set; }

        public bool hasPasswordManagerFile { get; set; }

        public string passwordManagerSalt { get; set; }

        public bool accountLocked { get; set; }

        public string photo { get; set; }

        public long storageQuota { get; set; }

        public long usedStorage { get; set; }

        public bool loginEnabled { get; set; }
    }
}
