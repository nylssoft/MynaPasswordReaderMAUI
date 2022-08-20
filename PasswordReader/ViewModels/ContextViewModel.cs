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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PasswordReader.ViewModels
{
    public class ContextViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        public async Task InitAsync()
        {
            Username = App.ContextService.GetUsername();
            UserPhotoUrl = App.ContextService.GetUserPhotoUrl();
            EmailAddress = App.ContextService.GetEmailAddress();
            LastLogin = App.ContextService.GetLastLogin();
            Password = "";
            SecurityCode = "";
            EncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
            IsLoggedIn = App.ContextService.IsLoggedIn();
            Requires2FA = App.ContextService.Requires2FA();
            HasLoginToken = await App.ContextService.HasLoginTokenAsync();
            HasPasswordItems = App.ContextService.HasPasswordItems();
            PasswordItems = null;
            SelectedPasswordItem = null;
            NoteItems = null;
            SelectedNoteItem = null;
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username == value) return;
                _username = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Username)));
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

        private string _encryptionKey;
        public string EncryptionKey
        {
            get => _encryptionKey;
            set
            {
                if (_encryptionKey == value) return;
                _encryptionKey = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncryptionKey)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCreateNote)));
            }
        }

        private string _securityCode;
        public string SecurityCode
        {
            get => _securityCode;
            set
            {
                if (_securityCode == value) return;
                _securityCode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecurityCode)));
            }
        }

        private bool _hasLoginToken;
        public bool HasLoginToken
        {
            get => _hasLoginToken;
            set
            {
                if (_hasLoginToken == value) return;
                _hasLoginToken = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasLoginToken)));
            }
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                if (_isLoggedIn == value) return;
                _isLoggedIn = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogin)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogout)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanChangeEncryptionKey)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanDecodePasswordItems)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCreateNote)));
            }
        }

        private bool _requires2FA;
        public bool Requires2FA
        {
            get => _requires2FA;
            set
            {
                if (_requires2FA == value) return;
                _requires2FA = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Requires2FA)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogin)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogout)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanConfirmSecurityCode)));
            }
        }

        private bool _hasPasswordItems;
        public bool HasPasswordItems
        {
            get => _hasPasswordItems;
            set
            {
                if (_hasPasswordItems == value) return;
                _hasPasswordItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasPasswordItems)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanDecodePasswordItems)));
            }
        }

        private ObservableCollection<PasswordItemViewModel> _passwordItems;
        public ObservableCollection<PasswordItemViewModel> PasswordItems
        {
            get => _passwordItems;
            set
            {
                if (_passwordItems == value) return;
                _passwordItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PasswordItems)));
            }
        }

        private PasswordItemViewModel _selectedPasswordItem;
        public PasswordItemViewModel SelectedPasswordItem
        {
            get => _selectedPasswordItem;
            set
            {
                if (_selectedPasswordItem == value) return;
                _selectedPasswordItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPasswordItem)));
            }
        }

        private string _emailAddress;
        public string EmailAddress
        {
            get => _emailAddress;
            set
            {
                if (_emailAddress == value) return;
                _emailAddress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmailAddress)));
            }
        }

        private string _lastLogin;
        public string LastLogin
        {
            get => _lastLogin;
            set
            {
                if (_lastLogin == value) return;
                _lastLogin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastLogin)));
            }
        }

        private string _userPhotoUrl;
        public string UserPhotoUrl
        {
            get => _userPhotoUrl;
            set
            {
                if (_userPhotoUrl == value) return;
                _userPhotoUrl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserPhotoUrl)));
            }
        }

        private ObservableCollection<NoteItemViewModel> _noteItems;
        public ObservableCollection<NoteItemViewModel> NoteItems
        {
            get => _noteItems;
            set
            {
                if (_noteItems == value) return;
                _noteItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NoteItems)));
            }
        }

        private NoteItemViewModel _selectedNoteItem;
        public NoteItemViewModel SelectedNoteItem
        {
            get => _selectedNoteItem;
            set
            {
                if (_selectedNoteItem == value) return;
                _selectedNoteItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedNoteItem)));
            }
        }

        public bool CanLogin => !_isLoggedIn && !_requires2FA && !_isRunning;

        public bool CanConfirmSecurityCode => _requires2FA && !_isRunning;

        public bool CanChangeEncryptionKey => _isLoggedIn;

        public bool CanLogout => _isLoggedIn || _requires2FA;

        public bool CanDecodePasswordItems => _isLoggedIn && _hasPasswordItems;

        public bool CanCreateNote => _isLoggedIn && !string.IsNullOrEmpty(_encryptionKey);

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogin)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogout)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanConfirmSecurityCode)));
            }
        }

    }
}
