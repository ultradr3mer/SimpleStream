using OneClickStreamClient.Services;
using OneClickStreamClient.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;

namespace OneClickStreamClient.ViewModels
{
  public class PreviewViewModel : BindableBase
  {
    #region Fields

    private readonly Client client;

    private readonly MainWindowViewModel mainWindowViewModel;
    private readonly IRegionManager regionManager;

    public string PreviewSource { get; private set; }

    #endregion Fields

    #region Constructors

    public PreviewViewModel(Client client, MainWindowViewModel mainWindowViewModel, IRegionManager regionManager)
    {
      this.RefreshCommand = new DelegateCommand(this.RefreshCommandExecute);
      this.ContinueCommand = new DelegateCommand(this.ContinueCommandExecute);
      this.client = client;
      this.mainWindowViewModel = mainWindowViewModel;
      this.regionManager = regionManager;
    }

    #endregion Constructors

    #region Properties

    public DelegateCommand ContinueCommand { get; }

    public DelegateCommand RefreshCommand { get; }

    #endregion Properties

    #region Methods

    private async void ContinueCommandExecute()
    {
      OutputsPostResultData outputResponse = await this.client.StreamCreateoutputsAsync(this.mainWindowViewModel.StreamId);
      this.mainWindowViewModel.OutputSource = outputResponse.EndpointSource;
      this.regionManager.RequestNavigate("MainRegion", new Uri(nameof(StreamRunningPage), UriKind.Relative));
    }

    private void RefreshCommandExecute()
    {
      this.RaisePropertyChanged(nameof(this.PreviewSource));
    }

    internal void Initialize()
    {
      this.PreviewSource = this.mainWindowViewModel.PreviewSource;
    }

    #endregion Methods
  }
}
