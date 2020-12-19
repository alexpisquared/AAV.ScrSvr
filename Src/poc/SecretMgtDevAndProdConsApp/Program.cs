using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using Microsoft.Extensions.Configuration;
using System.Linq;
// note: ConsoleApp1  registered as web  with:	Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)

const string
  _uri = "AppSettings:AzureKeyVault:Kv_Overview_VaultURI",
  _dir = "AppSettings:AzureKeyVault:Kv_Overview_DirectoryId",
  _app1 = "AppSettings:AzureKeyVault:AppRegs_TestAppWeb_Overview_AppClientId",
  _app2 = "AppSettings:AzureKeyVault:AppRegs_ConsoleApp1_Overview_AppClientId",
  _a1v1 = "AppSettings:AzureKeyVault:AppRegs_TestAppWeb_CertAndScrts_Scr_Val",
  _a2v1 = "AppSettings:AzureKeyVault:AppRegs_ConsoleApp1_CertAndScrts_Sc1_Val",
  _a2v2 = "AppSettings:AzureKeyVault:AppRegs_ConsoleApp1_CertAndScrts_Sc2_Val",
  _snms = "AppSettings:AzureKeyVault:SecretNames";

showConsoleColors();

var config = new ConfigurationBuilder()
  .SetBasePath(AppContext.BaseDirectory)
  .AddJsonFile("appsettings.json")
  .AddUserSecrets<WhatIsThatForType>().Build();

var some_Array = config.GetSection("VoiceNames").GetChildren().Select(x => x.Value).ToArray();
var voiceNames = config.GetSection("VoiceNames").Get<string[]>(); // needs Microsoft.Extensions.Configuration.Binder

Console.Write($"** URL: '{config["AppSettings:AzureKeyVault:URL"]}'    =>   ");
config["AppSettings:AzureKeyVault:URL"] = "Changed to this ... but not saved to file";
Console.Write($"'{config["AppSettings:AzureKeyVault:URL"]}'    \n\n\n");

Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine($"** POC:  !!!WTH!!! Any app can have access to any secret:");

listSecretValues(new SecretClient(new Uri(config[_uri]), new ClientSecretCredential(config[_dir], config[_app1], config[_a1v1])), config);
listSecretValues(new SecretClient(new Uri(config[_uri]), new ClientSecretCredential(config[_dir], config[_app2], config[_a2v1])), config);
listSecretValues(new SecretClient(new Uri(config[_uri]), new ClientSecretCredential(config[_dir], config[_app2], config[_a2v2])), config);

static void listSecretValues(SecretClient client, IConfiguration config)
{
  Console.ForegroundColor = ConsoleColor.Blue; Console.WriteLine($"\n{"Ssecret Name",34}  Secret Value"); Console.ForegroundColor = ConsoleColor.White;

  foreach (var secretName in config[_snms].Split(' '))
  {
    try
    {
      Console.Write($"{secretName,34}  ");
      Console.Write($"{client.GetSecret(secretName).Value.Value} \n");
    }
    catch (Exception ex) { Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($"@@ {ex.Message.Substring(0, 128).Replace("\n", " ").Replace("\r", " ")}"); Console.ResetColor(); Console.ForegroundColor = ConsoleColor.White; }
  }
}
static void showConsoleColors()
{
  for (var i = 0; i < 8; i++)
  {
    Console.ForegroundColor = (ConsoleColor)(i + 0); Console.Write($"{Console.ForegroundColor} ");
    Console.ForegroundColor = (ConsoleColor)(i + 8); Console.Write($"{Console.ForegroundColor} ");
  }
  Console.Write("\n");
}


public class WhatIsThatForType
{
  public string MyProperty { get; set; } = "<Default Value of Nothing SPecial>";
}
