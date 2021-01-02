using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using EnvironmentCanadaScrap;

namespace UsrCtrlPOCs
{
	/// <summary>
	/// Interaction logic for WeatherInfoUsrCtrl.xaml
	/// </summary>
	public partial class WeatherInfoUsrCtrl : UserControl
	{
		public WeatherInfoUsrCtrl()
		{
			InitializeComponent();
			var dbg = EnvntCanadaScraper.FetchCurConds(true);

			Trace.WriteLine(MethodInfo.GetCurrentMethod().Name + " 1", DateTime.Now.ToLongTimeString());
			launchScraper();// 			Loaded += new RoutedEventHandler(WeatherInfoUsrCtrl_Loaded);
			Trace.WriteLine(MethodInfo.GetCurrentMethod().Name + " 2", DateTime.Now.ToLongTimeString());
		}

		void WeatherInfoUsrCtrl_Loaded(object s, RoutedEventArgs e)
		{
			Trace.WriteLine(MethodInfo.GetCurrentMethod().Name + " 1", DateTime.Now.ToLongTimeString());
			launchScraper();
			Trace.WriteLine(MethodInfo.GetCurrentMethod().Name + " 2", DateTime.Now.ToLongTimeString());
		}

		void launchScraper()
		{
			Trace.WriteLine(MethodInfo.GetCurrentMethod().Name + " 1", DateTime.Now.ToLongTimeString());
			tbHih.Text =
			tbCur.Text =
			tbLow.Text = "...";

			var task = Task<EnvironmentCanadaData>.Factory.StartNew(() => EnvntCanadaScraper.FetchCurConds(true));
			task.ContinueWith(_ => updateUI(task.Result), TaskScheduler.FromCurrentSynchronizationContext());			//tu: Get the UI thread's context
			Trace.WriteLine(MethodInfo.GetCurrentMethod().Name + " 2", DateTime.Now.ToLongTimeString());
		}

		void updateUI(EnvironmentCanadaData ecd)
		{
			//22Trace.WriteLine(MethodInfo.GetCurrentMethod().Name + " 1", DateTime.Now.ToLongTimeString());
			tbExH.Text = ecd.TempExtrMax.ToString() + "°";
			tbHih.Text = ecd.TempNormMax.ToString() + "°";
			tbCur.Text = string.Format("{0}{1:N1}° ", ecd.TempAir > 0 ? "+" : "", ecd.TempAir);
			tbRea.Text = string.Format("{0:N0}° ", ecd.Humidex);
			tbLow.Text = ecd.TempNormMin.ToString() + "°";
			tbExL.Text = ecd.TempExtrMin.ToString() + "°";
			tbSun.Text = string.Format("↑☼  {0:H:mm}        {1:H:mm}  ☼↓", ecd.SunRise, ecd.SunSet);
			tbWhn.Text = string.Format("{0:H:mm}", ecd.TakenAt); //			tbWhn.Text = string.Format("{0:N0} min ago", (DateTime.Now - ecd.TakenAt).TotalMinutes);
			//if (!string.IsNullOrEmpty(ecd.ConditiImg)) 
			weaImg.Source = new BitmapImage(new Uri(ecd.ConditiImg ?? @"http://weather.gc.ca/weathericons/02.gif"));

			var tkn = ecd.TakenAt.ToOADate();
			tkn = tkn - Math.Floor(tkn);
			var now = DateTime.Now.ToOADate();
			now = now - Math.Floor(now);
			
			arc__.StartAngle =  tkn * 720 ;
			arc__.EndAngle =  now * 720;

			//22Trace.WriteLine(MethodInfo.GetCurrentMethod().Name + " 2", DateTime.Now.ToLongTimeString());
		}
	}
}
/// http://weather.yahoo.com/canada/ontario/markham-935/?unit=c
/// <div class="forecast-icon" style="filter: progid:DXImageTransform.Microsoft.AlphaImageLoader(src='http://l.yimg.com/a/i/us/nws/weather/gr/32d.png', sizingMethod='crop'); background-image: url("http://l.yimg.com/a/i/us/nws/weather/gr/32d.png"); background-attachment: scroll; background-repeat: repeat; background-position-x: 0%; background-position-y: 0%; background-size: auto; background-origin: padding-box; background-clip: border-box; background-color: transparent;"/>
/// 