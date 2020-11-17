using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
  public class WinFormHelper
  {
    void lkjl()
    {
      foreach (var screen in GetAllScreens())
        Debug.WriteLine($"{screen}");
    }

    public static Screen[] GetAllScreens() => Screen.AllScreens;
  }
}