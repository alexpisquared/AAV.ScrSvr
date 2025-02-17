using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

/// https://www.csharp.com/blogs/fetching-secrets-from-key-vault-in-net-console-app
/// mostly obsolete though

await M2();

static async Task M2()
{
  var client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
    async (string auth, string res, string scope) =>
    {
      var authContext = new AuthenticationContext(auth);
      var credential = new ClientCredential("ClientId", "ClientSecret");
      var result = await authContext.AcquireTokenAsync(res, credential);
      return result == null ? throw new InvalidOperationException("Failed to obtain the JWT token") : result.AccessToken;
    }));

  var secret = await client.GetSecretAsync("https://demopockv.vault.azure.net/secrets/SecretName/SecretVersion", "TestSecretKey");
  Console.WriteLine($"Secret: {secret.Value}");
}
