using SimpleStream.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace SimpleStreamWpf
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private static UnityContainer container = new UnityContainer();

    public App()
    {
      this.InitializeContainer(container);
    }

    private void InitializeContainer(UnityContainer container)
    {
      container.RegisterSingleton<AzureStreamService>();
      container.RegisterSingleton<ConsoleWraper>();
    }

    internal static T Resolve<T>()
    {
      return container.Resolve<T>();
    }
  }
}
