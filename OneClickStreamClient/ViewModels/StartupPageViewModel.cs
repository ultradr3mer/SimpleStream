using OneClickStreamClient.Configuration;
using OneClickStreamClient.Services;
using OneClickStreamClient.Views;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Diagnostics;
using System.IO;

namespace OneClickStreamClient.ViewModels
{
  public class StartupPageViewModel : BindableBase
  {
    #region Fields

    private readonly Client client;
    private readonly ConfigWrapper config;
    private readonly MainWindowViewModel mainWindowViewModel;
    private readonly IRegionManager regionManager;

    #endregion Fields

    #region Constructors

    public StartupPageViewModel(Client client, ConfigWrapper config, MainWindowViewModel mainWindowViewModel, IRegionManager regionManager)
    {
      this.client = client;
      this.config = config;
      this.mainWindowViewModel = mainWindowViewModel;
      this.regionManager = regionManager;
      this.Initialize();
    }

    #endregion Constructors

    #region Methods

    private async void Initialize()
    {
      var startResponse = await this.client.StreamStartupAsync();
      this.mainWindowViewModel.PreviewSource = startResponse.PreviewSource;
      this.mainWindowViewModel.StreamId = startResponse.Id;
      this.UpdateConfig(startResponse.IngestUrl);
      this.mainWindowViewModel.ObsProcess = Process.Start(this.config.ObsExecutable);
      this.regionManager.RequestNavigate("MainRegion", new Uri(nameof(StreamlabsPage), UriKind.Relative));
    }

    private void UpdateConfig(string ingestUrl)
    {
      string serviceJson = OneClickStreamClient.Properties.Resources.Service.Replace("{server}", ingestUrl);

      if (File.Exists(config.ConfigFile))
      {
        File.Delete(config.ConfigFile);
      }

      using (var file = File.CreateText(config.ConfigFile))
      {
        file.Write(serviceJson);
        file.Flush();
      }
    }

    #endregion Methods
  }
}