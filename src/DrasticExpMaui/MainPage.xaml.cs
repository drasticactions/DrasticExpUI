using System.Collections.ObjectModel;
using System.Reflection;

namespace DrasticExpMaui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		this.BindingContext = this;
		this.SetupPages();
	}

	private void SetupPages()
	{
        string ns = "DrasticExpMaui.Pages";

        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.Namespace == ns && type.IsSubclassOf(typeof(Page)))
            {
				this.Pages.Add(new PageHolder() { Title = type.Name, PageType = type });
            }
        }
    }

	public ObservableCollection<PageHolder> Pages { get; private set; } = new ObservableCollection<PageHolder>();

	public class PageHolder
	{
		public string Title { get; set; }

		public Type PageType { get; set; }
	}

    private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
		if (e.SelectedItem is not PageHolder holder)
		{
			return;
		}

		Page page = (Page)Activator.CreateInstance(holder.PageType);
		Navigation.PushAsync(page);
		((ListView)sender).SelectedItem = null;
    }
}

