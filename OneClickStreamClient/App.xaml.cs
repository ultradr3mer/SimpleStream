using OneClickStreamClient.ViewModels;
using OneClickStreamClient.Views;
using Prism.Ioc;
using System.Windows;

namespace OneClickStreamClient
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    #region Methods

    protected override Window CreateShell()
    {
      var mainWindow = Container.Resolve<MainWindow>();
      return mainWindow;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterSingleton<MainWindowViewModel>();
      containerRegistry.RegisterSingleton<StartupPageViewModel>();
      containerRegistry.RegisterSingleton<StreamlabsPage>();
      containerRegistry.RegisterSingleton<StreamRunningPageViewModel>();
    }

    #endregion Methods
  }
}