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

    tbk0.Text = " All Secrets:     \n Name                             Value                                                                                             Type\n";

    for (int i = 0; i < 5; i++)
    {
      tbk0.Text += $"\n{i} ■ \n{listSecretValues(new SecretClient(new Uri("https://demopockv.vault.azure.net/"), new ClientSecretCredential(cfg["roo:DirId"], cfg[$"app{i}:AppId"], cfg[$"app{i}:SeVal"])))}" ;
    }
  }

  string listSecretValues(SecretClient client)
  {
    var rv = "";

    try
    {
      foreach (var secret in client.GetPropertiesOfSecrets())
      {
        var secretValue = client.GetSecret(secret.Name);

        rv += ($" {secret.Name,-32} ");
        rv += ($"{(secretValue.Value.Value.Length > 96 ? secretValue.Value.Value[..96][..96] : secretValue.Value.Value),-96}");
        rv += ($" {secretValue.Value.Properties.ContentType}\n");
      }
    }
    catch (Exception ex) { Trace.WriteLine($"@@ {ex.Message.Replace("\n", " ").Replace("\r", " ")}"); return ex.Message; }

    return rv;
  }
}