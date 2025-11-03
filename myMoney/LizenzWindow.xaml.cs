using System;
using System.Windows;
using System.Windows.Threading;

namespace myMoney
{
    /// <summary>
    /// Interaction logic for LizenzWindow.xaml
    /// </summary>
    public partial class LizenzWindow : Window
    {
        #region Properties
        private int Seconds {  get; set; } = 30;
        private DispatcherTimer Timer { get; set; }
        #endregion Properties

        public LizenzWindow(string deviceId)
        {
            Timer = new DispatcherTimer();
            InitializeComponent();

            tbCode.Text = deviceId;
            StartTimer();
        }

        private void StartTimer()
        {
            tbZeit.Text = Seconds.ToString();

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        #region Events
        private void Timer_Tick(object? sender, EventArgs e)
        {
            Seconds--;
            tbZeit.Text = Seconds.ToString();
            
            if (Seconds == 0)
            {
                Timer.Stop();
                btnSchliessen.IsEnabled = true;
            }
        }

        private void Button_Speichern_Click(object sender, RoutedEventArgs e)
        {
            // Freischaltcode speichern
            if (!string.IsNullOrWhiteSpace(tbFreischaltCode.Text))
            {
                DataAccess.WriteCode(tbFreischaltCode.Text);
            }
        }

        private void Button_Schliessen_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_Kopieren_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbCode.Text);
        }
        #endregion Events
    }
}

