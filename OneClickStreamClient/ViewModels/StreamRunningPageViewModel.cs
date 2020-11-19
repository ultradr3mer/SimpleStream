using OneClickStreamClient.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Web;

namespace OneClickStreamClient.ViewModels
{
  public class StreamRunningPageViewModel : BindableBase
  {
    #region Fields

    private readonly Client client;
    private readonly MainWindowViewModel mainWindowViewModel;

    #endregion Fields

    #region Constructors

    public StreamRunningPageViewModel(MainWindowViewModel mainWindowViewModel, Client client)
    {
      this.OpenBrowserCommand = new DelegateCommand(this.OpenBrowserCommandExecute);
      this.EndStreamCommand = new DelegateCommand(this.EndStreamCommandExecute);
      this.mainWindowViewModel = mainWindowViewModel;
      this.client = client;
    }

    #endregion Constructors

    #region Properties

    public DelegateCommand EndStreamCommand { get; }

    public DelegateCommand OpenBrowserCommand { get; }

    #endregion Properties

    #region Methods

    private async void EndStreamCommandExecute()
    {
      this.ShutDownObs();
      await this.client.StreamCleanupAsync(this.mainWindowViewModel.StreamId);
      System.Windows.Application.Current.Shutdown();
    }

    private void OpenBrowserCommandExecute()
    {
      string source = HttpUtility.HtmlEncode(this.mainWindowViewModel.OutputSource + "&format=dash&heuristicprofile=lowlatency");
      System.Diagnostics.Process.Start("https://oneclickstream.azurewebsites.net/stream?source=" + source);
    }

    private void ShutDownObs()
    {
      if (!this.mainWindowViewModel.ObsProcess.HasExited)
      {
        this.mainWindowViewModel.ObsProcess.CloseMainWindow();
        this.mainWindowViewModel.ObsProcess.Close();
      }
    }

    #endregion Methods
  }
}