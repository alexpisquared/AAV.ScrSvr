namespace RadarPicCollect;

public class PicDetail
{
  double _measure = -99; // cm/hr/km^2

  public PicDetail(System.Drawing.Bitmap pic1, DateTime localTime1, string stationName, System.Drawing.Point picOffset, string cacheName)
  {
    Bitmap = pic1;
    ImageTime = localTime1;
    StationName = stationName;
    PicOffset = picOffset;
    CacheName = cacheName;
  }

  public System.Drawing.Bitmap Bitmap;
  public DateTime ImageTime;
  public string StationName { get; set; }
  public System.Drawing.Point PicOffset { get; set; }
  public string CacheName { get; set; }
  public double Measure => _measure == -99 && Bitmap != null ? _measure = PicMea.CalcMphInTheArea(Bitmap, ImageTime) : _measure;  
}
