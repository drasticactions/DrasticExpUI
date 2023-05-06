using System.Collections.ObjectModel;
using Drastic.ViewModels;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;

namespace DrasticExpUI.ViewModels;

public class ListViewModel : BaseViewModel
{
    private IRandomizerTimeSpan timespan;
    private IRandomizerString randomString;
    public ListViewModel(IServiceProvider services)
        : base(services)
    {
        this.randomString = RandomizerFactory.GetRandomizer(new FieldOptionsTextWords() { Min = 5, Max = 10});
        this.timespan = RandomizerFactory.GetRandomizer(new FieldOptionsTimeSpan());
        for (var i = 0; i < 100; i++)
        {
            var text = this.randomString.Generate()!;
            var time = this.timespan.Generate()!;
            this.ListItems.Add(new TestClass(i, text, time.Value));
        }
    }
    
   public ObservableCollection<TestClass> ListItems { get; } = new ObservableCollection<TestClass>();

   public class TestClass
   {
       public TestClass(int id, string title, TimeSpan detail)
       {
           this.Id = id;
           this.Text = title;
           this.Time = detail;
       }
       
       public int Id { get; }
       
       public string Text { get; }
       
       public TimeSpan Time { get; }
   }
}