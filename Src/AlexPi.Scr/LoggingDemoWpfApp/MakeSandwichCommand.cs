using Microsoft.Extensions.Logging;
using System;
using System.Windows;

namespace LoggingDemoWpfApp
{
  internal class MakeSandwichCommand : CommandBase
  {
    private ILogger<MakeSandwichCommand> _makeSandwichCommandLogger;

    public MakeSandwichCommand(ILogger<MakeSandwichCommand> makeSandwichCommandLogger) => _makeSandwichCommandLogger = makeSandwichCommandLogger;

    public override void Execute(object parameter)
    {
      _makeSandwichCommandLogger.LogInformation("Creating a sandwich");

      MessageBox.Show("Made", "Done");

      _makeSandwichCommandLogger.LogInformation("Created a sandwich!!!");
    }
  }
}

//todo: 15 jan t.Alla birthday
