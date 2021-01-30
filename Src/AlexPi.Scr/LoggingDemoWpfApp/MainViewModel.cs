namespace LoggingDemoWpfApp
{
  internal class MainViewModel
  {
    private MakeSandwichCommand makeSandwichCommand;

    public MainViewModel(MakeSandwichCommand makeSandwichCommand) => this.makeSandwichCommand = makeSandwichCommand;
  }
}