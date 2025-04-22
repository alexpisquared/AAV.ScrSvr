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
  string ColorCode);

public partial class MainVM : ObservableObject //tu: !*\bin\*;!*\obj\*;!*\.*\*;!*.g.cs
{
  public MainVM(SortedList<DateTime, EventOfInterestFlag> thisDayEois)
  {
    if (thisDayEois.Count < 2) return;

    List<EventOfInterest> eventOfInterestList = [];

    var prevEoi = thisDayEois.FirstOrDefault();
    eventOfInterestList.Add(new EventOfInterest(
      prevEoi.Key, prevEoi.Value,
      prevEoi.Key, prevEoi.Value,
      TimeSpan.Zero, TimeSpan.Zero,
      ActivityProjected.Unknown, ActivityProjected.Unknown, "#888"));

    foreach (var eoi in thisDayEois.Skip(1))
    {
      eventOfInterestList.Add(new EventOfInterest(
        eoi.Key, eoi.Value,
        prevEoi.Key, prevEoi.Value,
        eoi.Key - prevEoi.Key, TimeSpan.Zero,
        ActivityProjected.Unknown, ActivityProjected.Unknown, "#888"));
    }

    PageCvs = CollectionViewSource.GetDefaultView(eventOfInterestList); //tu: ?? instead of .LoadAsync() / .Local.ToObservableCollection() ?? === PageCvs = CollectionViewSource.GetDefaultView(await dbq.Funds.ToListAsync());
  }

  [ObservableProperty] ICollectionView? pageCvs;
}