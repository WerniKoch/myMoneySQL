using System.Windows;

namespace myMoney
{
    /// <summary>
    /// Interaction logic for PasswortWindow.xaml
    /// </summary>
    public partial class PasswortWindow : Window
    {
        public PasswortWindow()
        {
            InitializeComponent();

            txtPasswort.Focus();
        }

        private void Button_Schliessen_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            string pw = DataAccess.ReadPasswort();
            if (pw != txtPasswort.Password )
            {
                MessageBox.Show("Das Passwort ist falsch", "Loginfehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            DialogResult = true;

            Close();
        }
    }
}
