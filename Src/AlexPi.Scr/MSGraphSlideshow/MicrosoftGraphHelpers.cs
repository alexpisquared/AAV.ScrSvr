internal static class MicrosoftGraphHelpers
{
  public static (DateTimeOffset minDate, string report) GetEarliestDate(DriveItem? driveItem)
  {
    return EarliestDate(
      driveItem?.Photo?.TakenDateTime,
      driveItem?.CreatedDateTime,
      driveItem?.LastModifiedDateTime,
      driveItem?.FileSystemInfo?.CreatedDateTime,
      driveItem?.FileSystemInfo?.LastModifiedDateTime);
  }

  static (DateTimeOffset minDate, string report) EarliestDate(DateTimeOffset? taken, DateTimeOffset? created, DateTimeOffset? lastModified, DateTimeOffset? createdFSI, DateTimeOffset? lastModifiedFSI)
  {
    var same = " ____.____";
    var minValid = new DateTimeOffset(new DateTime(1980, 01, 01));
    var min = new[] { taken, created, lastModified, createdFSI, lastModifiedFSI, DateTimeOffset.Now }.Where(d => d.HasValue && d > minValid).Min(d => d.HasValue ? d.Value : minValid);
    var all = // $"{taken:yy-MM-dd}  {created:yy-MM-dd}  {lastModified:yy-MM-dd}  {createdFSI:yy-MM-dd}  {lastModifiedFSI:yy-MM-dd}   {min:yy-MM-dd HH:mm}";
      (taken == min ? same : $"{((taken ?? min) - min).TotalDays,10:####.###0}") +
      (created == min ? same : $"{((created ?? min) - min).TotalDays,10:####.###0}") +
      (lastModified == min ? same : $"{((lastModified ?? min) - min).TotalDays,10:####.###0}") +
      (createdFSI == min ? same : $"{((createdFSI ?? min) - min).TotalDays,10:####.###0}") +
      (lastModifiedFSI == min ? same : $"{((lastModifiedFSI ?? min) - min).TotalDays,10:####.###0}") +
      $"  {min:yy-MM-dd HH:mm}";

    return (min, all);
  }
}