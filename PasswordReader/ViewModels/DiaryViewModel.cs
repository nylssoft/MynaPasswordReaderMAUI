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
    public class DiaryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DiaryViewModel()
        {
            var today = DateTime.Now;
            _month = today.Month;
            _year = today.Year;
        }

        private int _month;
        public int Month
        {
            get => _month;
            set
            {
                if (_month == value) return;
                _month = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Month)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        private int _year;
        public int Year
        {
            get => _year;
            set
            {
                if (_year == value) return;
                _year = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Year)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        public string Title => $"{new DateTime(Year, Month, 1).ToString("MMMM", new CultureInfo("de-DE"))} {Year}";

        private List<int> _days;
        public List<int> Days
        {
            get => _days;
            set
            {
                if (_days == value) return;
                _days = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Days)));
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrorMessage)));
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(_errorMessage);
    }
}
