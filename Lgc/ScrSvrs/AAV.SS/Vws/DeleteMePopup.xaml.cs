using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AAV.SS.Vws
{
    public partial class DeleteMePopup : AAV.WPF.Base.WindowBase
  {
        public string MediaFile { get; }

        public DeleteMePopup(string mediaFile)
        {
            InitializeComponent();
            tbMediaFile.Text = MediaFile = mediaFile;
            me1.Source = new Uri(mediaFile);
        }

        void onDelete(object s, RoutedEventArgs e)
        {
            try
            {
                me1.LoadedBehavior = MediaState.Manual;
                me1.Stop();
                me1.Close();
                File.Delete(MediaFile);
                tbMediaFile.Foreground = new SolidColorBrush(Colors.Gray);
                onCancel(s, e);
            }
            catch (Exception ex)
            {
                tbMediaFile.Foreground = new SolidColorBrush(Colors.Orange);
                tbMediaFile.Text = ex.Message;
            }
        }

        void onCancel(object s, RoutedEventArgs e) => base.Hide();
        void on_MediaOpened(object s, /**/     RoutedEventArgs e) => Debug.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media Loaded : {e}");
        void on_MediaEnded(object s,  /**/     RoutedEventArgs e) => Debug.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media Ended  : {e}");
        void on_MediaFailed(object s, ExceptionRoutedEventArgs e) => Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media Failed : {e.ErrorException.Message} \t{((MediaElement)s)?.Source?.LocalPath}");
    }
}
