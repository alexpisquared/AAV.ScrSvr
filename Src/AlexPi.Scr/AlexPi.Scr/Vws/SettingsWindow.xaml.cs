using AAV.Sys.Helpers;
using AAV.WPF.AltBpr;
using AsLink;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace AlexPi.Scr
{
  public partial class SettingsWindow : Window
  {
    bool _manual = false;
    readonly NotifyIcon _ni = new();
    readonly BrowseForFolderDialog _dlg = new();
    readonly DispatcherTimer _timer = new();
    double _delay;
    int _seconds;

    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public SettingsWindow()
    {
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((s, e) => { if (e.IsTerminating) System.Windows.MessageBox.Show(e.ExceptionObject.ToString()); });

      InitializeComponent();

      tbCurVer.Text = $"{VerHelper.CurVerStr(".Net6")}";

      _delay = AppSettings.Instance.DelayMin;
      TransitionSlider.Value = AppSettings.Instance.TransitionSec;
      CrossFadeSlider.Value = AppSettings.Instance.CrossFadeSec;
      DelaySlider.Value = _delay;
      if (AppSettings.Instance.ImgPath != "null")
        txtPath.Text = AppSettings.Instance.ImgPath;
      else
        txtPath.Text = "";

      UpdateSeconds();

      _timer.Interval = TimeSpan.FromSeconds(1);
      _timer.Tick += new EventHandler(timer_Tick);
      _timer.Start();



      /* //tu: icon to/from resources
             * 1 - add the icon to the project resources and rename to icon.
             * 2 - open the designer of the form you want to add the icon to.
             * 3 - append the InitializeComponent function.
            */
      //_ni.Icon = AlexPi.Scr.Properties.Resources.icon; //tu: ni.Icon = new Icon(@"C:\c\Lgc\ScrSvrs\AlexPi.Scr\icon.ico"); //todo: unhardcode 			

      //_ni.Visible = true;
      //_ni.DoubleClick += delegate (object s, EventArgs args)
      //    {
      //      Show();
      //      WindowState = WindowState.Normal;
      //    };

      //var ctxTrayMenu = new ContextMenuStrip
      //{
      //  ShowImageMargin = false,
      //  ShowCheckMargin = false
      //};

      //var mnuSets = new ToolStripMenuItem { Text = "Settings" };
      //var mnuPrev = new ToolStripMenuItem { Text = "Preview" };
      //var mnuExit = new ToolStripMenuItem { Text = "Exit" };

      //mnuSets.Click += mnuSets_Click;
      //mnuPrev.Click += new EventHandler(mnuPrev_Click);
      //mnuExit.Click += new EventHandler(mnuExit_Click);

      //ctxTrayMenu.Items.Add(mnuSets);
      //ctxTrayMenu.Items.Add(mnuPrev);
      //ctxTrayMenu.Items.Add(new ToolStripSeparator());
      //ctxTrayMenu.Items.Add(mnuExit);
      //_ni.ContextMenuStrip = ctxTrayMenu;

      _dlg.Title = "Please select the folder where your images are stored :";
      _dlg.OKButtonText = "OK!";

      Hide();

      tbd.Text = $"CurDir: \t{Environment.CurrentDirectory}";
    }

    async void mnuPrev_Click(object s, EventArgs e) => await App.SpeakAsync("Retired piece."); //var ss = new SlideShowWindow();////??CurrentIsfNameShowDialog();

    void UpdateSeconds()
    {
      _delay = AppSettings.Instance.DelayMin;
      if (_delay == 0)
        _seconds = 30;
      else
        _seconds = Convert.ToInt32(_delay) * 60;
    }

    void mnuSets_Click(object s, EventArgs e) => Show();

    void mnuExit_Click(object s, EventArgs e)
    {
      _manual = true;
      { Close(); /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();2");*/ System.Windows.Application.Current.Shutdown(); }
    }

    protected override void OnStateChanged(EventArgs e)
    {
      if (WindowState == WindowState.Minimized)
        Hide();

      base.OnStateChanged(e);
    }

    void Window_Closing(object s, System.ComponentModel.CancelEventArgs e)
    {
      if (_manual)
      {
        //_ni.Visible = false;
      }
      else
      {
        e.Cancel = true;
        Hide();
      }
    }

    void DelaySlider_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e)
    {
      if (txtDelay != null && DelaySlider != null)
      {
        if (DelaySlider.Value == 0)
        {
          txtDelay.Text = "30 seconds";
          DelaySlider.Tag = 0;
        }
        else
        {
          if (Math.Round(DelaySlider.Value, 0) >= 1)
          {
            txtDelay.Text = Math.Round(DelaySlider.Value, 0).ToString() + " minutes";
            DelaySlider.Tag = Math.Round(DelaySlider.Value, 0);
          }
        }
      }
    }

    void Button_Click(object s, RoutedEventArgs e)
    {
      if (true == _dlg.ShowDialog(this))
      {
        txtPath.Text = _dlg.SelectedFolder;
      }
    }



    async void timer_Tick(object s, EventArgs e)
    {
      var lastinput = GetLastInputTime();
      if (lastinput >= _seconds)
      {
        _timer.Stop();
        await App.SpeakAsync("Retired piece #2."); //var ss = new SlideShowWindow(); ////??CurrentIsfNameShowDialog();
        _timer.Start();
      }
      Console.WriteLine(lastinput.ToString() + "/" + _seconds.ToString() + "secondes");

    }


    void onDone(object s, RoutedEventArgs e)
    {
      if (txtPath.Text == "")
        AppSettings.Instance.ImgPath = "null";
      else
        AppSettings.Instance.ImgPath = txtPath.Text;

      AppSettings.Instance.DelayMin = DelaySlider.Value;
      AppSettings.Instance.CrossFadeSec = Math.Round(CrossFadeSlider.Value);
      AppSettings.Instance.TransitionSec = Math.Round(TransitionSlider.Value);
      AppSettings.Instance.Save();

      UpdateSeconds();
      { Close(); /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();3");*/ System.Windows.Application.Current.Shutdown(); }
      App.Current.Shutdown();
    }

    void onClose(object s, RoutedEventArgs e)
    {
      //_ni.Visible = false;
      //_ni.Icon = null;
      //_ni.Dispose();
      { Close(); /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();1");*/ System.Windows.Application.Current.Shutdown(); }
      App.Current.Shutdown();
    }
    void onCurDir(object s, RoutedEventArgs e) => Process.Start("Explorer.exe", Environment.CurrentDirectory);
    void onIsoDir(object s, RoutedEventArgs e) => Process.Start("Explorer.exe", Environment.CurrentDirectory); //todo: no go for .net core 3: all are ""!IsoHelper.GetIsoFolder());
    void onIsoFil(object s, RoutedEventArgs e) => Process.Start("Explorer.exe", System.IO.IsolatedStorage.IsolatedStorageFile.GetStore(System.IO.IsolatedStorage.IsolatedStorageScope.User | System.IO.IsolatedStorage.IsolatedStorageScope.Assembly, null, null).ToString());
    void onAavNewLog(object s, RoutedEventArgs e) => Title = EvLogHelper.CheckCreateLogChannel();

    static int GetLastInputTime()
    {
      var idleTime = 0;
      var lastInputInfo = new LASTINPUTINFO();
      lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
      lastInputInfo.dwTime = 0;

      var envTicks = Environment.TickCount;

      if (GetLastInputInfo(ref lastInputInfo))
      {
        var lastInputTick = Convert.ToInt32(lastInputInfo.dwTime);

        idleTime = envTicks - lastInputTick;
      }
      var toret = ((idleTime > 0) ? (idleTime / 1000) : 0);

      Console.WriteLine("Idle time: " + idleTime.ToString());
      Console.WriteLine(toret.ToString());
      return toret;
    }

    async void onFreqWalk(object s, RoutedEventArgs e) => await ChimerAlt.FreqWalkUpDn();
  }

  [StructLayout(LayoutKind.Sequential)]
  struct LASTINPUTINFO
  {
    public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

    [MarshalAs(UnmanagedType.U4)]
    public int cbSize;
    [MarshalAs(UnmanagedType.U4)]
    public uint dwTime;
  }

}
