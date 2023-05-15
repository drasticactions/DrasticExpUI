using Downloader;
using Drastic.Tools;
using Drastic.ViewModels;

namespace DrasticExpUI.ViewModels
{
    public class DownloadViewModel : BaseViewModel
    {
        private string downloadUrl = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-large-v1.bin";
        private DownloadService download;
        CancellationTokenSource source;
        private bool downloadStarted;
        private double precent;

        public DownloadViewModel(IServiceProvider services)
            : base(services)
        {
            var downloadOpt = new DownloadConfiguration()
            {
                ChunkCount = 8,
                ParallelDownload = true,
            };
            this.download = new DownloadService(downloadOpt);
            this.DownloadCommand = new AsyncCommand(this.Download, () => !string.IsNullOrEmpty(this.DownloadUrl), this.Dispatcher, this.ErrorHandler);
            this.CancelCommand = new AsyncCommand(this.Cancel, null, this.Dispatcher, this.ErrorHandler);
            this.download.DownloadStarted += Download_DownloadStarted;
            this.download.DownloadFileCompleted += Download_DownloadFileCompleted;
            this.download.DownloadProgressChanged += Download_DownloadProgressChanged;
            this.source = new CancellationTokenSource();
        }

        private void Download_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
        {
            this.Precent = e.ProgressPercentage;
        }

        private void Download_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.DownloadStarted = false;
            if (e.Cancelled && e.UserState is Downloader.DownloadPackage package)
            {
                if (!string.IsNullOrEmpty(package.FileName))
                    File.Delete(package.FileName);
            }
        }

        private void Download_DownloadStarted(object? sender, DownloadStartedEventArgs e)
        {
            this.DownloadStarted = true;
        }

        public double Precent {
            get { return this.precent; }
            set { this.SetProperty(ref this.precent, value); }
        }

        public bool DownloadStarted {
            get { return this.downloadStarted; }
            set { this.SetProperty(ref this.downloadStarted, value); }
        }

        public string DownloadUrl {
            get { return this.downloadUrl; }
            set { this.SetProperty(ref this.downloadUrl, value);
                this.RaiseCanExecuteChanged();
            }
        }

        public AsyncCommand DownloadCommand { get; }

        public AsyncCommand CancelCommand { get; }

        private async Task Download()
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Drastic.Whisper");
            Directory.CreateDirectory(basePath);
            var filename = System.IO.Path.GetFileName(this.DownloadUrl);
            await this.download.DownloadFileTaskAsync(this.DownloadUrl, Path.Combine(basePath, filename), this.source.Token);
        }

        private async Task Cancel()
        {
            this.download.CancelAsync();
        }

        public override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.DownloadCommand.RaiseCanExecuteChanged();
        }
    }
}
