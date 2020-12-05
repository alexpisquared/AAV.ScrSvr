using AAV.Sys.Ext;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace AAV.WPF.Helpers
{
  public static class UnhandledExceptionHndlr // Core 3
  {
    public static void OnCurrentDispatcherUnhandledException(object s, DispatcherUnhandledExceptionEventArgs ex)
    {
      if (ex != null)
        ex.Handled = true;

      string _header = $"Current Dispatcher Unhandled Exception - {DateTimeOffset.Now: HH:mm:ss}";
      var innerMsgs = ex?.Exception.InnerMessages();

      try
      {
        Trace.Write($"{DateTimeOffset.Now:yy.MM.dd HH:mm:ss}> CurrentDispatcherUnhandledException: s: {s?.GetType().Name}. {innerMsgs}");
        Clipboard.SetText(innerMsgs);
#if Speakable
        new System.Speech.Synthesis.SpeechSynthesizer().SpeakAsync($"Oopsee... {imex.Message}");
#endif
        if (Debugger.IsAttached)
        {
          Debugger.Break(); //seems like always true: if (ex is System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)					Bpr.BeepEr();				else 
        }
        else if (MessageBox.Show($"An error occurred in this app...\n\n ...{innerMsgs}\n\nDo you want to continue?", _header, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes) == MessageBoxResult.No)
        {
          Trace.WriteLine("Decided NOT to continue: Application.Current.Shutdown();");
          Application.Current.Shutdown(44);
        }
      }
      catch (Exception fatalEx)
      {
        var msg = $"An error occurred while reportikng an error ...\n\n ...{fatalEx.Message}...\n\n ...{innerMsgs}";

        Environment.FailFast(msg, fatalEx); //tu: http://blog.functionalfun.net/2013/05/how-to-debug-silent-crashes-in-net.html // Capturing dump files with Windows Error Reporting: Db a key at HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\Windows Error Reporting\LocalDumps\[Your Application Exe FileName]. In that key, create a string value called DumpFolder, and set it to the folder where you want dumps to be written. Then create a DWORD value called DumpType with a value of 2.
        MessageBox.Show(msg, _header);

        throw;
      }
    }
  }
}