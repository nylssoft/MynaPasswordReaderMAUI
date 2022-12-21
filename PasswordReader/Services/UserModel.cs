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

        public string secKey { get; set; }
    }
}
