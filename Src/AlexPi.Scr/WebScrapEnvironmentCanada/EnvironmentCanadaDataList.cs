using System;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
//
using AAV.Sys.Ext;

namespace EnvironmentCanadaScrap
{
  /// <summary>
  /// list of all the measurements with persisting capabilities to different media (FS, SQL, XML, etc.)
  /// </summary>
  public class EnvironmentCanadaDataList
  {
    public SortedList<string, EnvironmentCanadaData> _lst = new SortedList<string, EnvironmentCanadaData>();

    public void Add(EnvironmentCanadaData ecd)
    {
      string key = ecd.TakenAt.ToString() + ecd.SiteId.ToString();
      if (!_lst.ContainsKey(key))
        _lst.Add(key, ecd);
    }
    public void SaveToSql()
    {
      int i = 0;
      foreach (EnvironmentCanadaData ecd in _lst.Values)
      {
        Console.WriteLine(":> {0}/{1} {2}%", ++i, _lst.Values.Count, 100 * i / _lst.Values.Count);
        SaveToSql(ecd);
      }
    }
    public void SaveToSql(EnvironmentCanadaData ecd)
    {
      string queryString = @"
				if not exists (select Id from Site where Id = @SiteId) 
					INSERT INTO Site (Id, [Name], Notes) VALUES (@SiteId,@SiteId, 'Auto added on a measure insert');
				if not exists (select Id from Measure where SiteId = @SiteId and TakenAt = @TakenAt) -- and RowAddedBy = 'EnvCanada') 
					INSERT INTO Measure (SiteId, TakenAt, TempAir, TempSea, Humidity, DewPoint, WindKmh, WindGust, WindDir, Pressure, Visibility, Humidex, TempNormMin, TempNormMax, SunRise, SunSet, WavePeriod, WaveHeight, Conditions, RawSrcText, RowAddedBy ) 
					VALUES             (@SiteId,@TakenAt,@TempAir,@TempSea,@Humidity,@DewPoint,@WindKmh,@WindGust,@WindDir,@Pressure,@Visibility,@Humidex,@TempNormMin,@TempNormMax,@SunRise,@SunSet,@WavePeriod,@WaveHeight,@Conditions,@RawSrcText, 'EnvCanada')";

      ExecuteSqlCommand(queryString, ecd);
    }

    public void Load(string file)
    {
      string[] readText = File.ReadAllLines(file);
      foreach (string s in readText)
      {
        Console.WriteLine(s);
      }
    }
    public void Save(string file)
    {


      if (File.Exists(file))
      {
        Console.WriteLine("{0} already exists.", file);
        return;
      }

      using (StreamWriter sw = File.CreateText(file))
      {
        foreach (EnvironmentCanadaData ecd in _lst.Values)
        {
          sw.WriteLine(ecd.ToString());
        }
        sw.Close();
      }

    }
    public static EnvironmentCanadaDataList XmlLoad(string file)
    {
      EnvironmentCanadaDataList _EnvironmentCanadaDataList = null;

      try
      {
        StreamReader reader = new StreamReader(file);
        XmlSerializer xml = new XmlSerializer(typeof(EnvironmentCanadaDataList));
        _EnvironmentCanadaDataList = (EnvironmentCanadaDataList)xml.Deserialize(reader);
        reader.Close();
      }
      catch (Exception ex)
      {
        ex.Log();
        _EnvironmentCanadaDataList = null;
      }

      return _EnvironmentCanadaDataList;
    }
    public bool XmlSave(string file)
    {
      try
      {
        StreamWriter writer = new StreamWriter(file);
        XmlSerializer xml = new XmlSerializer(GetType());
        xml.Serialize(writer, this);
        writer.Close();
      }
      catch (Exception ex)
      {
        ex.Log(); // ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());

        return false;
      }

      return true;
    }

