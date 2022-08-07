using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PasswordReader.ViewModels
{
    public class ContextViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        public bool CanLogin => !_isLoggedIn && !_requires2FA;

        public bool CanConfirmSecurityCode => _requires2FA;

        public bool CanChangeEncryptionKey => _isLoggedIn;

        public bool CanLogout => _isLoggedIn || _requires2FA;

        public bool CanDecodePasswordItems => _isLoggedIn && _hasPasswordItems;
    }
}
