using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;

///2021-01-30   https://www.youtube.com/watch?v=SfTdUNuApYc
/// a refresher on how to bolt in a [seri]logger to a WPF app in the simplest way
/// (see IHost part for DI way)

namespace LoggingDemoWpfApp
{
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      var loggerFactory = LoggerFactory.Create(builder =>
      {
        var loggerConfiguration = new LoggerConfiguration()
          .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
          .MinimumLevel.Information();

        builder.AddSerilog(loggerConfiguration.CreateLogger());
      });

      var makeSandwichCommandLogger = loggerFactory.CreateLogger<MakeSandwichCommand>();

      var makeSandwichCommand = new MakeSandwichCommand(makeSandwichCommandLogger);

      MainWindow = new MainWindow()
      {
        DataContext = new MainViewModel(makeSandwichCommand)
      };
      MainWindow.Show();

      base.OnStartup(e);
    }
  }
}
