using AsLink;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AlexPi.Scr.Logic
{
  public class GlobalEventHandler
  {
    public GlobalEventHandler() { }

    internal void TglContainerVis(CheckBox checkBox)
    {
      TglContainerVis(checkBox.Name, checkBox.IsChecked == false);
      checkBox.IsChecked = checkBox.IsChecked == false ? true : false;
    }

    internal void TglContainerVis(string name, bool isVisible)
    {
      var vis = isVisible ? Visibility.Visible : Visibility.Collapsed;
      switch (name)
      {
        case "ContainerA": ((App)Application.Current).CntrA.Visibility = vis; AppSettings.Instance.CtrlA = isVisible; break;
        case "ContainerB": ((App)Application.Current).CntrB.Visibility = vis; AppSettings.Instance.CtrlB = isVisible; break;
        case "ContainerC": ((App)Application.Current).CntrC.Visibility = vis; AppSettings.Instance.CtrlC = isVisible; break;
        case "ContainerD": ((App)Application.Current).CntrD.Visibility = vis; AppSettings.Instance.CtrlD = isVisible; break;
        case "ContainerE": ((App)Application.Current).CntrE.Visibility = vis; AppSettings.Instance.CtrlE = isVisible; break;
        case "ContainerF": ((App)Application.Current).CntrF.Visibility = vis; AppSettings.Instance.CtrlF = isVisible; break;
        case "ContainerG": ((App)Application.Current).CntrG.Visibility = vis; AppSettings.Instance.CtrlG = isVisible; break;
        case "ContainerH": ((App)Application.Current).CntrH.Visibility = vis; AppSettings.Instance.CtrlH = isVisible; break;
        case "ContainerI": ((App)Application.Current).CntrI.Visibility = vis; AppSettings.Instance.CtrlI = isVisible; break;
        case "ContainerJ": ((App)Application.Current).CntrJ.Visibility = vis; AppSettings.Instance.CtrlJ = isVisible; break;
        case "ContainerK": ((App)Application.Current).CntrK.Visibility = vis; AppSettings.Instance.CtrlK = isVisible; break;
        default: break;
      }

    }
    internal void TglContainerVis(string name)
    {
      switch (name)
      {
        case "ContainerA": AppSettings.Instance.CtrlA = !AppSettings.Instance.CtrlA; ((App)Application.Current).CntrA.Visibility = AppSettings.Instance.CtrlA ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerB": AppSettings.Instance.CtrlB = !AppSettings.Instance.CtrlB; ((App)Application.Current).CntrB.Visibility = AppSettings.Instance.CtrlB ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerC": AppSettings.Instance.CtrlC = !AppSettings.Instance.CtrlC; ((App)Application.Current).CntrC.Visibility = AppSettings.Instance.CtrlC ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerD": AppSettings.Instance.CtrlD = !AppSettings.Instance.CtrlD; ((App)Application.Current).CntrD.Visibility = AppSettings.Instance.CtrlD ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerE": AppSettings.Instance.CtrlE = !AppSettings.Instance.CtrlE; ((App)Application.Current).CntrE.Visibility = AppSettings.Instance.CtrlE ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerF": AppSettings.Instance.CtrlF = !AppSettings.Instance.CtrlF; ((App)Application.Current).CntrF.Visibility = AppSettings.Instance.CtrlF ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerG": AppSettings.Instance.CtrlG = !AppSettings.Instance.CtrlG; ((App)Application.Current).CntrG.Visibility = AppSettings.Instance.CtrlG ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerH": AppSettings.Instance.CtrlH = !AppSettings.Instance.CtrlH; ((App)Application.Current).CntrH.Visibility = AppSettings.Instance.CtrlH ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerI": AppSettings.Instance.CtrlI = !AppSettings.Instance.CtrlI; ((App)Application.Current).CntrI.Visibility = AppSettings.Instance.CtrlI ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerJ": AppSettings.Instance.CtrlJ = !AppSettings.Instance.CtrlJ; ((App)Application.Current).CntrJ.Visibility = AppSettings.Instance.CtrlJ ? Visibility.Visible : Visibility.Collapsed; break;
        case "ContainerK": AppSettings.Instance.CtrlK = !AppSettings.Instance.CtrlK; ((App)Application.Current).CntrK.Visibility = AppSettings.Instance.CtrlK ? Visibility.Visible : Visibility.Collapsed; break;
        default: break;
      }
    }

    public void HandleKeyUp(KeyEventArgs e)
    {
      switch (e.Key)
      {
        //case Key.F2: onKeepAwake_F2(); break;
        //case Key.F3: onIsHeaterOnF3(); break;
        //case Key.F6: onIsSpeechOnF6(); break;
        //case Key.F7: onIsSayMinOnF7(); break;
        //case Key.F8: onAutoSleep_F8(); break;
        //case Key.F9: onAutoLocke_F9(); break;
        default: return;
      }

      e.Handled = true;
    }
  }
}
