using AAV.SS.Vws;
using AsLink;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AAV.SS
{
  public partial class SlideShowWindow : TopmostUnCloseableWindow
  {
    readonly PerformanceCounter
            _perfCountAA = new PerformanceCounter("Memory", "Available MBytes"), // new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "AAV.SS.scr"),
            _perfCountEx = new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "Explorer"),
            _perfCounCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total"),
            //_perfCounGPU = new PerformanceCounter("GPU", "% GPU Time", "_Total"), // % GPU Time-Base - https://github.com/Alexey-Kamenev/GpuPerfCounters/blob/master/src/GpuPerfCounters/PerfCounterService.cs
            _ramCounterr = new PerformanceCounter("Memory", "Available MBytes");
    public const int AnimDurnInSec = 2,      //5 * 60, // !! Keep in synch with xaml storyboards!!
       _maxPicsToShow = 40;                   //40;     // 10 min
    bool _isActive;
    System.Windows.Point mousePosition;

    //List<string> _images = null;
    readonly Random _rnd = new Random();
    readonly Storyboard _sb1;
    double sum;
    int cnt;

    public SlideShowWindow()
    {
      InitializeComponent();
      scrtm1.Visibility = scrtm2.Visibility = scrtm3.Visibility = System.Windows.Forms.Screen.AllScreens.Length > 1 ? Visibility.Collapsed : Visibility.Visible;
      Closed += (s, e) => { App.LogScrSvrUptime("ScrSvr - Dn - SlideShowWindow.Closed()"); Application.Current.Shutdown(); };
      //MouseMove += closeOnMouseMove;
      KeyUp += onKey; // PreviewKeyDown += onKey;
      Closing += Window_Closing;
      _sb1 = (FindResource("_sb1") as Storyboard);

#if !DEBUG
      //TransitionTime =  AppSettings.Instance.TransitionTime;
      //CrossFadeTime =  AppSettings.Instance.CrossFadeTime;
#endif

      new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, new EventHandler((s, e) => updateClock()), Dispatcher.CurrentDispatcher).Start(); //tu: prevent screensaver; //tu: one-line timer
      new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Background, new EventHandler((s, e) => measure_Cpu()), Dispatcher.CurrentDispatcher).Start(); //tu: prevent screensaver; //tu: one-line timer

      Loaded += onLoaded;
    }

    async void onLoaded(object s, RoutedEventArgs e)
    {
      //?await System.Threading.Tasks.Task.Delay(10000);

      try
      {
        Cursor = Cursors.Pen;


        tbCurVer.Text = $"{VerHelper.CurVerStr()} {(VerHelper.IsVIP ? '+' : '!')}vip ";

        //var imgPath = AppSettings.Instance.ImgPath;
        //if (imgPath == "null" || !Directory.Exists(imgPath))
        //    imgPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ScrSvr");
        //if (!Directory.Exists(imgPath))
        //{
        //    Directory.CreateDirectory(imgPath);
        //    //Process.Start("Explorer", imgPath);
        //    //MessageBox.Show(string.Format("The folder is just created here:\n\n{0}\n\n. Fill it in with pictures of your choice now!", imgPath), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    //{ Close (); System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss.f} => ::>Application.Current.Shutdown();a"); Application.Current.Shutdown(); }
        //}

        //string[] files = null;
        //if (System.IO.Directory.Exists(imgPath)) files = System.IO.Directory.GetFiles(imgPath); //else        {          MessageBox.Show("The folder you selected does not exist, please select a new one!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);          { Close (); System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss.f} => ::>Application.Current.Shutdown();b"); Application.Current.Shutdown(); }        }

        ////if (files != null && files.Length <= 0)        {          MessageBox.Show(string.Format("The folder is empty:\n\n{0}\n\n. Fill it in with pictures of your choice and restart!", imgPath), "Uh-oh", MessageBoxButton.OK, MessageBoxImage.Error);          { Close (); System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss.f} => ::>Application.Current.Shutdown();c"); Application.Current.Shutdown(); }          return;        }

        //_images = getImageList(files);
        //if (_images.Count <= 0)
        //{
        //    //MessageBox.Show(string.Format("The folder is empty:\n\n{0}\n\n. Fill it in with pictures of your choice and restart!", imgPath), "Uh-oh", MessageBoxButton.OK, MessageBoxImage.Error); { Close (); System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss.f} => ::>Application.Current.Shutdown();d"); Application.Current.Shutdown(); }
        //    return;
        //}

        ////_tmr.Interval = TimeSpan.FromSeconds(AnimDurnInSec * 2);
        ////_tmr.Tick += new EventHandler(tick);
        ////_tmr.Start();


        //swap(true);
      }
      finally
      {
        await App.SleepLogic();
      }
    }

    void Button_Click(object sender, RoutedEventArgs e) => Close();

    void closeOnMouseMove(object s, MouseEventArgs e)
    {
      System.Windows.Point currentPosition = e.MouseDevice.GetPosition(this);
      if (!_isActive) // Set IsActive and MouseLocation only the first time this event is called.
      {
        mousePosition = currentPosition;
        _isActive = true;
      }
      else // If the mouse has moved significantly since first call, close.
      {
        if ((Math.Abs(mousePosition.X - currentPosition.X) > 50) ||
            (Math.Abs(mousePosition.Y - currentPosition.Y) > 50))
        {
          { Close(); /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss.f} => ::>Application.Current.Shutdown();7");*/ Application.Current.Shutdown(); }
        }
      }
    }

    void onKey(object s, KeyEventArgs e)
    {
      switch (e.Key)
      {
        default: e.Handled = false; return;
        case Key.Up: { Close();     /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss.f} => ::>Application.Current.Shutdown();8");*/ Application.Current.Shutdown(); } break;
        case Key.Escape: { Close(); /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss.f} => ::>Application.Current.Shutdown();9");*/ Application.Current.Shutdown(); } break;
        case Key.F4: onSetttings(); break;
        case Key.PageUp:
        case Key.Left: swap(false); break;
        case Key.PageDown:
        case Key.Right:
        case Key.Down: swap(true); break;
          //case Key.Space: _tmr.IsEnabled = !_tmr.IsEnabled; break;
      }

      e.Handled = true;
    }

    void Window_Closing(object s, System.ComponentModel.CancelEventArgs e) => App.LogScrSvrUptime("ScrSvr - Dn - slideShow_Closing.Closed!");

    void onManualUpdateRequested(object s, RoutedEventArgs e) { ((Button)s).Visibility = Visibility.Collapsed; App.Synth.SpeakAsync($"{Vws.EvDbLogSsvrHelper.UpdateEvLogToDb(15)} events found."); }

    void measure_Cpu()
    {
      try
      {
        double k = 1.8;
        float p = _perfCounCPU.NextValue();
        double v = k * p - 90;
        gaugeTorCPU.InnerVal/*Anim*/ = (sum += v) / (++cnt);
        gaugeTorCPU.MiddlValAnim = v;
        if (gaugeTorCPU.OuterVal < v)
          gaugeTorCPU.OuterVal/*Anim*/ = v;
        gaugeTorCPU.GaugeText = $"{p:N0}\r\n{((gaugeTorCPU.InnerVal + 90) / k):N0} - {((gaugeTorCPU.OuterVal + 90) / k):N0}";

        p = _perfCountEx.NextValue() / 1000000; // ~100 Mb on Asus2
        ao1.GaugeText = $"{p:N0}";
        ao1.MiddlValAnim = 2.5 * p - 150;

        p = _perfCountAA.NextValue() / 1000; // ~6,000 Mb on Asus2
        ao2.GaugeText = $"{p:N2}";
        ao2.MiddlValAnim = 37.5 * p - 150;

        //p = _perfCounGPU.NextValue(); // exception - Category does not exist.
        //ao2.GaugeText = $"{p}";
        //ao2.MiddlValAnim = 37.5 * p - 150;
      }
      catch (Exception ex) { ex.Log($"_perfCountAA.InstanceName: {_perfCountAA.InstanceName}"); }
    }

    void updateClock()
    {
      DateTime tnow = DateTime.Now;
      TimeSpan idle = ((tnow - App.Started) + TimeSpan.FromSeconds(App.Ssto));
      TimeSpan left = App.Started.AddMinutes(AppSettings.Instance.Min2Sleep) - tnow;
      tbOutOff.Text = (idle.TotalMinutes < 60 ? $"{idle:mm\\:ss}" : $"{idle:h\\:mm} ") + (AppSettings.Instance.AutoSleep ? $"\t\t\t\t {((left)):h\\:mm\\:ss}" : "");
      if (AppSettings.Instance.AutoSleep)
      {
        pbOutOff.Maximum = (idle + left).TotalMinutes;
        pbOutOff.Value = idle.TotalMinutes;
      }
    }

    List<string> getImageList(string[] files)
    {
      List<string> images = new List<string>();
      files.Where(r => new string[] { "jpg", "bmp", "jpeg", "png", "tiff", "tif", "gif" }.Contains(System.IO.Path.GetExtension(r).ToLower().Replace(".", ""))).ToList().ForEach(file => images.Add(file));
      return images;
    }

    void onSetttings(object s = null, RoutedEventArgs e = null) => new SettingsWindow().ShowDialog();

    //void swapImage_(object s, EventArgs e)    {      swap(true);      if (--_maxPicsToShow < 0) { _imgSwapper.Stop(); tbMin2Sleep.Visibility = Visibility.Visible; }    }
    void swap(bool fwd)
    {
      //img2.Source = img1.Source;
      //img1.Source = new BitmapImage(new Uri(_images[_rnd.Next(_images.Count)], UriKind.RelativeOrAbsolute));
      //see cpu gain : _sb1.Begin();
    }
  }
}
