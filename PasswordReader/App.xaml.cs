using PasswordReader.Services;
using PasswordReader.ViewModels;

namespace PasswordReader;

public partial class App : Application
{
	public static IContextService ContextService { get; private set; }

	public static ContextViewModel ContextViewModel { get; private set; }

	public App(IContextService contextService, ContextViewModel viewModel)
	{
        ContextService = contextService;
        ContextViewModel = viewModel;
        InitializeComponent();
		MainPage = new AppShell();
	}
}
