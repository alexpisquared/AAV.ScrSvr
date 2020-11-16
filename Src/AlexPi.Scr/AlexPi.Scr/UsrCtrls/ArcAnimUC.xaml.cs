using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AlexPi.Scr.UsrCtrls
{
    /// <summary>
    /// Interaction logic for ArcAnimUC.xaml
    /// </summary>
    public partial class ArcAnimUC : UserControl
    {
        Storyboard /*sbMoveHands, */sbRotoMobil;
        static DoubleAnimation _da = new DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(1.0)) };
        public ArcAnimUC()
        {
            InitializeComponent();
            sbRotoMobil = (FindResource("sbRotoMobil") as Storyboard);
            Loaded += onLoaded;
        }
        async void onLoaded(object s, RoutedEventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(10000);

            var idleAt = App.Started.AddSeconds(-App.ScrSvrTimeoutSec);
            //StartMin = idleAt.Minute * 6 + idleAt.Second * .1;
            //StartHou = idleAt.TimeOfDay.TotalDays * 720;

            sbRotoMobil.Begin();
            while (true)
            {
                toggleAnimModes(null); /**/ await Task.Delay(64000);
                toggleAnimModes(false);/**/ await Task.Delay(32000);
                toggleAnimModes(true); /**/ await Task.Delay(16000);
            }
        }

        public static readonly DependencyProperty SecOpacityProperty = DependencyProperty.Register("SecOpacity", typeof(double), typeof(ArcAnimUC), new UIPropertyMetadata(1.0d));     /**/public double SecOpacity { get { return (double)GetValue(SecOpacityProperty); } set { SetValue(SecOpacityProperty, value); } }
        public static readonly DependencyProperty BlurRaduisProperty = DependencyProperty.Register("BlurRaduis", typeof(double), typeof(ArcAnimUC), new UIPropertyMetadata(30d));      /**/public double BlurRaduis { get { return (double)GetValue(BlurRaduisProperty); } set { SetValue(BlurRaduisProperty, value); } }
        public static readonly DependencyProperty SecHandVisProperty = DependencyProperty.Register("SecHandVis", typeof(Visibility), typeof(ArcAnimUC)); public Visibility SecHandVis { get { return (Visibility)GetValue(SecHandVisProperty); } set { SetValue(SecHandVisProperty, value); } }

        void toggleAnimModes(bool? isRoto)
        {
            if (isRoto == true)       // shortest
            {
                _da.To = 0; BeginAnimation(BlurRaduisProperty, _da);
                _da.To = 0; BeginAnimation(SecOpacityProperty, _da); // 
                SecHandVis = Visibility.Collapsed;
                sbRotoMobil.Resume();
            }
            else if (isRoto == false) // medium
            {
                _da.To = 0; BeginAnimation(BlurRaduisProperty, _da);
                _da.To = 1; BeginAnimation(SecOpacityProperty, _da); // 
                SecHandVis = Visibility.Visible;
                sbRotoMobil.Pause();
                //todo: restore if needed. HandSec.BeginStoryboard(sbMoveHands);
            }
            else                      // longest 
            {
                _da.To = 20; BeginAnimation(BlurRaduisProperty, _da);
                _da.To = 00; BeginAnimation(SecOpacityProperty, _da); // 
                SecHandVis = Visibility.Collapsed;
                sbRotoMobil.Pause();
                //sbMoveHands.Pause();
            }
        }


    }
}
