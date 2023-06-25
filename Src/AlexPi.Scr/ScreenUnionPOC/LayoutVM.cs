namespace ScreenUnionPOC;

public partial class LayoutVM : ObservableValidator
{
  [ObservableProperty] double top;
  [ObservableProperty] double left;
  [ObservableProperty] double width;
  [ObservableProperty] double height;
  [ObservableProperty] double right;
  [ObservableProperty] double bottom;
  [ObservableProperty] double zoom;
  [ObservableProperty] bool windowState;
}