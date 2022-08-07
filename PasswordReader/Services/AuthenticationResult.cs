using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordReader.Services
{
    public class AuthenticationResult
    {
        public string token { get; set; }
        public bool requiresPass2 { get; set; }
        public string longLivedToken { get; set; }
        public string username { get; set; }
    }
}
