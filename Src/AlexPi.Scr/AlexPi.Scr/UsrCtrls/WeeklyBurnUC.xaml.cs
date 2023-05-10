//using StandardLib.Extensions;
using AsLink;
using Db.EventLog.Main;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AlexPi.Scr.UsrCtrls
{
    public partial class WeeklyBurnUC
    {
        //double _aw/*, _ah*/;
        readonly DateTime _start = App.StartedAt;
        readonly DateTime _idleAt = App.StartedAt.AddSeconds(-App.IdleTimeoutSec);
        async void onLoaded(object s, RoutedEventArgs e)
        {
            try
            {
                const double _wkDayHrs = 8.5;
                var ttlWkExpHr = _wkDayHrs * 5;
                var wkStartsAtHr = 8.25;
                var now = DateTime.Now;
                var today = DateTime.Today;
                var lastSunday = today.AddDays(-(int)today.DayOfWeek);

                var daySpans = new double[(int)today.DayOfWeek + 1];
                var day = 0;
                for (DateTime d = lastSunday; d <= today; d = d.AddDays(1)) daySpans[day++] = await EvLogHelper.GetWkSpanForTheDay(d);

                var weekSpans = daySpans.Sum(r => r);
                var expectatio = now.Hour >= wkStartsAtHr + _wkDayHrs ?
                    (today - lastSunday).TotalDays * _wkDayHrs :
                    ((today - lastSunday).TotalDays - 1) * _wkDayHrs + ((now - today).TotalHours - wkStartsAtHr);

                var percentage = (100.0 * weekSpans / ttlWkExpHr);
                var leftHrs = ttlWkExpHr - weekSpans;

                gaugeTor0.MiddlValAnim = percentage * 1.8 - 90;
                gaugeTor0.OuterValAnim = (100.0 * expectatio / ttlWkExpHr) * 1.8 - 90;
                gaugeTor0.InnerValAnim = (100.0 * (day - 1) * _wkDayHrs / ttlWkExpHr) * 1.8 - 90; // < target % for the EODay

                if (leftHrs < _wkDayHrs) // if left is less than a day's worth: display time to leave.
                    gaugeTor0.GaugeText = $"{weekSpans:N1} / {ttlWkExpHr:N1}\r\n@ {now.AddHours(leftHrs):HH:mm}";
                else
                    gaugeTor0.GaugeText = $"{weekSpans:N1} / {ttlWkExpHr:N1}\r\n{leftHrs:N1} hr";
            }
            catch (Exception ex) { ex.Log(); } // catch (Exception ex)  {  ex.Log(); }
        }

        public WeeklyBurnUC() { InitializeComponent(); }



        void addWkTimeSegment(double v1, double v2, EvOfIntFlag eoi/*, object clr*/) { }
    }
}