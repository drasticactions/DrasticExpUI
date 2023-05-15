using DrasticExpUI.ViewModels;

namespace DrasticExpMaui.Pages;

public partial class DownloadViewModelPage : ContentPage
{
	public DownloadViewModelPage()
	{
		InitializeComponent();
        this.BindingContext = this.Vm = App.Current.Handler.MauiContext.Services.GetService<DownloadViewModel>();
    }

    public DownloadViewModel Vm { get; }
}