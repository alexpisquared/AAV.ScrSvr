#if RegistryPkgAdded
using Microsoft.Win32;
#endif
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
///  from Ofc at 
///   C:\Temp\x\WpfApp1\ConsoleApp1\RegEditScrSvrNow.cs
/// </summary>

namespace AAV.Sys.Helpers
{
  public class RegEditScrSvrNow
  {
    public static void Go()
    {
#if RegistryPkgAdded
      var _key = @"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Control Panel\Desktop";
      Registry.SetValue(_key, "SCRNSAVE.EXE", "C:\\Users\\alex.pigida\\OneDrive\\Public\\bin\\AAV.SS.scr");
      Registry.SetValue(_key, "SCRNSAVE.EXE`", "rundll32 user32.dll,LockWorkStation");
      Registry.SetValue(_key, "ScreenSaveActive", "1");
      Registry.SetValue(_key, "ScreenSaverIsSecure", "0");
      Registry.SetValue(_key, "ScreenSaveTimeOut", "59");
#endif

      var txt = new StringBuilder();

      var rv1 = NativeMethods.SendMessage(0xffff, 0x001a, 0, 1);
      Trace.WriteLine($" *** Result 1 = {rv1}");      
      var rv2 = NativeMethods.SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "", SMTO_ABORTIFHUNG, 100, IntPtr.Zero);
      Trace.WriteLine($" *** Result 2 = {rv2}");      
      var rv3 = NativeMethods.SendMessageTimeoutW(HWND_BROADCAST, WM_SETTINGCHANGE, 0, txt, SMTO_ABORTIFHUNG, 100, 0);
      Trace.WriteLine($" *** Result 3 = {rv3}");

      //dMessageTimeoutW(HWND_BROADCAST, WM_SETTINGCHANGE, 0, (LPARAM)L"Environment", SMTO_ABORTIFHUNG, 5000, &dwReturnValue);
    }


    static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
    const int WM_SETTINGCHANGE = 0x1a;
    const int SMTO_ABORTIFHUNG = 0x0002;

    const uint SPI_SETDESKWALLPAPER = 20;
    const uint SPIF_UPDATEINIFILE = 0x1;
    int SetImage(string filename) => NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename, SPIF_UPDATEINIFILE);



    static string GetText(IntPtr hwnd) { var text = new StringBuilder(1024); return NativeMethods.SendMessageTimeoutW(hwnd, 0xd, 1024, text, 0x2, 1000, 0) != 0 ? text.ToString() : "__not sure__"; }
  }
}
