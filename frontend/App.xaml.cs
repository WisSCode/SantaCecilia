using Microsoft.Extensions.DependencyInjection;

namespace frontend;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