    void ExecuteSqlCommand(string queryString, EnvironmentCanadaData ecd)
    {
      try
      {
        using (SqlConnection connection = new SqlConnection(@"Data Source=.\SqlExpress;Initial Catalog=Weather;Integrated Security=True;Connect Timeout=180"))// SqlConnectivity.ConStrProvider.GetLocalizedConStr("Weather")))
        {
          SqlCommand command = new SqlCommand(queryString, connection);
          command.Parameters.AddWithValue("@SiteId", ecd.SiteId);
          command.Parameters.AddWithValue("@TakenAt", ecd.TakenAt);
          addPar("@TempAir", ecd.TempAir, command, -50);
          addPar("@TempSea", ecd.TempSea, command, 1);
          addPar("@Humidity", ecd.Humidity, command, 1);
          addPar("@DewPoint", ecd.DewPoint, command, 1);
          addPar("@WindKmh", ecd.WindKmH, command, 0);
          addPar("@WindGust", ecd.WindGust, command, 0);
          command.Parameters.AddWithValue("@WindDir", ecd.WindDir);
          addPar("@Pressure", ecd.Pressure, command, 10);
          addPar("@Visibility", ecd.Visibility, command, 1);
          addPar("@Humidex", ecd.Humidex, command, 1);
          addPar("@TempNormMin", ecd.TempNormMin, command, 1);
          addPar("@TempNormMax", ecd.TempNormMax, command, 1);
          if (ecd.SunRise == DateTime.MinValue)
            command.Parameters.AddWithValue("@SunRise", DBNull.Value);
          else
            command.Parameters.AddWithValue("@SunRise", ecd.SunRise);
          if (ecd.SunSet == DateTime.MinValue)
            command.Parameters.AddWithValue("@SunSet", DBNull.Value);
          else
            command.Parameters.AddWithValue("@SunSet", ecd.SunSet);
          addPar("@WavePeriod", ecd.WavePeriod, command, 0);
          addPar("@WaveHeight", ecd.WaveHeight, command, 0);
          //if (!string.IsNullOrEmpty(ecd.Conditions))
          command.Parameters.AddWithValue("@Conditions", string.IsNullOrEmpty(ecd.Conditions) ? "" : ecd.Conditions);
          command.Parameters.AddWithValue("@RawSrcText", string.IsNullOrEmpty(ecd.RawSrcText) ? "" : ecd.RawSrcText);
          command.Connection.Open();
          command.ExecuteNonQuery();
        }
      }
      catch (Exception ex)
      {
        ex.Log(); // ex.Log(); // AAV.CustomControlLibrary.Logger.LogException(ex, System.Reflection.MethodInfo.GetCurrentMethod());
      }

    }
    static void addPar(string sqlParName, double? parValue, SqlCommand command, double minAcceptablevalue)
    {
      if (parValue != null && parValue >= minAcceptablevalue)
        command.Parameters.AddWithValue(sqlParName, parValue);
      else
        command.Parameters.AddWithValue(sqlParName, DBNull.Value);
    }
  }

  //COPY FROM dal
  //  public static class ConStrProvider
  //  {
  //    static ConStrProvider()
  //    {
  //      string s = GetLocalizedConStr("");
  //    }

  //    public static string GetLocalizedConStr(string dbName)
  //    {
  //      switch (Environment.MachineName)
  //      {
  //        default: File.AppendAllText(OneDrive.Root, @"\web.cache\New PC for ConStrProvider.cs .txt", DateTime.Now.ToString("dd-MMM-yyyy HH:mm") + " " + Environment.MachineName + "\r\n"); break;
  //        case "HP1": return string.Format(@"Data Source=HP1\SQL05;Initial Catalog={0};Persist Security Info=True;User ID=UniReadOnly;password=kjldfs;Connect Timeout=3", dbName);
  //#if DEBU G
  //        case "DELL2": return string.Format(@"Data Source=DELL2\SQL05;Initial Catalog={0};Integrated Security=True;Connect Timeout=3", dbName);
  //#else
  //        case "DELL2": return string.Format(@"Data Source=HP1\SQL05;Initial Catalog={0};Persist Security Info=True;User ID=UniReadOnly;password=kjldfs;Connect Timeout=3", dbName);
  //#endif
  //        case "CON-AHRAPACH": return string.Format("Data Source=localhost;Initial Catalog={0};Integrated Security=True;Connect Timeout=3", dbName);
  //      }

  //      return null;
  //    }
  //  }

}
