/*
    Myna Password Reader MAUI
    Copyright (C) 2023 Niels Stockfleth

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
    public class ContactData
    {
        public long nextId { get; set; }

        public int version { get; set; }

        public List<ContactItem> items { get; set; }
    }

    public class ContactItem
    {
        public long id { get; set; }
        public string name { get; set; }
        public string birthday { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string note { get; set; }
    }
}
