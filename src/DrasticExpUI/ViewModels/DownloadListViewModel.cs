using Downloader;
using Drastic.Services;
using Drastic.Tools;
using Drastic.ViewModels;
using DrasticExpUI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DrasticExpUI.ViewModels
{
    public class DownloadListViewModel : BaseViewModel
    {
        private GgmlModelService modelService;

        public DownloadListViewModel(IServiceProvider services) : base(services)
        {
            this.modelService = services.GetService(typeof(GgmlModelService)) as GgmlModelService ?? throw new NullReferenceException(nameof(GgmlModelService));
            foreach(var item in this.modelService.AllModels)
            {
                this.Downloads.Add(new GgmlDownload(item, this.Dispatcher));
            }
        }

        public ObservableCollection<GgmlDownload> Downloads { get; } = new ObservableCollection<GgmlDownload>();
    }

    public class GgmlModel
    {
        public GgmlModel(GgmlType type)
        {
            this.Type = type;
            this.Name = type switch
            {
                GgmlType.Tiny => "Tiny",
                GgmlType.TinyEn => "Tiny (English)",
                GgmlType.Base => "Base",
                GgmlType.BaseEn => "Base (English)",
                GgmlType.Small => "Small",
                GgmlType.SmallEn => "Small (English)",
                GgmlType.Medium => "Medium",
                GgmlType.MediumEn => "Medium (English)",
                GgmlType.LargeV1 => "Large (v1)",
                GgmlType.Large => "Large",
                _ => throw new NotImplementedException(),
            };

            this.DownloadUrl = type switch
            {
                GgmlType.Tiny => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin",
                GgmlType.TinyEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-tiny-en.bin",
                GgmlType.Base => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-base.bin",
                GgmlType.BaseEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-base-en.bin",
                GgmlType.Small => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-small.bin",
                GgmlType.SmallEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-small-en.bin",
                GgmlType.Medium => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin",
                GgmlType.MediumEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-medium-en.bin",
                GgmlType.LargeV1 => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-large-v1.bin",
                GgmlType.Large => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-large.bin",
                _ => throw new NotImplementedException(),
            };

            // TODO: Add descriptions
            this.Description = type switch
            {
                GgmlType.Tiny => "Tiny model trained on 1.5M samples",
                GgmlType.TinyEn => "Tiny model trained on 1.5M samples (English)",
                GgmlType.Base => "Base model trained on 1.5M samples",
                GgmlType.BaseEn => "Base model trained on 1.5M samples (English)",
                GgmlType.Small => "Small model trained on 1.5M samples",
                GgmlType.SmallEn => "Small model trained on 1.5M samples (English)",
                GgmlType.Medium => "Medium model trained on 1.5M samples",
                GgmlType.MediumEn => "Medium model trained on 1.5M samples (English)",
                GgmlType.LargeV1 => "Large model trained on 1.5M samples (v1)",
                GgmlType.Large => "Large model trained on 1.5M samples",
                _ => throw new NotImplementedException(),
            };

            this.FileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Drastic.Whisper", Path.GetFileName(this.DownloadUrl));
        }

        public GgmlType Type { get; }

        public string Name { get; }

        public string DownloadUrl { get; }

        public string Description { get; }

        public string FileLocation { get; }

        public bool IsDownloaded {
            get {
                return File.Exists(this.FileLocation);
            }
        }
    }

    public class GgmlDownload : INotifyPropertyChanged, IDisposable, IErrorHandlerService
    {
        private DownloadService download;
        CancellationTokenSource source;
        private bool downloadStarted;
        private double precent;
        private bool disposedValue;
        private IAppDispatcher dispatcher;

        public GgmlDownload(GgmlModel model, IAppDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.Model = model;

            this.download = new DownloadService(new DownloadConfiguration()
            {
                ChunkCount = 8,
                ParallelDownload = true,
            });

            this.download.DownloadStarted += Download_DownloadStarted;
            this.download.DownloadFileCompleted += Download_DownloadFileCompleted;
            this.download.DownloadProgressChanged += Download_DownloadProgressChanged;

            this.source = new CancellationTokenSource();

            this.DownloadCommand = new AsyncCommand(this.DownloadAsync, () => !string.IsNullOrEmpty(this.Model.DownloadUrl), this.dispatcher, this);
            this.CancelCommand = new AsyncCommand(this.CancelAsync, null, this.dispatcher, this);
            this.DeleteCommand = new AsyncCommand(this.DeleteAsync, null, this.dispatcher, this);
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        public GgmlModel Model { get; private set; }

        public double Precent {
            get { return this.precent; }
            set { this.SetProperty(ref this.precent, value); }
        }

        public bool DownloadStarted {
            get { return this.downloadStarted; }
            set { this.SetProperty(ref this.downloadStarted, value); }
        }

        public bool ShowDownloadButton => !this.Model.IsDownloaded && !this.DownloadStarted;

        public bool ShowCancelButton => this.DownloadStarted;

        public bool ShowDeleteButton => this.Model.IsDownloaded && !this.DownloadStarted;

        private void UpdateButtons()
        {
            this.OnPropertyChanged(nameof(ShowDownloadButton));
            this.OnPropertyChanged(nameof(ShowCancelButton));
            this.OnPropertyChanged(nameof(ShowDeleteButton));

            this.DownloadCommand.RaiseCanExecuteChanged();
            this.CancelCommand.RaiseCanExecuteChanged();
            this.DeleteCommand.RaiseCanExecuteChanged();
        }

        private void Download_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
        {
            this.Precent = e.ProgressPercentage / 100;
        }

        private void Download_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.DownloadStarted = false;
            if (e.Cancelled && e.UserState is Downloader.DownloadPackage package)
            {
                this.DeleteAsync().FireAndForgetSafeAsync();
            }

           this.UpdateButtons();
        }

        public AsyncCommand DownloadCommand { get; }

        public AsyncCommand CancelCommand { get; }

        public AsyncCommand DeleteCommand { get; }

        private void Download_DownloadStarted(object? sender, DownloadStartedEventArgs e)
        {
            this.DownloadStarted = true;
            this.UpdateButtons();
        }

        private async Task DownloadAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.Model.FileLocation)!);
            await this.download.DownloadFileTaskAsync(this.Model.DownloadUrl, this.Model.FileLocation, this.source.Token);
        }

        private async Task CancelAsync()
        {
            this.download.CancelAsync();
            this.UpdateButtons();
        }

        private async Task DeleteAsync()
        {
            if (File.Exists(this.Model.FileLocation))
                File.Delete(this.Model.FileLocation);

            this.OnPropertyChanged(nameof(this.Model.IsDownloaded));
            this.UpdateButtons();
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.source?.Cancel();
                    this.download.DownloadStarted -= Download_DownloadStarted;
                    this.download.DownloadFileCompleted -= Download_DownloadFileCompleted;
                    this.download.DownloadProgressChanged -= Download_DownloadProgressChanged;
                    this.download.Dispose();
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void HandleError(Exception ex)
        {
        }
    }

    public enum GgmlType
    {
        Tiny,
        TinyEn,
        Base,
        BaseEn,
        Small,
        SmallEn,
        Medium,
        MediumEn,
        LargeV1,
        Large
    }
}
