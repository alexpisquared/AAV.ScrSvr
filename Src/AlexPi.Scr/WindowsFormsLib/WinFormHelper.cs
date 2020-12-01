using System;
using System.Windows.Forms;

namespace WindowsFormsLib
{
  public class WinFormHelper { public static Screen[] GetAllScreens() => Screen.AllScreens; }
  static class Program {[STAThread] static void Main() { } }
}
