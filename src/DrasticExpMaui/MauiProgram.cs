using CommunityToolkit.Maui;
using Drastic.Services;
using DrasticExpMaui.Services;
using DrasticExpUI.Services;
using DrasticExpUI.ViewModels;
using Microsoft.Extensions.Logging;

namespace DrasticExpMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.Services
            .AddSingleton<IAppDispatcher, MauiAppDispatcherService>()
            .AddSingleton<IErrorHandlerService, ExampleErrorHandler>()
            .AddSingleton<GgmlModelService>()
            .AddSingleton<DownloadViewModel>()
             .AddSingleton<DownloadListViewModel>()
            .AddSingleton<ListViewModel>();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
