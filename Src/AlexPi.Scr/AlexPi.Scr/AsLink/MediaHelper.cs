namespace AsLink
{
  public static class MediaHelper
  {
    public static string AllMediaExtensions => ".3g2,.3gp2,.3gp,.3gpp,.aac,.ac3,.adt,.adts,.aif,.amr,.asf,.avi,.bc!,.bin,.divx,.ec3,.flac,.m2ts,.m4a,.m4r,.m4v,.mka,.mkv,.mov,.mp3,.mp4,.mpa,.mpeg,.mpg,.mts,.part,.rm,.rmvb,.vob,.wav,.wma,.wmv";
    public static string AllVideoExtensions => ".3g2,.3gp2,.3gp,.3gpp,.asf,.avi,.bin,.divx,.m2ts,.m4v,.mkv,.mov,.mp4,.mp4v,.mpeg,.mpg,.mts,.part,.vob,.wmv";
    public static string AllAudioExtensions => ".3g2,.3gp2,.3gp,.3gpp,.aac,.adt,.ac3,.adts,.aif,.asf,.ec3,.m4a,.mp3,.mpeg,.mpg,.part,.rm,.rmvb,.wav,.wma";

    public static string[] AllMediaExtensionsAry => AllMediaExtensions.ToLower().Split(',');

    public static string AudioExtDsv => AllMediaExtensions.Replace(",", "");

    public static string WalkmanPlayableExtDsv => ".mp3";
    public static string AnonsExt => " .wav.mp3";

    public static bool IsVideo(string fullPath) => AllVideoExtensions.Contains(System.IO.Path.GetExtension(fullPath));

    public static bool IsAudio(string fullPath) => AllAudioExtensions.Contains(System.IO.Path.GetExtension(fullPath));
  }
}
/*
From:
    media associations for zune - use to improve VPC list.reg

Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages\Microsoft.ZuneMusic_10.18011.13411.0_x64__8wekyb3d8bbwe\Microsoft.ZuneMusic\Capabilities\FileAssociations]
".aac"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".ac3"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".adt"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".adts"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".amr"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".ec3"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".flac"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".m4a"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".m4r"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".mka"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".mp3"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".mpa"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".wav"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".wma"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".m3u"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".wpl"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"
".zpl"="AppXqj98qxeaynz6dv4459ayz6bnqxbyaqcs"

*/


