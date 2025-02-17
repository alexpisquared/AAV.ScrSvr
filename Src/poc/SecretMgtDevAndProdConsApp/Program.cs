using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;

SetupAndDisplaySecrets2020();

static void listSecretValues(SecretClient client, string[]? names)
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
      Console.Write($"{(secretValue.Value.Value.Length > 96 ? secretValue.Value.Value.Substring(0, 96).Substring(0, 96) : secretValue.Value.Value),-96} ");
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

static void SetupAndDisplaySecrets2020()
{
  // note: ConsoleApp1  registered as web  with:	Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)
  // ^ ?? ^ https://learn.microsoft.com/en-us/security/zero-trust/develop/identity-supported-account-types
  //2025:
  //tu: 2025 !!! app registration, key vault, secret, client secret, app id, directory id, tenant id, subscription id, resource group, key vault name, secret name, secret value
  // https://learn.microsoft.com/en-us/entra/external-id/customers/sample-desktop-wpf-dotnet-sign-in - fails at the end
  // https://www.c-sharpcorner.com/blogs/fetching-secrets-from-key-vault-in-net-console-app
  // https://learn.microsoft.com/en-us/entra/external-id/customers/sample-desktop-wpf-dotnet-sign-in#register-the-desktop-app
  // https://learn.microsoft.com/en-us/entra/external-id/self-service-sign-up-user-flow#enable-self-service-sign-up-for-your-tenant

  var url = new Uri("https://demopockv.vault.azure.net/");

  var c = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>().Build();

  string?
    _dir0 = c["akv:Kv_Overview_DirectoryId"], _app0 = c["akv:Application_client_ID_2025"], _app1 = c["akv:AppRegs_TestAppWeb_Overview_AppClientId"], _app2 = c["akv:AppRegs_ConsoleApp1_Overview_AppClientId"], _a1v1 = c["akv:AppRegs_TestAppWeb_CertAndScrts_Scr_Val"], _a2v1 = c["akv:AppRegs_ConsoleApp1_CertAndScrts_Sc1_Val"], _a2v2 = c["akv:AppRegs_ConsoleApp1_CertAndScrts_Sc2_Val"];
  var _snms = c["akv:SecretNames"]?.Split(' ');

  showConsoleColors(); Console.ForegroundColor = ConsoleColor.Gray; Console.WriteLine($"** POC:  [explanation]");
  listSecretValues(new SecretClient(url, new ClientSecretCredential(_dir0, c["akv:SecretMgtDevAndProdConsApp2025w_AppId"], c["akv:SecretMgtDevAndProdConsApp2025w_SeVal"])), _snms);
  //listSecretValues(new SecretClient(url, new ClientSecretCredential(_dir0, c["akv:Application_client_ID_2025"], c["akv:TestClientSecret5616_Value"])), _snms);
  //listSecretValues(new SecretClient(url, new ClientSecretCredential(_dir0, c["akv:Application_client_ID_2025"], c["akv:TestClientSecret5616_SecretId"])), _snms);
  //listSecretValues(new SecretClient(url, new ClientSecretCredential(_dir0, _app1, _a1v1)), _snms);
  //listSecretValues(new SecretClient(url, new ClientSecretCredential(_dir0, _app2, _a2v1)), _snms);
  //listSecretValues(new SecretClient(url, new ClientSecretCredential(_dir0, _app2, _a2v2)), _snms);
}
