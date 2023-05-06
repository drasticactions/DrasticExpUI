using DrasticExpUI.ViewModels;

namespace DrasticExpMaui.Pages;

public partial class ListViewSliderPage : ContentPage
{
	public ListViewSliderPage()
	{
		InitializeComponent();
		this.BindingContext = this.Vm = App.Current.Handler.MauiContext.Services.GetService<ListViewModel>();
	}
	
	public ListViewModel Vm { get; }
}