namespace OleksaScrSvr.Views;
public partial class SqlNtvIpmPermMgrView : UserControl
{
  public SqlNtvIpmPermMgrView()
  {
    InitializeComponent();

    tbxUsrSearch.PreviewKeyDown += async (s, e) =>
    {
      switch (e.Key)
      {
        default: WriteLine($"{e.Key} is not handled"); await Task.Yield(); break;
      }
    };

    IsEnabledChanged += async (s, e) =>
    {
      if ((e.NewValue as bool?) != true)
      {
        _focusedControlK = Keyboard.FocusedElement; //FocusManager.GetFocusedElement(this);
      }
      else
      {
        await Task.Delay(50)/*!!.ConfigureAwait(false)*/;
        _ = (_focusedControlK ?? tbxUsrSearch).Focus();
      }
    };

    Loaded += async (s, e) => { await Task.Delay(3500)/*!!.ConfigureAwait(false)*/; _ = tbxUsrSearch.Focus(); };
  }

  IInputElement? _focusedControlK;
  public static readonly DependencyProperty FileDropCommandProperty = DependencyProperty.Register("FileDropCommand", typeof(ICommand), typeof(SqlNtvIpmPermMgrView), new PropertyMetadata(null)); public ICommand FileDropCommand { get => (ICommand)GetValue(FileDropCommandProperty); set => SetValue(FileDropCommandProperty, value); }

  static void OpenOrNavigate(string? filename, bool isOpen) { if (filename is not null && File.Exists(filename)) _ = Process.Start("Explorer.exe", isOpen ? filename : $"/select, \"{filename}\""); else _ = MessageBox.Show($"Filename \n\n{filename} \n\ndoes not exist", "Warning"); }
}