using AlexPi.Scr.Logic;
using AsLink;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AlexPi.Scr.Vws
{
  public partial class ControlPanel : TopmostUnCloseableWindow
  {
    const int stepDelayMs = 333;
    bool _alreadyCached = false, _cacheA, _cacheB, _cacheC, _cacheD, _cacheE, _cacheF, _cacheG, _cacheH, _cacheI, _cacheJ, _cacheK;

    public ControlPanel(GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      InitializeComponent();
      DataContext = this;
      Loaded += onLoaded;
      PreviewKeyUp += onPreviewKeyUp;
    }

    async void onLoaded(object s, RoutedEventArgs e)
    {
      await Task.Delay(stepDelayMs * 10); // App.SpeakAsync("Show time");
      ContainerA.IsChecked = AppSettings.Instance.CtrlA; await Task.Delay(stepDelayMs);
      ContainerB.IsChecked = AppSettings.Instance.CtrlB; await Task.Delay(stepDelayMs);
      ContainerC.IsChecked = AppSettings.Instance.CtrlC; await Task.Delay(stepDelayMs);
      ContainerD.IsChecked = AppSettings.Instance.CtrlD; await Task.Delay(stepDelayMs);
      ContainerE.IsChecked = AppSettings.Instance.CtrlE; await Task.Delay(stepDelayMs);
      ContainerF.IsChecked = AppSettings.Instance.CtrlF; await Task.Delay(stepDelayMs);
      ContainerG.IsChecked = AppSettings.Instance.CtrlG; await Task.Delay(stepDelayMs);
      ContainerH.IsChecked = AppSettings.Instance.CtrlH; await Task.Delay(stepDelayMs);
      ContainerI.IsChecked = AppSettings.Instance.CtrlI; await Task.Delay(stepDelayMs);
      ContainerJ.IsChecked = AppSettings.Instance.CtrlJ; await Task.Delay(stepDelayMs);
      ContainerK.IsChecked = AppSettings.Instance.CtrlK; await Task.Delay(stepDelayMs);
    }
    void onPreviewKeyUp(object s, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.F12:
          if (cbAll.IsChecked == true)      /**/ cbAll.IsChecked = false;
          else if (cbAll.IsChecked == null) /**/ cbAll.IsChecked = true;
          else                              /**/ cbAll.IsChecked = null;
          onTglAll(s, null); goto case Key.None;
        case Key.None:
        case Key.NumPad0:
        case Key.NumPad8:
        case Key.NumPad9:
        case Key.D0:
        case Key.D8:
        case Key.D9:

        case Key.F1: _GlobalEventHandler.TglContainerVis(ContainerA); break;
        case Key.F2: _GlobalEventHandler.TglContainerVis(ContainerB); break;
        case Key.F3: _GlobalEventHandler.TglContainerVis(ContainerC); break;

        case Key.F9:
        case Key.F10:
        case Key.F11:
          App.SpeakAsync($"{e.Key} is in the list. Handled set to true.");
          break;
        default: App.SpeakAsync($"{e.Key} is handled by the default case, but handled is still false!"); return;
      }

      e.Handled = true;
    }
    void onChkUnC(object s, RoutedEventArgs e) => _GlobalEventHandler.TglContainerVis(((FrameworkElement)s).Name, (((ToggleButton)s).IsChecked == true));//var vis = boo ? Visibility.Visible : Visibility.Collapsed;//switch (((CheckBox)s).Name)//{//  case "ContainerA": ((App)Application.Current).CntrA.Visibility = vis; AppSettings.Instance.CtrlA = boo; break;//  case "ContainerB": ((App)Application.Current).CntrB.Visibility = vis; AppSettings.Instance.CtrlB = boo; break;//  case "ContainerC": ((App)Application.Current).CntrC.Visibility = vis; AppSettings.Instance.CtrlC = boo; break;//  case "ContainerD": ((App)Application.Current).CntrD.Visibility = vis; AppSettings.Instance.CtrlD = boo; break;//  case "ContainerE": ((App)Application.Current).CntrE.Visibility = vis; AppSettings.Instance.CtrlE = boo; break;//  case "ContainerF": ((App)Application.Current).CntrF.Visibility = vis; AppSettings.Instance.CtrlF = boo; break;//  case "ContainerG": ((App)Application.Current).CntrG.Visibility = vis; AppSettings.Instance.CtrlG = boo; break;//  case "ContainerH": ((App)Application.Current).CntrH.Visibility = vis; AppSettings.Instance.CtrlH = boo; break;//  case "ContainerI": ((App)Application.Current).CntrI.Visibility = vis; AppSettings.Instance.CtrlI = boo; break;//  case "ContainerJ": ((App)Application.Current).CntrJ.Visibility = vis; AppSettings.Instance.CtrlJ = boo; break;//  case "ContainerK": ((App)Application.Current).CntrK.Visibility = vis; AppSettings.Instance.CtrlK = boo; break;//  default://    break;//}
    void onTglAll(object s, RoutedEventArgs e) { switch (cbAll.IsChecked) { case true: cacheCurVals(); setViz_And_ChkBoxVal(true); break; case false: cacheCurVals(); setViz_And_ChkBoxVal(false); break; default: restoreCachedVals(); break; } }

    // public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register("IsOn", typeof(bool), typeof(ControlPanel)/*, new PropertyMetadata(true, pcc)*/); public bool IsOn { get { return (bool)GetValue(IsOnProperty); } set { SetValue(IsOnProperty, value); } } static void pcc(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((App)App.Current).CntrA.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;

    void setViz_And_ChkBoxVal(bool value)
    {
      foreach (var cb in sp1.Children)
        if (cb is CheckBox && ((CheckBox)cb).Name.StartsWith("Container"))
        {
          _GlobalEventHandler.TglContainerVis(((CheckBox)cb).Name, value);
          ((CheckBox)cb).IsChecked = value;
        }
    }
    void setVizToChkBoxVal() { foreach (var cb in sp1.Children) if (cb is CheckBox) onChkUnC(cb, null); }
    void cacheCurVals()
    {
      if (_alreadyCached) return;

      _alreadyCached = true;
      _cacheA = AppSettings.Instance.CtrlA;
      _cacheB = AppSettings.Instance.CtrlB;
      _cacheC = AppSettings.Instance.CtrlC;
      _cacheD = AppSettings.Instance.CtrlD;
      _cacheE = AppSettings.Instance.CtrlE;
      _cacheF = AppSettings.Instance.CtrlF;
      _cacheG = AppSettings.Instance.CtrlG;
      _cacheH = AppSettings.Instance.CtrlH;
      _cacheI = AppSettings.Instance.CtrlI;
      _cacheJ = AppSettings.Instance.CtrlJ;
      _cacheK = AppSettings.Instance.CtrlK;
    }
    void restoreCachedVals()
    {
      ContainerA.IsChecked = _cacheA;
      ContainerB.IsChecked = _cacheB;
      ContainerC.IsChecked = _cacheC;
      ContainerD.IsChecked = _cacheD;
      ContainerE.IsChecked = _cacheE;
      ContainerF.IsChecked = _cacheF;
      ContainerG.IsChecked = _cacheG;
      ContainerH.IsChecked = _cacheH;
      ContainerI.IsChecked = _cacheI;
      ContainerJ.IsChecked = _cacheJ;
      ContainerK.IsChecked = _cacheK;

      setVizToChkBoxVal();
    }
  }
}