using AMGH.ITInventory.Mobile.ViewModels;
using ZXing.Net.Maui;

namespace AMGH.ITInventory.Mobile.Pages;

public partial class ScanPage : ContentPage
{
    private readonly ScanViewModel _vm;
    public ScanPage(ScanViewModel vm)
    {
        InitializeComponent(); BindingContext = _vm = vm;
        Reader.Options = new BarcodeReaderOptions { Formats = BarcodeFormats.All, AutoRotate = true, Multiple = false };
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results?.FirstOrDefault(); if (first is null) return;
        MainThread.BeginInvokeOnMainThread(async () => await _vm.OnCodeScannedAsync(first.Value));
    }
}
