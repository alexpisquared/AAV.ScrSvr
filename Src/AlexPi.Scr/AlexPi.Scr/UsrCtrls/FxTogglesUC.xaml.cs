﻿using AsLink;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AlexPi.Scr.UsrCtrls
{
  public partial class FxTogglesUC
  {
    public FxTogglesUC()
    {
      InitializeComponent();

      PreviewKeyDown += Window_PreviewKeyDown;
      Loaded += onLoaded;
    }
    async void onLoaded(object s, RoutedEventArgs e)
    {
      IsEnabled = false;
      await System.Threading.Tasks.Task.Delay(2000);

      //chKeepAwake.IsChecked = AppSettings.Instance.KeepAwake;
      //chIsHeaterOn.IsChecked = AppSettings.Instance.IsHeaterOn;

      chIsChimesOn.IsChecked = AppSettings.Instance.IsChimesOn;
      chIsRepeatOn.IsChecked = AppSettings.Instance.IsRepeatOn;
      chIsSaySecOn.IsChecked = AppSettings.Instance.IsSaySecOn;
      chIsSpeechOn.IsChecked = AppSettings.Instance.IsSpeechOn;
      chIsSayMinOn.IsChecked = AppSettings.Instance.IsSayMinOn;

      chAutoSleep.IsChecked = AppSettings.Instance.AutoSleep;
      chAutoLocke.IsChecked = AppSettings.Instance.AutoLocke;

      srMin2Sleep.Value = Math.Log10(AppSettings.Instance.Min2Sleep);
      srMin2Locke.Value = Math.Log10(AppSettings.Instance.Min2Locke);
      srMin2Sleep.ValueChanged += onMin2Sleep;
      srMin2Locke.ValueChanged += onMin2Locke;
      IsEnabled = true;
      tbMin2Sleep.Text = $"{AppSettings.Instance.Min2Sleep:N0}";
      tbMin2Locke.Text = $"{AppSettings.Instance.Min2Locke:N0}";
    }

    //void onKeepAwake_F2(object s = null, RoutedEventArgs e = null) => chKeepAwake.IsChecked = (AppSettings.Instance.KeepAwake = !AppSettings.Instance.KeepAwake);
    //void onIsHeaterOnF3(object s = null, RoutedEventArgs e = null) => chIsHeaterOn.IsChecked = (AppSettings.Instance.IsHeaterOn = !AppSettings.Instance.IsHeaterOn);

    void onIsChimesOnF3(object s = null, RoutedEventArgs e = null) => chIsChimesOn.IsChecked = (AppSettings.Instance.IsChimesOn = !AppSettings.Instance.IsChimesOn);
    void onIsRepeatOnF4(object s = null, RoutedEventArgs e = null) => chIsRepeatOn.IsChecked = (AppSettings.Instance.IsRepeatOn = !AppSettings.Instance.IsRepeatOn);
    void onIsSaySecOnF5(object s = null, RoutedEventArgs e = null) => chIsSaySecOn.IsChecked = (AppSettings.Instance.IsSaySecOn = !AppSettings.Instance.IsSaySecOn);
    void onIsSpeechOnF6(object s = null, RoutedEventArgs e = null) => chIsSpeechOn.IsChecked = (AppSettings.Instance.IsSpeechOn = !AppSettings.Instance.IsSpeechOn);
    void onIsSayMinOnF7(object s = null, RoutedEventArgs e = null) => chIsSayMinOn.IsChecked = (AppSettings.Instance.IsSayMinOn = !AppSettings.Instance.IsSayMinOn);
    void onAutoSleep_F8(object s = null, RoutedEventArgs e = null) => chAutoSleep.IsChecked = (AppSettings.Instance.AutoSleep = !AppSettings.Instance.AutoSleep);
    void onAutoLocke_F9(object s = null, RoutedEventArgs e = null) => chAutoLocke.IsChecked = (AppSettings.Instance.AutoLocke = !AppSettings.Instance.AutoLocke);
    void onMin2Sleep(object s, RoutedPropertyChangedEventArgs<double> e) { AppSettings.Instance.Min2Sleep = (int)Math.Pow(10, e.NewValue); tbMin2Sleep.Text = $"{AppSettings.Instance.Min2Sleep:N0}"; }
    void onMin2Locke(object s, RoutedPropertyChangedEventArgs<double> e) { AppSettings.Instance.Min2Locke = (int)Math.Pow(10, e.NewValue); tbMin2Locke.Text = $"{AppSettings.Instance.Min2Locke:N0}"; }

    void Window_PreviewKeyDown(object s, KeyEventArgs e) => e.Handled = true;
    //switch (e.Key)
    //{
    //  //case Key.F2: onKeepAwake_F2(); break;
    //  //case Key.F3: onIsHeaterOnF3(); break;
    //  case Key.F6: onIsSpeechOnF6(); break;
    //  case Key.F7: onIsSayMinOnF7(); break;
    //  case Key.F8: onAutoSleep_F8(); break;
    //  case Key.F9: onAutoLocke_F9(); break;
    //  default: return;
    //}
  }
}
