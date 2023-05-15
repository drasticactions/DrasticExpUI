using DrasticExpUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrasticExpMaui.Pages;

public partial class DownloadListPage : ContentPage
{
    public DownloadListPage()
    {
        InitializeComponent();
        this.BindingContext = this.Vm = App.Current.Handler.MauiContext.Services.GetService<DownloadListViewModel>();
    }

    public DownloadListViewModel Vm;
}