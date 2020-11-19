using MahApps.Metro.Controls;
using OneClickStreamClient.ViewModels;
using Prism.Ioc;
using Prism.Regions;
using System.Windows;
using Unity;

namespace OneClickStreamClient.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : MetroWindow
  {
    #region Constructors

    public MainWindow(IRegionManager regionManager, IUnityContainer container)
    {
      InitializeComponent();

      regionManager.RegisterViewWithRegion("MainRegion", () => container.Resolve<StartupPage>());
      regionManager.RegisterViewWithRegion("MainRegion", () => container.Resolve<StreamlabsPage>());
      regionManager.RegisterViewWithRegion("MainRegion", () => container.Resolve<PreviewPage>());
      regionManager.RegisterViewWithRegion("MainRegion", () => container.Resolve<StreamRunningPage>());
    }

    #endregion Constructors

    #region Methods

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      ((MainWindowViewModel)this.DataContext).Unload();
    }

    #endregion Methods
  }
}