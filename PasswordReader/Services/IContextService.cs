/*
    Myna Password Reader MAUI
    Copyright (C) 2022-2023 Niels Stockfleth

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
    public interface IContextService
    {

        Task LoginAsync(string username, string password);

        Task Login2FAAsync(string securityCode);

        Task LoginWithTokenAsync();

        Task LoginWithPinAsync(string pin);

        Task<List<PasswordItem>> GetPasswordItemsAsync();

        Task UploadPasswordItemsAsync(List<PasswordItem> items);

        Task<string> DecodePasswordAsync(string encryptedPassword);

        Task<string> EncodePasswordAsync(string password);

        Task<List<ContactItem>> GetContactItemsAsync();

        Task UploadContactItemsAsync(List<ContactItem> items);

        Task<List<Note>> GetNotesAsync();

        Task<Note> GetNoteAsync(long noteId);

        Task<long> CreateNoteAsync();

        Task DeleteNoteAsync(long noteId);

        Task<DateTime> UpdateNoteAsync(long noteId, string title, string content);

        Task<List<int>> GetDiaryDaysAsync(int year, int month);

        Task<Diary> GetDiaryAsync(int year, int month, int day);

        Task SaveDiaryAsync(int year, int month, int day, string entry);

        Task<List<DocumentItem>> GetDocumentItemsAsync(long? currentId = null);

        Task<byte[]> DownloadDocumentAsync(long id);

        Task<string> GetFamilyAccessTokenAsync();

        Task LogoutAsync();

        bool IsLoggedIn();

        bool Requires2FA();

        bool RequiresPin();

        bool HasPasswordItems();

        bool IsLoggedOut();

        Task<bool> HasLoginTokenAsync();

        Task<bool> HasEncryptionKeyAsync();

        Task<string> GetEncryptionKeyAsync();

        Task SetEncryptionKeyAsync(string encryptionKey);

        string GetUsername();

        string GetUserPhotoUrl();

        string GetEmailAddress();

        string GetLastLogin();

        bool NoteChanged { get; set; }

        bool PasswordChanged { get; set; }

        bool DiaryChanged { get; set; }

        bool ContactChanged { get; set; }

        string StartPage { get; set; }
    }
}
