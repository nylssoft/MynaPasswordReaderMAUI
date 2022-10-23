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
using PasswordReader.Services;
using PasswordReader.ViewModels;
using System.Collections.ObjectModel;

namespace PasswordReader.Pages;

public partial class DocumentListPage : ContentPage
{
    private ContextViewModel _model;

    public DocumentListPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
    }

    protected async override void OnAppearing()
	{
		base.OnAppearing();
        App.ContextService.StartPage = "//documentlist";
        if (_model.DocumentItems == null || _model.HasErrorMessage)
        {
            await GetDocumentItemsAsync();
        }
    }

    private DocumentItemViewModel GetDocumentItemViewModel(DocumentItem item)
    {
        return new DocumentItemViewModel
        {
            Name = item.name,
            DocumentType = item.type,
            Id = item.id,
            ParentId = item.parentId
        };
    }

    private async Task GetDocumentItemsAsync(long? id = null)
    {
        if (!_model.IsLoggedIn)
        {
            _model.ErrorMessage = "Du bist nicht angemeldet.";
            return;
        }
        try
        {
            _model.IsRunning = true;
            var items = await App.ContextService.GetDocumentItemsAsync(id);
            var volume = GetDocumentItemViewModel(items.First((i) => i.type == "Volume"));
            _model.CurrentDocumentItem = id == null ? volume : GetDocumentItemViewModel(items.First((i) => i.id == id));
            if (id == null)
            {
                id = volume.Id;
            }
            _model.DocumentItems = new ObservableCollection<DocumentItemViewModel>();
            var docs = items.Where(i => i.type == "Document" && i.accessRole == null).OrderBy(i => i.name);
            var folders = items.Where(i => i.type == "Folder" && i.parentId == id && i.accessRole == null).OrderBy(i => i.name);
            foreach (var folder in folders)
            {
                _model.DocumentItems.Add(GetDocumentItemViewModel(folder));
            }
            foreach (var doc in docs)
            {
                _model.DocumentItems.Add(GetDocumentItemViewModel(doc));
            }
            _model.IsRunning = false;
            _model.ErrorMessage = "";
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            _model.ErrorMessage = ex.Message;
        }
    }

    private async Task<string> DownloadAsync(long id, string name)
    {
        var ext = Path.GetExtension(name).ToLowerInvariant();
        var data = await App.ContextService.DownloadDocumentAsync(id);
        var fileName = Path.Combine(FileSystem.Current.CacheDirectory, $"docitem{ext}");
        using FileStream outputStream = File.OpenWrite(fileName);
        await outputStream.WriteAsync(data);
        return fileName;
    }

    private async Task ProcessDocumentItem(CollectionView view, DocumentItemViewModel item)
    {
        if (item.DocumentType == "Folder")
        {
            await GetDocumentItemsAsync(item.Id);
        }
        else if (item.DocumentType == "Volume")
        {
            await GetDocumentItemsAsync();
        }
        else if (item.DocumentType == "Document")
        {
            _model.IsRunning = true;
            try
            {
                var fileName = await DownloadAsync(item.Id, item.Name);
                await Launcher.Default.OpenAsync(new OpenFileRequest($"Öffne {item.Name}", new ReadOnlyFile(fileName)));
                _model.IsRunning = false;
                _model.ErrorMessage = "";
            }
            catch (Exception ex)
            {
                _model.IsRunning = false;
                _model.ErrorMessage = ex.Message;
            }
            view.SelectedItem = null;
        }
    }

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 1)
        {
            var selected = e.CurrentSelection[0] as DocumentItemViewModel;
            var view = sender as CollectionView;
            if (view != null && selected != null)
            {
                await ProcessDocumentItem(view, selected);
            }
        }
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        await GetDocumentItemsAsync();
    }

    private async void UpButton_Clicked(object sender, EventArgs e)
    {
        await GetDocumentItemsAsync(_model.CurrentDocumentItem?.ParentId);
    }

}