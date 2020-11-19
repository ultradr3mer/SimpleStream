using Microsoft.Extensions.Configuration;
using OneClickStreamClient.Configuration;
using OneClickStreamClient.Services;
using Prism.Mvvm;
using Prism.Regions;
using System.Diagnostics;
using System.IO;
using Unity;

namespace OneClickStreamClient.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {

    #region Fields

    private readonly Client client;
    private readonly ConfigWrapper config;

    #endregion Fields

    #region Constructors

    public MainWindowViewModel(IRegionManager regionManager, IUnityContainer container)
    {
      this.config = new ConfigWrapper(new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .Build());
      container.RegisterInstance(this.config);

      this.client = new Client(new CustomHttpClient(this.config.Username, this.config.Password));
      container.RegisterInstance(this.client);
    }

    #endregion Constructors

    #region Properties

    public Process ObsProcess { get; set; }
    public string OutputSource { get; set; }
    public string PreviewSource { get; set; }
    public string StreamId { get; set; }
    public string Title { get; set; } = "OneClickStreamClient";

    #endregion Properties

    #region Methods

    internal void Unload()
    {
      this.client.StreamCleanup(this.StreamId);
    }

    #endregion Methods

  }
}