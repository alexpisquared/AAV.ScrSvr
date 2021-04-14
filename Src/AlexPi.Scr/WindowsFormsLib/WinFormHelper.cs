using System;
using System.Windows.Forms;

namespace WindowsFormsLib
{
  public class WinFormHelper
  {
    public static Screen[] GetAllScreens() => Screen.AllScreens;
    public static Screen PrimaryScreen => Screen.PrimaryScreen;
  }

  static class Program {[STAThread] static void Main___() { } }
}
