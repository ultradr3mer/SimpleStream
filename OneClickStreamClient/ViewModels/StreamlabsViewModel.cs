using OneClickStreamClient.Services;
using OneClickStreamClient.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows.Threading;

namespace OneClickStreamClient.ViewModels
{
  public class StreamlabsViewModel : BindableBase
  {
    #region Fields

    private readonly Client client;

    private readonly MainWindowViewModel mainWindowViewModel;
    private readonly IRegionManager regionManager;
    private DispatcherTimer timer;

    #endregion Fields

    #region Constructors

    public StreamlabsViewModel(Client client, MainWindowViewModel mainWindowViewModel, IRegionManager regionManager)
    {
      this.client = client;
      this.mainWindowViewModel = mainWindowViewModel;
      this.regionManager = regionManager;

      this.Message = "Awaiting input...";
    }

    #endregion Constructors

    #region Properties

    public DelegateCommand ContinueCommand { get; }

    public DelegateCommand RefreshCommand { get; }

    public string Message { get; set; }

    #endregion Properties

    #region Methods

    internal void Initialize()
    {
      this.timer = new DispatcherTimer();
      this.timer.Tick += this.TimerTick;
      this.timer.Interval = new TimeSpan(0, 0, 3);
      this.timer.Start();
    }

    private async void TimerTick(object sender, EventArgs e)
    {
      var outputResponse = await this.client.StreamCheckpreviewAsync(this.mainWindowViewModel.StreamId);

      if(outputResponse.IsInputReceived)
      {
        this.Message = "Input received, buffering...";
      }

      if (outputResponse.IsPreviewReady)
      {
        this.timer.Stop();
        this.Message = "Success...";

        this.regionManager.RequestNavigate("MainRegion", new Uri(nameof(PreviewPage), UriKind.Relative));
      }
    }

    #endregion Methods
  }
}