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
    public interface IContextService
    {
        Task LoginAsync(string username, string password);

        Task Login2FAAsync(string securityCode);

        Task LoginWithTokenAsync();

        Task<List<PasswordItem>> GetPasswordItemsAsync();

        Task<string> DecodePasswordAsync(string password);

        Task<List<Note>> GetNotesAsync();

        Task<Note> GetNoteAsync(long noteId);

        Task<long> CreateNoteAsync();

        Task DeleteNoteAsync(long noteId);

        Task<DateTime> UpdateNoteAsync(long noteId, string title, string content);

        Task LogoutAsync();

        bool IsLoggedIn();
        
        bool Requires2FA();

        bool HasPasswordItems();

        bool IsLoggedOut();

        Task<bool> HasLoginTokenAsync();

        Task<bool> HasEncryptionKeyAsync();

        Task<string> GetEncryptionKeyAsync();

        Task SetEncryptionKeyAsync(string encryptionKey);

        string GetUsername();

        string GetUserPhotoUrl();

        bool NoteChanged { get; set; }
    }
}
