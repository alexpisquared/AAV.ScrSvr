using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AAV.SS.UsrCtrls
{
  public partial class DeleteFileUsrCtrl : UserControl
  {
    //blic delegate void EventHandler(object sender, EventArgs e);    //A: provided by framework!!!
    public event EventHandler ClosedA;                                //A:
    public delegate void ClosedB_handler(string msg, int i, bool b);  //B: provided by ...us.
    public event ClosedB_handler ClosedB;                             //B:
    public Action<string, int, bool> ClosedC;                         //C:    //tu: modern/functional approach to delegate. (event delegate action)

    string[] _allFiles;
    int _curIndex;

    public DeleteFileUsrCtrl() => InitializeComponent();

    void btnDelt_Click(object s, RoutedEventArgs e)
    {
      me1.Stop();
      me1.Source = null;

      if (!File.Exists(_allFiles[_curIndex]))
      {
        tbbr.Text = "!Exists!";
      }
      else
      {
        if (MessageBox.Show(_allFiles[_curIndex], "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
          File.Delete(_allFiles[_curIndex]);
        else
          tbbr.Text = "Good.";
      }

      Debug.WriteLine($"*** Deleted: {_allFiles[_curIndex]}");
    }
    void btnBack_Click(object s, RoutedEventArgs e)
    {
      Visibility = Visibility.Collapsed;
      ClosedA.Invoke(s, e);
      ClosedB("Awesome B", 7, true);
      ClosedC("Awesome C", 7, true);
    }

    void on_MediaOpened(object s, /**/     RoutedEventArgs e) { Debug.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media Loaded : {e}"); me1.Play(); }
    void on_MediaEnded(object s,  /**/     RoutedEventArgs e) { Debug.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media  Ended : {e}"); me1.Position = TimeSpan.Zero; me1.Play(); }
    void on_MediaFailed(object s, ExceptionRoutedEventArgs e) => Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media Failed : {e.ErrorException.Message} \t{((MediaElement)s)?.Source?.LocalPath}");

    void btnPrev_Click(object s, RoutedEventArgs e) => iterate(false);
    void btnNext_Click(object s, RoutedEventArgs e) => iterate(true);

    void iterate(bool isNext)
    {
      if ((!isNext && _curIndex < 1) || (isNext && _curIndex >= _allFiles.Length - 1)) return;

      var curFileN = _allFiles[isNext ? ++_curIndex : --_curIndex];
      me1.Source = new Uri(curFileN); me1.Play();

      tbbr.Text = $"{_curIndex,6:N0} / {_allFiles.Length:N0}";
      tbtr.Text = $"{System.IO.Path.GetDirectoryName(_allFiles[_curIndex])} \r\n{System.IO.Path.GetFileName(_allFiles[_curIndex])} \r\n";                    //tbtl.Text = $"{f.FileCreated:yyyy-MM-dd  HH}";
    }

    internal void ShowEditorPanel(string[] allFiles, int curIndex)
    {
      me1.Stop();
      Visibility = Visibility.Visible;
      _allFiles = allFiles;
      _curIndex = curIndex;

      me1.Source = new Uri(_allFiles[_curIndex]);
      me1.Play();
    }
  }
}