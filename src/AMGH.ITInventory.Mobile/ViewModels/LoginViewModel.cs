using AMGH.ITInventory.Mobile.Pages;
using AMGH.ITInventory.Shared.Client;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AMGH.ITInventory.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAmghApiClient _api; private readonly IServiceProvider _sp;
    [ObservableProperty] private string _userName = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _error = "";
    [ObservableProperty] private bool _busy;
    public LoginViewModel(IAmghApiClient api, IServiceProvider sp) { _api = api; _sp = sp; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        Busy = true; Error = "";
        try
        {
            var r = await _api.LoginAsync(UserName, Password);
            if (r?.Data is null) { Error = "Invalid credentials"; return; }
            await Application.Current!.MainPage!.Navigation.PushAsync(_sp.GetRequiredService<ScanPage>());
        }
        catch (HttpRequestException) { Error = "Cannot reach server. Check network."; }
        finally { Busy = false; }
    }
}
