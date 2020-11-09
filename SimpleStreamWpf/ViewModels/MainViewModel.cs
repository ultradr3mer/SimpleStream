using Prism.Commands;
using SimpleStream.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SimpleStream.ViewModels
{
  public class MainViewModel : INotifyPropertyChanged
  {
    #region Fields

    private readonly ConsoleWraper consoleWraper;
    private readonly AzureStreamService streamService;
    private NextMetod nextMetod = NextMetod.Start;

    #endregion Fields

    #region Constructors

    public MainViewModel(AzureStreamService streamService, ConsoleWraper consoleWraper)
    {
      this.streamService = streamService;
      this.consoleWraper = consoleWraper;

      this.ContinueCommand = new DelegateCommand(this.ContinueCommandExecute, this.ContinueCommandCanExecute);
      this.RefreshCommand = new DelegateCommand(this.RefreshCommandExecute);
      this.OpenInBrowser = new DelegateCommand(this.OpenInBrowserExecute);
      this.PropertyChanged += this.MainViewModel_PropertyChanged;

      this.consoleWraper.ConsoleWritten += this.ConsoleWraperConsoleWritten;
    }

    #endregion Constructors

    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion Events

    #region Enums

    public enum NextMetod
    {
      Start,
      CreateOutputs,
      Cleanup,
      Finished
    }

    #endregion Enums

    #region Properties

    public string BrowserUrl { get; set; }

    public DelegateCommand ContinueCommand { get; }

    public bool IsBusy { get; set; }

    public string Log { get; set; }

    public DelegateCommand OpenInBrowser { get; }

    public DelegateCommand RefreshCommand { get; private set; }

    public string OutputSource { get; set; }

    #endregion Properties

    #region Methods

    public async Task Start()
    {
      await this.ContinueAsync();
    }

    internal void Unload()
    {
      this.streamService.CleanUpLiveEventAndLocator();
    }

    /// <summary>
    /// Writes the Log.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The text to append.</param>
    private void ConsoleWraperConsoleWritten(object sender, string e)
    {
      this.Log += e;
    }

    private async Task ContinueAsync()
    {
      this.IsBusy = true;

      switch (this.nextMetod)
      {
        case NextMetod.Start:
          this.BrowserUrl = await this.streamService.Startup();
          this.nextMetod = NextMetod.CreateOutputs;
          break;

        case NextMetod.CreateOutputs:
          this.OutputSource = await this.streamService.CreateOutputs();
          this.nextMetod = NextMetod.Cleanup;
          break;

        case NextMetod.Cleanup:
          await this.streamService.Cleanup();
          this.nextMetod = NextMetod.Finished;
          break;
      }

      this.IsBusy = false;
    }

    private bool ContinueCommandCanExecute()
    {
      return !this.IsBusy;
    }

    private async void ContinueCommandExecute()
    {
      await this.ContinueAsync();
    }

    private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(IsBusy))
      {
        this.ContinueCommand.RaiseCanExecuteChanged();
      }
    }

    private void OpenInBrowserExecute()
    {
      System.Diagnostics.Process.Start("https://oneclickstream.azurewebsites.net/stream?source=" + this.OutputSource);
    }

    private void RefreshCommandExecute()
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BrowserUrl)));
    }

    #endregion Methods
  }
}