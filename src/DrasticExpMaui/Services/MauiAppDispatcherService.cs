

using Drastic.Services;

namespace DrasticExpMaui.Services;

public class MauiAppDispatcherService : IAppDispatcher
{
    public bool Dispatch(Action action)
    {
        return Microsoft.Maui.Controls.Application.Current!.Dispatcher.Dispatch(action);
    }
}