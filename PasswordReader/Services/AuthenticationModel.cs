using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordReader.Services
{
    public class AuthenticationModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string ClientUUID { get; set; }

        public string ClientName { get; set; }
    }
}
