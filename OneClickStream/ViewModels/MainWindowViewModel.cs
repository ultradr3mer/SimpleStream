using Microsoft.Extensions.Configuration;
using OneClickStream.Configuration;
using OneClickStream.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace OneClickStream.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    #region Fields

    private readonly IRegionManager regionManager;
    private Client client;
    private ConfigWrapper config;

    #endregion Fields

    #region Constructors

    public MainWindowViewModel(IRegionManager regionManager, IUnityContainer container)
    {
      this.config = new ConfigWrapper(new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .Build());
      container.RegisterInstance(this.config);

      this.client = new Client(new CustomHttpClient(config.Username, config.Password));
      container.RegisterInstance(this.client);
    }

    #endregion Constructors


    #region Properties

    public string StreamId { get; set; }
    public Process ObsProcess { get; set; }
    public string OutputSource { get; set; }
    public string Title { get; set; } = "Prism Application";
    public string PreviewSource { get; set; }

    #endregion Properties

    #region Methods

    internal void Unload()
    {
      this.client.StreamCleanup(this.StreamId);
    }

    #endregion Methods
  }
}