using OneClickStream.ViewModels;
using Prism.Regions;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace OneClickStream.Views
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

    private StreamlabsViewModel viewModel
    {
      get { return this.DataContext as StreamlabsViewModel; }
      set { this.DataContext = value; }
    }

    #endregion Properties

    #region Methods

    public static void SetSilent(WebBrowser browser, bool silent)
    {
      if (browser == null)
        throw new ArgumentNullException("browser");

      // get an IWebBrowser2 from the document
      IOleServiceProvider sp = browser.Document as IOleServiceProvider;
      if (sp != null)
      {
        Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

        object webBrowser;
        sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
        if (webBrowser != null)
        {
          webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
        }
      }
    }

    private void NavigateBrowserTo(string videoSource)
    {
      var html = Properties.Resources.AzureMediaPlayer;
      html = html.Replace("{videoSource}", videoSource);
      this.browser.NavigateToString(html);
      SetSilent(browser, true);
    }

    private void UserControlDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
      var newViewModel = (StreamlabsViewModel)e.NewValue;

      newViewModel.PropertyChanged += this.ViewModelPropertyChanged;
    }

    private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(StreamlabsViewModel.PreviewSource))
      {
        NavigateBrowserTo(this.viewModel.PreviewSource);
      }
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      this.viewModel.Initialize();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
      this.browser.NavigateToString("<body/>");
    }

    #endregion Methods
  }
}