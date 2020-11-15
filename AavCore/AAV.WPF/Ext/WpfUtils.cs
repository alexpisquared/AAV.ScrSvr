using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AAV.WPF.Ext
{
  public static class WpfUtils
  {
    public static Window? FindParentWindow(this FrameworkElement element)
    {
      if (element?.Parent == null) return null;

      if (element.Parent as Window != null) return element.Parent as Window;

      var frel = element.Parent as FrameworkElement;
      if (frel != null) return FindParentWindow(frel);

      return null;
    }

    public static void AutoInvokeOnUiThread(Action action)
    {
      _ = action ?? throw new ArgumentNullException(paramName: nameof(action)); // == if (action == null) throw new ArgumentNullException(paramName: nameof(action));

      if (Application.Current.Dispatcher.CheckAccess()) 
        action();
      else Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => 
        action()));
    }
    public static bool? AutoInvokeOnUiThread(Func<bool?> action)
    {
      _ = action ?? throw new ArgumentNullException(paramName: nameof(action)); // == if (action == null) throw new ArgumentNullException(paramName: nameof(action));

      bool? rv = false;

      if (Application.Current.Dispatcher.CheckAccess())
        rv = action();
      else Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        rv = action()));

      return rv;
    }
    public static async Task AutoInvokeOnUiThreadAsync(Func<Task> action)
    {
      _ = action ?? throw new ArgumentNullException(paramName: nameof(action)); // == if (action == null) throw new ArgumentNullException(paramName: nameof(action));

      if (Application.Current.Dispatcher.CheckAccess())
        await action().ConfigureAwait(false);
      else await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () =>
        await action().ConfigureAwait(false)));
    }

    public static void AutoInvokeOnUiThread(Action<string> action, string param0)
    {
      _ = action ?? throw new ArgumentNullException(paramName: nameof(action)); // == if (action == null) throw new ArgumentNullException(paramName: nameof(action));

      if (Application.Current.Dispatcher.CheckAccess())
        action(param0);
      else Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        action(param0)));
    }


    public static void AutoInvokeOnUiThread(Action<DependencyProperty, object> action, DependencyProperty dp, object value)
    {
      _ = action ?? throw new ArgumentNullException(paramName: nameof(action)); // == if (action == null) throw new ArgumentNullException(paramName: nameof(action));

      if (Application.Current.Dispatcher.CheckAccess()) 
        action(dp, value);
      else Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        action(dp, value)));
    }
  }
}
