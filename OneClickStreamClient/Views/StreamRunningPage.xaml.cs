using OneClickStreamClient.ViewModels;
using System.Windows.Controls;

namespace OneClickStreamClient.Views
{
  /// <summary>
  /// Interaction logic for PrismUserControl1
  /// </summary>
  public partial class StreamRunningPage : UserControl
  {
    public StreamRunningPage(StreamRunningPageViewModel viewModel)
    {
      InitializeComponent();

      this.DataContext = viewModel;
    }
  }
}
