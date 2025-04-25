using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

SetupAndDisplaySecrets2020();

static void SetupAndDisplaySecrets2020()
{
  var urlKV = new Uri("https://demopockv.vault.azure.net/");

  var cfg = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

  string? _dir0 = cfg["akv:DirId"], _app0 = cfg["akv:Application_client_ID_2025"], _app1 = cfg["akv:AppRegs_TestAppWeb_Overview_AppClientId"], _app2 = cfg["akv:AppRegs_ConsoleApp1_Overview_AppClientId"], _a1v1 = cfg["akv:AppRegs_TestAppWeb_CertAndScrts_Scr_Val"], _a2v1 = cfg["akv:AppRegs_ConsoleApp1_CertAndScrts_Sc1_Val"], _a2v2 = cfg["akv:AppRegs_ConsoleApp1_CertAndScrts_Sc2_Val"];

  showConsoleColors(); Console.ForegroundColor = ConsoleColor.Gray; Console.WriteLine($"** POC:  [explanation]");

  if (DateTime.Now != DateTime.Today)
    listSecretValues(new SecretClient(urlKV, new ClientSecretCredential(_dir0, cfg["akv:AppId"], cfg["akv:SeVal"])));
  else
  {
    listSecretValues(new SecretClient(urlKV, new ClientSecretCredential(_dir0, cfg["akv:Application_client_ID_2025"], cfg["akv:TestClientSecret5616_Value"])));
    listSecretValues(new SecretClient(urlKV, new ClientSecretCredential(_dir0, cfg["akv:Application_client_ID_2025"], cfg["akv:TestClientSecret5616_SecretId"])));
    listSecretValues(new SecretClient(urlKV, new ClientSecretCredential(_dir0, _app0, _a1v1)));
    listSecretValues(new SecretClient(urlKV, new ClientSecretCredential(_dir0, _app1, _a1v1)));
    listSecretValues(new SecretClient(urlKV, new ClientSecretCredential(_dir0, _app2, _a2v1)));
    listSecretValues(new SecretClient(urlKV, new ClientSecretCredential(_dir0, _app2, _a2v2)));
  }
}
static void listSecretValues(SecretClient client)
{
  Console.ForegroundColor = ConsoleColor.DarkGray; Console.WriteLine("All Secrets:     Name             Value                                                Type");

  foreach (var secret in client.GetPropertiesOfSecrets())
  {
    try
    {
      var secretValue = client.GetSecret(secret.Name);
      Console.ForegroundColor = ConsoleColor.DarkYellow;
      Console.Write($" {secret.Name,-32} ");
      Console.ForegroundColor = ConsoleColor.DarkCyan;
      Console.Write($"{(secretValue.Value.Value.Length > 96 ? secretValue.Value.Value[..96][..96] : secretValue.Value.Value),-96} ");
      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.Write($"{secretValue.Value.Properties.ContentType}\n");
    }
    catch (Exception ex) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"@@ {ex.Message.Replace("\n", " ").Replace("\r", " ")}"); Console.ResetColor(); Console.ForegroundColor = ConsoleColor.White; }
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

