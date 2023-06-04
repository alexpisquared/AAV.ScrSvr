﻿namespace AlexPi.Scr.Vws;

public partial class ContainerK : TopmostUnCloseableWindow
{
  public ContainerK(GlobalEventHandler globalEventHandler) : base(globalEventHandler)
  {
    try
    {
      InitializeComponent();
      var sj = new ConfigRandomizer();
      MsgSlideshowUsrCtrl1.ClientId = sj.GetRandomFromUserSection("ClientId");
    }
    catch (Exception ex) { ex.Pop(); }
  }

  async void onManualUpdateRequested(object s, RoutedEventArgs e) { ((Button)s).Visibility = Visibility.Collapsed; await App.SpeakAsync($"{await EvLogHelper.UpdateEvLogToDb(15, "ContainerK.onManualUpdateRequested()")} rows saved", ignoreBann: true); }
}