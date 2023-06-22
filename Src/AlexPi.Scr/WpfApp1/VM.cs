namespace WpfApp1;
public partial class VM : ObservableValidator
{
  [ObservableProperty] LayoutVM layout1 = new();
  [ObservableProperty] LayoutVM layout2 = new();
  [ObservableProperty] LayoutVM layout3 = new();
  [ObservableProperty] LayoutVM layout4 = new();
  [ObservableProperty] LayoutVM layout5 = new();
}

public partial class LayoutVM : ObservableValidator
{
  [ObservableProperty] double top;
  [ObservableProperty] double left;
  [ObservableProperty] double right;
  [ObservableProperty] double bottom;
  [ObservableProperty] double zoom;
}