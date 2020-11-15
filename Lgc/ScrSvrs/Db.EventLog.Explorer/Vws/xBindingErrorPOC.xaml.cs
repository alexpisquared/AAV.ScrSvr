using System.Windows;

namespace Db.EventLog.Explorer
{
    /// <summary>
    /// Interaction logic for xBindingErrorPOC.xaml
    /// </summary>
    public partial class xBindingErrorPOC : Window
    {
        public xBindingErrorPOC()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource pcLogicViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("pcLogicViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // pcLogicViewSource.Source = [generic data source]
        }
    }
}
