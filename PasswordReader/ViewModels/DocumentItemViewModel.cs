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
    public class DocumentItemViewModel : INotifyPropertyChanged
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

        private string _documentType;
        public string DocumentType
        {
            get => _documentType;
            set
            {
                if (_documentType == value) return;
                _documentType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DocumentType)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DocumentTypeImage)));
            }
        }

        public string DocumentTypeImage => _documentType == "Document" ? "filepdfdark.png" : "folderdark.png";

        private long _id;
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

        private long? _parentId;
        public long? ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId == value) return;
                _parentId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParentId)));
            }
        }
    }
}
