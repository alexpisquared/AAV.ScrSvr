namespace OleksaScrSvr.VM.Misc;

public class DeplConst
{
    const string
      DeplName = "OleksaScrSvr",
      DeplExe = DeplName + ".App.exe";
    public static string DeplSrcDir => DevOps.IsDevMachineH ? @"C:\g\OleksaScrSvr\Src\OleksaScrSvr\OleksaScrSvr\bin\Release\net8.0-windows8.0\publish\win-x64" : $"""\\oak\cm\felixdev\apps\data\Oleksa\Tooling\{DeplName}\bin\Phase0""";
    public static string DeplTrgDir => $"""C:\Apps\DEV\{DeplName}""";
    public static string DeplSrcExe => $"""{DeplSrcDir}\{DeplExe}""";
    public static string DeplTrgExe => $"""{DeplTrgDir}\{DeplExe}""";
}