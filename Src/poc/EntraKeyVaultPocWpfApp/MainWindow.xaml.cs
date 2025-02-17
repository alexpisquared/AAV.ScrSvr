using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Windows;
namespace EntraKeyVaultPocWpfApp;
public partial class MainWindow : Window
{
  public MainWindow() => InitializeComponent();

  private void OnLoaded(object sender, RoutedEventArgs e) => SetupAndDisplaySecrets2020();

  void SetupAndDisplaySecrets2020()
  {

    var cfg = new ConfigurationBuilder().AddUserSecrets<MainWindow>().Build();

    listSecretValues(new SecretClient(new Uri("https://demopockv.vault.azure.net/"), new ClientSecretCredential(
      cfg["akv:Kv_Overview_DirectoryId"],
      cfg["akv:EntraKeyVaultPocWpfApp2025_AppId"],
      cfg["akv:EntraKeyVaultPocWpfApp2025_SeVal"])));
  }
  void listSecretValues(SecretClient client)
  {
    tbk0.Text = (" All Secrets:     \n Name                             Value                                                                                             Type\n\n");

    foreach (var secret in client.GetPropertiesOfSecrets())
    {
      try
      {
        var secretValue = client.GetSecret(secret.Name);
        
        tbk0.Text += ($" {secret.Name,-32} ");
        tbk0.Text += ($"{(secretValue.Value.Value.Length > 96 ? secretValue.Value.Value[..96][..96] : secretValue.Value.Value),-96}");
        tbk0.Text += ($" {secretValue.Value.Properties.ContentType}\n");
      }
      catch (Exception ex) { Trace.WriteLine($"@@ {ex.Message.Replace("\n", " ").Replace("\r", " ")}"); }
    }
  }
}