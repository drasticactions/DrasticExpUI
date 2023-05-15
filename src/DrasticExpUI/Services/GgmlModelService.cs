using DrasticExpUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Drastic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DrasticExpUI.Services
{
    public class GgmlModelService : INotifyPropertyChanged
    {
        private string basePath;
        private IAppDispatcher dispatcher;
        private GgmlModel? selectedModel;
        
        public GgmlModelService(IServiceProvider provider)
        {
            this.dispatcher = provider.GetRequiredService<IAppDispatcher>();
            this.basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Drastic.Whisper");
            foreach (var item in Enum.GetValues(typeof(GgmlType)))
            {
                this.AllModels.Add(new GgmlModel((GgmlType)item));
            }

            this.UpdateAvailableModels();
        }
        
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<GgmlModel> AllModels { get; } = new ObservableCollection<GgmlModel>();

        public ObservableCollection<GgmlModel> AvailableModels { get; } = new ObservableCollection<GgmlModel>();
        
        public GgmlModel? SelectedModel
        {
            get
            {
                return this.selectedModel;
            }

            set
            {
                this.SetProperty(ref this.selectedModel, value);
            }
        }

        public void UpdateAvailableModels()
        {
            lock (this)
            {
                this.AvailableModels.Clear();
                var models = this.AllModels.Where(n => n.IsDownloaded);
                foreach(var model in models)
                {
                    this.AvailableModels.Add(model);
                }

                if (this.SelectedModel is null) return;
                if (!this.AvailableModels.Contains(this.SelectedModel))
                {
                    this.SelectedModel = null;
                }
            }
        }
        
        /// <summary>
        /// On Property Changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.dispatcher?.Dispatch(() =>
            {
                var changed = this.PropertyChanged;
                if (changed == null)
                {
                    return;
                }

                changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

#pragma warning disable SA1600 // Elements should be documented
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
#pragma warning restore SA1600 // Elements should be documented
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
