using AAV.Sys.Ext;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AAV.WPF.Ext
{
  public static class ExnPopr
  {
    public static void Pop(this Exception ex, string optl = null, [CallerMemberName] string cmn = "", [CallerFilePath] string cfp = "", [CallerLineNumber] int cln = 0)
    {
      var msgForPopup = ex.Log(optl);

      if (Debugger.IsAttached)
        Debugger.Break();
      else
        new AAV.WPF.View.ExceptionPopup(ex, optl, cmn, cfp, cln).ShowDialog(); //MessageBox.Show($"{cfp}({cln}): {cmn}()\r\n{(optl?.Length > 0 ? optl + "\r\n" : "")}\n{ex.InnerMessages()}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }
}
