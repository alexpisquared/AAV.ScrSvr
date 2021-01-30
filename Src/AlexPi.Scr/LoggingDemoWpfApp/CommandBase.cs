using System;
using System.Windows.Input;

namespace LoggingDemoWpfApp
{
  internal class CommandBase : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => throw new NotImplementedException();
    public virtual void Execute(object parameter) => throw new NotImplementedException();
  }
}