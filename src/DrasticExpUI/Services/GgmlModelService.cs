using DrasticExpUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrasticExpUI.Services
{
    public class GgmlModelService
    {
        private string basePath;

        public GgmlModelService()
        {
            this.basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Drastic.Whisper");
            foreach (var item in Enum.GetValues(typeof(GgmlType)))
            {
                this.AllModels.Add(new GgmlModel((GgmlType)item));
            }
        }

        public ObservableCollection<GgmlModel> AllModels { get; } = new ObservableCollection<GgmlModel>();
    }
}
