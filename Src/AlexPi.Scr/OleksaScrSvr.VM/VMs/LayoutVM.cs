namespace OleksaScrSvr.VM.VMs;
public partial class LayoutVM2 : ObservableValidator
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