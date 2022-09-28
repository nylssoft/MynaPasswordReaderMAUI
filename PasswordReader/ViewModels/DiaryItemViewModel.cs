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
using System.Globalization;

namespace PasswordReader.ViewModels
{
    [Serializable]
    public class DiaryItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date == value) return;
                _date = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayDate)));
            }
        }

        private string _entry = "";
        public string Entry
        {
            get => _entry;
            set
            {
                if (_entry == value) return;
                _entry = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Entry)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanDeleteDiary)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasStatusMessage)));
            }
        }

        public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

        private bool _changed;
        public bool Changed
        {
            get => _changed;
            set
            {
                if (_changed == value) return;
                _changed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Changed)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSaveDiary)));
            }
        }

        public bool IsUpdating { get; set; } = true;

        public bool CanSaveDiary => _changed && !_isRunning;

        public bool CanDeleteDiary => !_isRunning && !string.IsNullOrEmpty(Entry);

        public string DisplayDate => _date.ToLocalTime().ToString("d", new CultureInfo("de-DE"));

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSaveDiary)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanDeleteDiary)));
            }
        }

    }
}
