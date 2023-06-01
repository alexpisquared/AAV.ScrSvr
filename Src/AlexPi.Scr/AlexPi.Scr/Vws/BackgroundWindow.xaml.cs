using System.Windows.Forms;

namespace AlexPi.Scr.Vws;

public partial class BackgroundWindow : UnCloseableWindow
{
  public BackgroundWindow(AlexPi.Scr.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
  {
    InitializeComponent();
    DataContext = this;
    Name = "ths";
  }
  public void ShowOnTargetScreen(Screen trgScreen, bool showMaximized)
  {
    //Task.Run(async () => await Task.Delay(waitMs)).ContinueWith(_ =>  {
    Show();
    Left = trgScreen.Bounds.Left;
    Top = trgScreen.Bounds.Top;
    Title = $" {string.Join(" ", Environment.GetCommandLineArgs())} \t\t {trgScreen.DeviceName} - {(trgScreen.Primary ? "Primary  " : "Secondary")}   XY: {trgScreen.Bounds.X,5} x {trgScreen.Bounds.Y,-5} \t\t {VersionHelper.CurVerStr("")}"; // always NaN / 0: ► Left-Top: {(double.IsNaN(window.Left) ? -1.0 .Left)}-{(double.IsNaN(window.Top) ? -1.0 .Top)}   Actual W x H: {window.ActualWidth}x{window.ActualHeight}" +

    if (showMaximized)
    {
      WindowState = WindowState.Maximized;
    }
    else
    {
      WindowState = WindowState.Normal;
      Width = trgScreen.Bounds.Width;
      Height = 50; //  trgScreen.Bounds.Height / 32;
      //Background = Brushes.DarkSlateGray;
      Top = trgScreen.Bounds.Top + 40; // task bar considerations.
      //Opacity = .75;
    }

    //}, TaskScheduler.FromCurrentSynchronizationContext());

    Debug.WriteLine($"  {Title.Replace("\r", " ").Replace("\n", " ")}");
  }

  void onShow(object s, System.Windows.Input.MouseEventArgs e) => ((TextBlock)s).Opacity = 1;
  void onHide(object s, System.Windows.Input.MouseEventArgs e) => ((TextBlock)s).Opacity = 0;
  void TextBlock_MouseLeftButtonDown(object s, MouseButtonEventArgs e) { WindowState = WindowState.Normal; Left = Top = Height = Width = 200; }

  //? public BackgroundWindow() : this(new AlexPi.Scr.Logic.GlobalEventHandler()) { } // The type 'UnCloseableWindow' cannot have a Name attribute. Value types and types without a default constructor can be used as items within a ResourceDictionary
}

// useles fun trick: Topmost=true;  Loaded += async (s, e) => { await Task.Delay(10000); Topmost = false; };