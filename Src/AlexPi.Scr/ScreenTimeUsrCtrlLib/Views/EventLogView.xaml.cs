using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Data;

namespace ScreenTimeUsrCtrlLib.Views;
public partial class EventLogView : Window
{
  public EventLogView(SortedList<DateTime, EventOfInterestFlag> thisDayEois, Window? owner)
  {
    InitializeComponent();
    DataContext = new MainVM(thisDayEois);
    Owner = owner;
  }

  void OnClose(object sender, RoutedEventArgs e) => Close();
}

public record EventOfInterest(
  DateTime ThisTime, EventOfInterestFlag ThisEoi,
  DateTime PrevTime, EventOfInterestFlag PrevEoi,
  TimeSpan TimeSpent,
  TimeSpan TimeSpentTotal,
  ActivityProjected ActivityAsIs,
  ActivityProjected ActivityCorrected,
  string TimeSpentFormatted,
  string ColorCode);

public partial class MainVM : ObservableObject //tu: !*\bin\*;!*\obj\*;!*\.*\*;!*.g.cs
{
  public MainVM(SortedList<DateTime, EventOfInterestFlag> thisDayEois)
  {
    if (thisDayEois.Count < 2) return;

    List<EventOfInterest> eventOfInterestList = [];

    var prevEoi = thisDayEois.FirstOrDefault();
    eventOfInterestList.Add(new EventOfInterest(prevEoi.Key, prevEoi.Value, prevEoi.Key, prevEoi.Value, TimeSpan.Zero, TimeSpan.Zero, ActivityProjected.Unknown, ActivityProjected.Unknown, "", "#888"));

    foreach (var eoi in thisDayEois.Skip(1))
    {
      var act2 = GetActivityFromEdgeEOIs(prevEoi.Value, eoi.Value);
      var ww = new EventOfInterest(
        eoi.Key, eoi.Value,
        prevEoi.Key, prevEoi.Value,
        eoi.Key - prevEoi.Key,
        TimeSpan.Zero,
        GetActivityFromFinishedEOI(prevEoi.Value),
        act2,
        $"{new string('\t', ActivityProjected.Busy - act2)} {(eoi.Key - prevEoi.Key):h\\:mm\\:ss}",
        act2 switch
        {
          ActivityProjected.Busy => "#0d0",
          ActivityProjected.Idle => "#c70",
          ActivityProjected.Off => "#d28",
          _ => "#888"
        });

      eventOfInterestList.Add(ww);

      prevEoi = eoi;
    }

    PageCvs = CollectionViewSource.GetDefaultView(eventOfInterestList); //tu: ?? instead of .LoadAsync() / .Local.ToObservableCollection() ?? === PageCvs = CollectionViewSource.GetDefaultView(await dbq.Funds.ToListAsync());
  }

  private ActivityProjected GetActivityFromEdgeEOIs(EventOfInterestFlag prevEoi, EventOfInterestFlag eoi) => (prevEoi, eoi) switch
  {
    (EventOfInterestFlag.___Pwr, _) => ActivityProjected.Off,
    (EventOfInterestFlag.Idle___, _) => ActivityProjected.Idle,
    (EventOfInterestFlag.___Idle, _) => ActivityProjected.Busy,
    (EventOfInterestFlag.Pwr___, _) => ActivityProjected.Busy,
    (EventOfInterestFlag.Day1stMaybe, _) => ActivityProjected.Unknown,
    (EventOfInterestFlag.WasOffIgnore, _) => ActivityProjected.Busy,
    (EventOfInterestFlag.WasOn_Ignore, _) => ActivityProjected.Busy,
    (EventOfInterestFlag.NowBusy, _) => ActivityProjected.Busy,
    (EventOfInterestFlag.Who_Knows_What, _) => ActivityProjected.Busy,
    (_, _) => ActivityProjected.Unknown
  };

  private static ActivityProjected GetActivityFromFinishedEOI(EventOfInterestFlag prevEoi) => prevEoi switch
  {
    EventOfInterestFlag.___Pwr => ActivityProjected.Off,
    EventOfInterestFlag.Idle___ => ActivityProjected.Idle,
    EventOfInterestFlag.___Idle => ActivityProjected.Busy,
    EventOfInterestFlag.Pwr___ => ActivityProjected.Busy,
    EventOfInterestFlag.Day1stMaybe => ActivityProjected.Unknown,
    EventOfInterestFlag.WasOffIgnore => ActivityProjected.Busy,
    EventOfInterestFlag.WasOn_Ignore => ActivityProjected.Busy,
    EventOfInterestFlag.NowBusy => ActivityProjected.Busy,
    EventOfInterestFlag.Who_Knows_What => ActivityProjected.Busy,
    _ => ActivityProjected.Unknown
  };

  [ObservableProperty] ICollectionView? pageCvs;
}