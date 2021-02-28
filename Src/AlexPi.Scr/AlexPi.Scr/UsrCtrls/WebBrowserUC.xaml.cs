using Microsoft.Web.WebView2.Core;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AlexPi.Scr.UsrCtrls
{
  public partial class WebBrowserUC : UserControl
  {
    public WebBrowserUC()
    {
      InitializeComponent();
      InitializeAsync();
    }

    async void InitializeAsync() => await webview21.EnsureCoreWebView2Async(null);

    void Button_Click(object sender, RoutedEventArgs e) => goNavigateButton_Click(@"https://portal.azure.com");
    void Button_Clic2(object sender, RoutedEventArgs e) => goNavigateButton_Click(@"https://portal.azure.com/#blade/Microsoft_Azure_CostManagement/Menu/costanalysis/scope/%2Fsubscriptions%2F55444df9-fdd4-43d1-a454-16a90de75646/open/CostAnalysis/view/%7B%22currency%22%3A%22CAD%22%2C%22dateRange%22%3A%22Last7Days%22%2C%22query%22%3A%7B%22type%22%3A%22ActualCost%22%2C%22dataSet%22%3A%7B%22granularity%22%3A%22Daily%22%2C%22aggregation%22%3A%7B%22totalCost%22%3A%7B%22name%22%3A%22PreTaxCost%22%2C%22function%22%3A%22Sum%22%7D%7D%2C%22sorting%22%3A%5B%7B%22direction%22%3A%22ascending%22%2C%22name%22%3A%22UsageDate%22%7D%5D%2C%22grouping%22%3A%5B%7B%22type%22%3A%22Dimension%22%2C%22name%22%3A%22ResourceId%22%7D%5D%7D%2C%22timeframe%22%3A%22None%22%7D%2C%22chart%22%3A%22GroupedColumn%22%2C%22accumulated%22%3A%22false%22%2C%22pivots%22%3A%5B%7B%22type%22%3A%22Dimension%22%2C%22name%22%3A%22ServiceName%22%7D%2C%7B%22type%22%3A%22Dimension%22%2C%22name%22%3A%22ResourceLocation%22%7D%2C%7B%22type%22%3A%22Dimension%22%2C%22name%22%3A%22ResourceGroupName%22%7D%5D%2C%22scope%22%3A%22subscriptions%2F55444df9-fdd4-43d1-a454-16a90de75646%22%2C%22kpis%22%3A%5B%7B%22type%22%3A%22Forecast%22%2C%22enabled%22%3Atrue%7D%5D%2C%22displayName%22%3A%22AccumulatedCosts%22%7D");
    void goNavigateButton_Click(string url)
    {
#if true
      //wb1.Url = url;
#else
      var uri = new Uri(url, UriKind.RelativeOrAbsolute);

      // Only absolute URIs can be navigated to  
      if (!uri.IsAbsoluteUri)
      {
        MessageBox.Show("The Address URI must be absolute. For example, 'http://www.microsoft.com'");
        return;
      }

      //wb1.ScriptErrorsSuppressed = true;
      wb1.Navigate(uri);
#endif
    }
  }
}
