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
    var minValid = new DateTimeOffset(new DateTime(1980, 01, 01));
    var min = new[] {
      taken,
      created,
      lastModified,
      createdFSI,
      lastModifiedFSI, DateTimeOffset.Now }.Where(d => d.HasValue && d > minValid).Min(d => d.HasValue ? d.Value : minValid);
    var all = // $"{taken:yy-MM-dd}  {created:yy-MM-dd}  {lastModified:yy-MM-dd}  {createdFSI:yy-MM-dd}  {lastModifiedFSI:yy-MM-dd}   {min:yy-MM-dd HH:mm}";
      (taken == min ? "__-__-__  " : $"{((taken ?? min) - min).TotalDays,9:N5} ") +
      (created == min ? "__-__-__  " : $"{((created ?? min) - min).TotalDays,9:N5} ") +
      (lastModified == min ? "__-__-__  " : $"{((lastModified ?? min) - min).TotalDays,9:N5} ") +
      (createdFSI == min ? "__-__-__  " : $"{((createdFSI ?? min) - min).TotalDays,9:N5} ") +
      (lastModifiedFSI == min ? "__-__-__  " : $"{((lastModifiedFSI ?? min) - min).TotalDays,9:N5} ") +
      $" {min:yy-MM-dd HH:mm}";

    return (min, all);
  }
}