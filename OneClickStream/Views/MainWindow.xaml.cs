using OneClickStream.ViewModels;
using Prism.Ioc;
using Prism.Regions;
using System.Windows;
using Unity;

namespace OneClickStream.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    #region Constructors

    public MainWindow(IRegionManager regionManager, IUnityContainer container)
    {
      InitializeComponent();

      regionManager.RegisterViewWithRegion("MainRegion", () => container.Resolve<StartupPage>());
      regionManager.RegisterViewWithRegion("MainRegion", () => container.Resolve<StreamlabsPage>());
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