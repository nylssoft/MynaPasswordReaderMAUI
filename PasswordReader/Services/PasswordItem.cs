using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordReader.Services
{
    public class PasswordItem
    {
        private string _imageUrl;

        public string Name { get; set; }
        public string Url { get; set; }
        public string Login { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }

        public PasswordItem()
        {
        }

        public PasswordItem(PasswordItem item)
        {
            Name = item.Name;
            Url = item.Url;
            Login = item.Login;
            Description = item.Description;
            Password = item.Password;
        }

        public string ImageUrl
        {
            get
            {
                if (_imageUrl == null)
                {
                    var u = Url;
                    if (!string.IsNullOrEmpty(u))
                    {
                        if (!u.ToLowerInvariant().StartsWith("http"))
                        {
                            u = $"https://{Url}";
                        }
                        try
                        {
                            var host = new Uri(u).Host;
                            _imageUrl = $"https://www.google.com/s2/favicons?domain={host}";
                        }
                        catch
                        {
                            _imageUrl = "";
                        }
                    }
                }
                return _imageUrl;
            }
        }
    }
}
