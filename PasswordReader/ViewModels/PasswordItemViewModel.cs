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
using System.ComponentModel;

namespace PasswordReader.ViewModels
{
    [Serializable]
    public class PasswordItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                if (_imageUrl == value) return;
                _imageUrl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageUrl)));
            }
        }

        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                if (_url == value) return;
                _url = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Url)));
            }
        }

        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                if (_login == value) return;
                _login = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Login)));
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                _description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;
                _password = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage == value) return;
                _statusMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
            }
        }
    }
}
