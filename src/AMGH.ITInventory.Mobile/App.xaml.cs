using AMGH.ITInventory.Mobile.Pages;
namespace AMGH.ITInventory.Mobile;

public partial class App : Application
{
    public App(IServiceProvider sp) { InitializeComponent(); MainPage = new NavigationPage(sp.GetRequiredService<LoginPage>()); }
}
