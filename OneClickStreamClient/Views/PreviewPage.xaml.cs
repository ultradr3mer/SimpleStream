using OneClickStreamClient.ViewModels;
using Prism.Regions;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace OneClickStreamClient.Views
{
  /// <summary>
  /// Interaction logic for PreviewPage.xaml
  /// </summary>
  public partial class PreviewPage : UserControl, INavigationAware
  {
    #region Constructors

    public PreviewPage(PreviewViewModel viewModel)
    {
      this.InitializeComponent();

      this.viewModel = viewModel;
    }

    #endregion Constructors

    #region Interfaces

    [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IOleServiceProvider
    {
      #region Methods

      [PreserveSig]
      int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);

      #endregion Methods
    }

    #endregion Interfaces

    #region Properties

    private PreviewViewModel viewModel
    {
      get { return this.DataContext as PreviewViewModel; }
      set { this.DataContext = value; }
    }

    #endregion Properties

    #region Methods

    public static void SetSilent(WebBrowser browser, bool silent)
    {
      if (browser == null)
      {
        throw new ArgumentNullException("browser");
      }

      IOleServiceProvider sp = browser.Document as IOleServiceProvider;
      if (sp != null)
      {
        Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

        sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out object webBrowser);
        if (webBrowser != null)
        {
          webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
        }
      }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
      this.browser.NavigateToString("<body/>");
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      this.viewModel.Initialize();
    }

    private void NavigateBrowserTo(string videoSource)
    {
      string html = Properties.Resources.AzureMediaPlayer;
      html = html.Replace("{videoSource}", videoSource);
      this.browser.NavigateToString(html);
      SetSilent(this.browser, true);
    }

    private void UserControlDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
      PreviewViewModel newViewModel = (PreviewViewModel)e.NewValue;

      newViewModel.PropertyChanged += this.ViewModelPropertyChanged;
    }

    private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(PreviewViewModel.PreviewSource))
      {
        this.NavigateBrowserTo(this.viewModel.PreviewSource);
      }
    }

    #endregion Methods
  }
}