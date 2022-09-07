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
                    _imageUrl = "eyedark.png";
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
                        }
                    }
                }
                return _imageUrl;
            }
        }
    }
}
