using AMGH.ITInventory.Mobile.ViewModels;
namespace AMGH.ITInventory.Mobile.Pages;
public partial class LoginPage : ContentPage { public LoginPage(LoginViewModel vm) { InitializeComponent(); BindingContext = vm; } }
