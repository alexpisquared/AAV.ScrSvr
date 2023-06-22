public class OleksaScrSvrModelBase
{
  public static T Load<T>() where T : new() => JsonFileSerializer.Load<T>(_fullPath) ?? new T(); //JsonIsoFileSerializer.Load<T>(iss: IsoConst.URoaA) ?? new T();
  public static void Save<T>(T ths) => JsonFileSerializer.Save(ths, _fullPath);                  //JsonIsoFileSerializer.Save(ths, iss: IsoConst.URoaA);

  static string _fullPath => (@$"{nameof(OleksaScrSvrModel)}.json"); // => Path.Combine(@$"C:\Apps\DEV\{nameof(OleksaScrSvrModel)}.json"); // => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @$"AppSettings\{AppDomain.CurrentDomain.FriendlyName}\UserSettings.json");
}