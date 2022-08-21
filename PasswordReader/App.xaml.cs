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

namespace PasswordReader;

public partial class App : Application
{
	public static IContextService ContextService { get; private set; }

	public static ContextViewModel ContextViewModel { get; private set; }

    public static bool IsLightAppTheme => Current.RequestedTheme == AppTheme.Light || Current.RequestedTheme == AppTheme.Unspecified;

    public App(IContextService contextService, ContextViewModel viewModel)
	{
        ContextService = contextService;
        ContextViewModel = viewModel;
        InitializeComponent();
		MainPage = new AppShell();
	}
}
