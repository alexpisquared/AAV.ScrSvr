namespace MSGraphSlideshow;

//[TestClass]
public class EarliestDateTests
{
  //[TestMethod]
  public void TestEarliestDate()
  {
    // Arrange
    var driveItem = new DriveItem
    {
      Photo = new Photo
      {
        TakenDateTime = new DateTimeOffset(new DateTime(2022, 1, 1))
      },
      CreatedDateTime = new DateTimeOffset(new DateTime(2022, 2, 1)),
      LastModifiedDateTime = new DateTimeOffset(new DateTime(2022, 3, 1)),
      //FileSystemInfo = new Microsoft.Graph.FileSystemInfo
      //{
      //  CreatedDateTime = new DateTimeOffset(new DateTime(2022, 4, 1)),
      //  LastModifiedDateTime = new DateTimeOffset(new DateTime(2022, 5, 1))
      //}
    };

    var expectedMinDate = new DateTimeOffset(new DateTime(2022, 1, 1));
    var (minDate, report) = MicrosoftGraphHelpers.GetEarliestDate(driveItem);

    Debug.WriteLine(report);
    Debug.Assert(expectedMinDate == minDate);
  }
}
