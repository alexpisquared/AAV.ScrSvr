using AAV.Sys.Helpers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace AlexPi.Scr.Vws
{
  public partial class BackgroundWindow : UnCloseableWindow
  {
    public BackgroundWindow(AlexPi.Scr.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      InitializeComponent();
      DataContext = this;
      Name = "ths";
    }
    public void ShowOnTargetScreen(Screen trgScreen/*, int waitMs*/)
    {
      //Task.Run(async () => await Task.Delay(waitMs)).ContinueWith(_ =>  {
      Show();
      Left = trgScreen.Bounds.Left;
      Top = trgScreen.Bounds.Top;
      Title = $" {string.Join(" ", Environment.GetCommandLineArgs())} \t\t {trgScreen.DeviceName} - {(trgScreen.Primary ? "Primary  " : "Secondary")}   XY: {trgScreen.Bounds.X,5} x {trgScreen.Bounds.Y,-5} \t\t {(VerHelper.IsVIP ? "VIP  :)" : "!vip   :(")}      {VerHelper.CurVerStr(".Net 5.0")}"; // always NaN / 0: ► Left-Top: {(double.IsNaN(window.Left) ? -1.0 : window.Left)}-{(double.IsNaN(window.Top) ? -1.0 : window.Top)}   Actual W x H: {window.ActualWidth}x{window.ActualHeight}" +
#if DEBUG
      WindowState = WindowState.Normal;
      Width = trgScreen.Bounds.Width / 10;
      Height = trgScreen.Bounds.Height / 10;
      Background = Brushes.Teal;
      Opacity = .5;
#else
      WindowState = WindowState.Maximized;
#endif
      //}, TaskScheduler.FromCurrentSynchronizationContext());

      Debug.WriteLine($"  {Title.Replace("\r", " ").Replace("\n", " ")}");
    }

    void onShow(object s, System.Windows.Input.MouseEventArgs e) => ((TextBlock)s).Opacity = 1;
    void onHide(object s, System.Windows.Input.MouseEventArgs e) => ((TextBlock)s).Opacity = 0;
    void TextBlock_MouseLeftButtonDown(object s, MouseButtonEventArgs e) { WindowState = WindowState.Normal; Left = Top = Height = Width = 200; }

    //? public BackgroundWindow() : this(new AlexPi.Scr.Logic.GlobalEventHandler()) { } // The type 'UnCloseableWindow' cannot have a Name attribute. Value types and types without a default constructor can be used as items within a ResourceDictionary
  }
}

// useles fun trick: Topmost=true;  Loaded += async (s, e) => { await Task.Delay(10000); Topmost = false; };