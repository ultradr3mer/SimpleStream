using SimpleStream.ViewModels;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStreamWpf.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    #region Constructors

    public MainWindow()
    {
      InitializeComponent();
      this.ViewModel = App.Resolve<MainViewModel>();
      this.ViewModel.PropertyChanged += this.ViewModelPropertyChanged;
    }

    #endregion Constructors

    #region Interfaces

    [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IOleServiceProvider
    {
      [PreserveSig]
      int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
    }

    #endregion Interfaces

    #region Properties

    public MainViewModel ViewModel { get { return this.DataContext as MainViewModel; } set { this.DataContext = value; } }

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

    private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(MainViewModel.BrowserUrl))
      {
        var html = Properties.Resources.Azure_Media_Player;
        html = html.Replace("{videoSource}", this.ViewModel.BrowserUrl);
        this.browser.NavigateToString(html);
        SetSilent(this.browser, true);
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      this.ViewModel.Unload();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      await this.ViewModel.Start();
    }

    #endregion Methods
  }
}