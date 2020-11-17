using AAV.Sys.Helpers;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AAV.Sys.Ext
{
  public static class ExnLogr // the one and only .net core 3 (Dec2019)
  {
    public static TraceSwitch AppTraceLevelCfg => new TraceSwitch("CfgTraceLevelSwitch", "Switch in config file:  <system.diagnostics><switches><!--0-off, 1-error, 2-warn, 3-info, 4-verbose. --><add name='CfgTraceLevelSwitch' value='3' /> ");

    public static string Log(this Exception ex, string? optl = null, [CallerMemberName] string? cmn = "", [CallerFilePath] string cfp = "", [CallerLineNumber] int cln = 0)
    {
      //r msgForPopup = $"{ex?.GetType().Name} at {cfp}({cln}): {cmn}() {optl}\r\n{ex?.InnerMessages()}" ?? "Never null";
      var msgForPopup = $"{ex?.InnerMessages()}\r\n{ex?.GetType().Name} at {cfp}({cln}): {cmn}() {optl}";

      Trace.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.f} ██ {msgForPopup.Replace("\n", "  "/*, StringComparison.Ordinal*/).Replace("\r", "  "/*, StringComparison.Ordinal*/)}"); // .TraceError just adds the "ProgName.exe : Error : 0" line <= no use.

      traceStackTraceIfVerbose(ex);

      if (Debugger.IsAttached)
        Debugger.Break();
      else
        Bpr.ErrorAsync();

      //todo: catch (Exception fatalEx) { Environment.FailFast("An error occured whilst reporting an error.", fatalEx); }//tu: http://blog.functionalfun.net/2013/05/how-to-debug-silent-crashes-in-net.html //tu: Capturing dump files with Windows Error Reporting: Db a key at HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\Windows Error Reporting\LocalDumps\[Your Application Exe FileName]. In that key, create a string value called DumpFolder, and set it to the folder where you want dumps to be written. Then create a DWORD value called DumpType with a value of 2.

      return msgForPopup;
    }

    static void traceStackTraceIfVerbose(Exception? ex)
    {
      if (ExnLogr.AppTraceLevelCfg.TraceVerbose)
      {
        var prevLv = Trace.IndentLevel;
        var prevSz = Trace.IndentSize;
        Trace.IndentLevel = 2;
        Trace.IndentSize = 2;
        Trace.WriteLineIf(ExnLogr.AppTraceLevelCfg.TraceVerbose, ex?.StackTrace);
        Trace.IndentLevel = prevLv;
        Trace.IndentSize = prevSz;
      }
    }

    public static string InnerMessages(this Exception? ex, char delimiter = '\n')
    {
      var sb = new StringBuilder();
      while (ex != null)
      {
        sb.Append($" - {ex.Message}{delimiter}");
        ex = ex.InnerException;
      }

      return sb.ToString();
    }
    public static string InnermostMessage(this Exception ex)
    {
      while (ex != null)
      {
        if (ex.InnerException == null)
          return ex.Message;

        ex = ex.InnerException;
      }

      return "This is very-very odd.";
    }

    #region Proposals - cop
    #endregion
  }
}
