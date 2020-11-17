using System;

namespace WebScrap//WindIWindsurf
{
	public class HourWeatherData
	{
		//public static HourWeatherData Create(string[] raw)
		//{
		//  HourWeatherData hwd = new HourWeatherData();

		//  if (hwd.ValidateInstantiate(raw))
		//    return hwd;
		//  else
		//    return null;
		//}

		public HourWeatherData(){}
		public HourWeatherData(DateTime time, double temp, string img, string cond)
		{
			Time = time;
			Temp = temp;
			ConditionsDescription = cond;
			ConditionsImageUrl = img;
		}

		public bool ValidateInstantiate(string[] raw)
		{
			try
			{
				if (raw.Length != 5)
					return false;

				ConditionsDescription = raw[4];
				ConditionsImageUrl = raw[3];
				var d = string.Format("{0}, 2008 {1}:00 {2}", raw[0].Split(',')[1].Trim(), raw[1].Split(' ')[0], raw[1].Split(' ')[1].ToUpper());
				Time = DateTime.Parse(d);
				Temp = Convert.ToInt32(raw[2]);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public DateTime Time { get; set; }
		public double Temp { get; set; }
		public double TempFeel { get; set; }
		public string ConditionsDescription { get; set; }
		public string ConditionsImageUrl { get; set; }
		public string ConditionsImageLocalCacheUrl { get; set; }
    //public System.Drawing ConditionsImage2 { get; set; }

    public override string ToString() => string.Format("{0}, {1}°, {2},\t {3}", Time.ToString("ddd MMM dd HH:mm"), Temp, ConditionsImageUrl, ConditionsDescription);
  }
}
