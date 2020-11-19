using OneClickStreamClient.ViewModels;
using Prism.Regions;
using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OneClickStreamClient.Views
{
  /// <summary>
  /// Interaction logic for StreamlabsPage
  /// </summary>
  public partial class StreamlabsPage : UserControl, INavigationAware
  {
    #region Constructors

    public StreamlabsPage(StreamlabsViewModel viewModel)
    {
      InitializeComponent();

      this.viewModel = viewModel;
    }

    #endregion Constructors

    #region Properties

    private StreamlabsViewModel viewModel
    {
      get { return this.DataContext as StreamlabsViewModel; }
      set { this.DataContext = value; }
    }

    #endregion Properties

    #region Methods

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      this.viewModel.Initialize();
    }

    private void DispatcherTimerTick(object sender, EventArgs e)
    {
      throw new NotImplementedException();
    }

    #endregion Methods
  }
}