﻿/*
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
    public class NoteItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private long _id = -1;

        public long Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }

        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        private string _content = "";
        public string Content
        {
            get => _content;
            set
            {
                if (_content == value) return;
                _content = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
            }
        }

        private string _lastModified;
        public string LastModified
        {
            get => _lastModified;
            set
            {
                if (_lastModified == value) return;
                _lastModified = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastModified)));
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

        private bool _changed;
        public bool Changed
        {
            get => _changed;
            set
            {
                if (_changed == value) return;
                _changed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Changed)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSaveNote)));
            }
        }

        public bool IsUpdating { get; set; } = true;

        public bool CanDeleteNote => !_isRunning;

        public bool CanSaveNote => _changed && !_isRunning;

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanDeleteNote)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSaveNote)));
            }
        }

    }
}
