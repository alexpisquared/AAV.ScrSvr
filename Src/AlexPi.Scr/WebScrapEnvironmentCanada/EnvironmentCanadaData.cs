using AAV.Sys.Ext;
using System;
using System.IO;
using System.Xml.Serialization;

namespace EnvironmentCanadaScrap
{
	[Serializable]
	public class EnvironmentCanadaData
	{
		public bool IsValid
		{
			get { return true; }
		}
		public int SiteId { get; set; }
		public double TempAir { get; set; }
		public double TempSea { get; set; }
		public double Humidity { get; set; }
		public double DewPoint { get; set; }
		public double WindKmH { get; set; }
		public double WindGust { get; set; }
		public string WindDir { get; set; }
		public double Pressure { get; set; }
		public double Visibility { get; set; }
		public double? Humidex { get; set; }
		public double TempYesterdayMin { get; set; }
		public double TempYesterdayMax { get; set; }
		public double TempNormMin { get; set; }
		public double TempNormMax { get; set; }
		public double TempExtrMin { get; set; } // no data at the assoiciated page ==> goto http://www.weatheroffice.gc.ca/almanac/almanac_e.html?ykz
		public double TempExtrMax { get; set; }
		public int YearExtrMin { get; set; }
		public int YearExtrMax { get; set; }

		DateTime _SunRise = DateTime.Today.AddHours(6);

		public DateTime SunRise
		{
			get { return _SunRise; }
			set { _SunRise = value; }
		}
		DateTime _SunSet = DateTime.Today.AddHours(18);

		public DateTime SunSet
		{
			get { return _SunSet; }
			set { _SunSet = value; }
		}
		DateTime _TakenAt = DateTime.Now;

		public DateTime TakenAt
		{
			get { return _TakenAt; }
			set { _TakenAt = value; }
		}

		public double WaveHeight { get; set; }
		public double WavePeriod { get; set; }
		public string Conditions { get; set; }
		public string ConditiImg { get; set; }
		public string RawSrcText { get; set; }

		public override string ToString()
		{
			return string.Format("{0}\t{1,3:N0}\t{2,3:N0}\t{3,3:N0}\t{4,3:N0}\t{5,3:N0}\t{6,6:N1}\t{7,3:N0}\t{8,3:N0}\t{9,3:N0}\t{10,3:N0}\t{11,3:N0}\t{12,3:N0} \t{13} \t{14} \t{15} \t{16}",
				TakenAt.ToString("ddMMMyy HH:mm"), // 0
				TempAir,          // 1                                          
				Humidity,				// 2
				DewPoint,				// 3
				WindKmH,					// 4
				WindGust,				// 5
				Pressure,				// 6
				Visibility,			// 7
				Humidex,					// 8
				TempYesterdayMin,// 9
				TempYesterdayMax,// 10
				TempNormMin,			// 11
				TempNormMax,			// 12
				SunRise.ToString("ddMMMyy HH:mm"),					// 13
				SunSet.ToString("ddMMMyy HH:mm"),					// 14
				Conditions,			// 15
				WindDir);					// 16
		}

		public static EnvironmentCanadaData XmlLoad(string file)
		{
			EnvironmentCanadaData _EnvironmentCanadaData = null;

			try
			{
				StreamReader reader = new StreamReader(file);
				XmlSerializer xml = new XmlSerializer(typeof(EnvironmentCanadaData));
				_EnvironmentCanadaData = (EnvironmentCanadaData)xml.Deserialize(reader);
				reader.Close ( );
			}
			catch (Exception ex)
			{
				ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());

				_EnvironmentCanadaData = null;
			}

			return _EnvironmentCanadaData;
		}
		public bool XmlSave(string file)
		{
			try
			{
				StreamWriter writer = new StreamWriter(file);
				XmlSerializer xml = new XmlSerializer(GetType());
				xml.Serialize(writer, this);
				writer.Close ( );
			}
			catch (Exception ex)
			{
				ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());

				return false;
			}

			return true;
		}
	}
}