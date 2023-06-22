namespace OleksaScrSvr.View;
public partial class BmsPermMgrSymtrlView : UserControl
{
  IBmsPermMgrSymtrlVMLtd _vm = default!;
  bool _loaded;
  string _lastSelectUserId = "", _lastSelectRoleid = "";

  public BmsPermMgrSymtrlView() => InitializeComponent();
  void OnLoaded(object s, RoutedEventArgs e)
  {
    ArgumentNullException.ThrowIfNull(DataContext as IBmsPermMgrSymtrlVMLtd, nameof(_vm));
    _vm = (IBmsPermMgrSymtrlVMLtd)DataContext;
    _loaded = true;
    _ = cbxApps.Focus();
  }
  void DgPerm_SelectedCellsChanged(object s, SelectedCellsChangedEventArgs e)
  {
    if (!_loaded || e.AddedCells.Count < 1 || ((DataGrid)s).SelectedCells[0].Column.Header as string == "Granted") return;

    colPG.Visibility = colPg.Visibility = Visibility.Collapsed;
    colUG.Visibility = colUg.Visibility = Visibility.Visible;
    var selectedRole = (Role)e.AddedCells[0].Item;
    _lastSelectRoleid = selectedRole.RoleName;
    _lastSelectUserId = "";
    _vm.ReloadUsersForSelectRole(selectedRole);
    selectedRole.Selectd = true;
    dgPerm.Items.Refresh();
    dgUser.Items.Refresh();
  }
  void DgUser_SelectedCellsChanged(object s, SelectedCellsChangedEventArgs e)
  {
    if (!_loaded || e.AddedCells.Count < 1 || ((DataGrid)s).SelectedCells[0].Column.Header as string == "Granted") return;

    colPG.Visibility = colPg.Visibility = Visibility.Visible;
    colUG.Visibility = colUg.Visibility = Visibility.Collapsed;
    var selectedUser = (User)e.AddedCells[0].Item;
    _lastSelectUserId = selectedUser.UserName;
    _lastSelectRoleid = "";
    _vm.ReloadRolesForSelectUser(selectedUser);
    selectedUser.Selectd = true;
    dgUser.Items.Refresh();
    dgPerm.Items.Refresh();
  }
  async void OnToggleGrant(object s, RoutedEventArgs e) => await _vm.ToggleGrant(((FrameworkElement)s).DataContext, _lastSelectUserId, _lastSelectRoleid);  //void OnUnderConstruction(object s, RoutedEventArgs e) => _ = MessageBox.Show("Under Construction...", "Under Construction...", MessageBoxButton.OK, MessageBoxImage.Information);
}