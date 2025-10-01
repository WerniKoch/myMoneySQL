using myMoney.DTO;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for JahresabschlussControl.xaml
    /// </summary>
    public partial class JahresabschlussControl : UserControl
    {
        public JahresabschlussControl()
        {
            InitializeComponent();
            Jahr = DataAccess.GetOldestJahr();
            SetLoeschJahr();
        }

        private int Jahr {  get; set; }

        private void SetLoeschJahr()
        {
            chkJahrLoeschen.Content = "Das Jahr " + Jahr.ToString() + " löschen?";
        }

        private void OnJahrLoeschen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            btnLoeschen.IsEnabled = chkJahrLoeschen.IsChecked ?? false;
        }

        private void OnBtnLoeschen_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Jahr wirklich löschen?", "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            var alleBuchungenList = DataAccess.ReadBuchungen(Guid.Empty);

            // Alle Konti durchloopen
            var kontoList = DataAccess.ReadKontos();
            foreach (Konto konto in kontoList)
            {
                // Alle Buchungen vom Jahr summieren und löschen
                decimal kontosumme = 0.0m;
                var buchungList = DataAccess.ReadBuchungen(konto.Id).Where(x => x.Datum.Year == Jahr).ToList();
                foreach (Buchung buchung in buchungList)
                {
                    kontosumme += buchung.BetragMitVorzeichen;

                    var item = alleBuchungenList.FirstOrDefault(x => x.Id == buchung.Id);
                    if (item == null)
                        continue;

                    alleBuchungenList.Remove(item);
                }
                
                // Kontosaldo nachtragen
                if (kontosumme != 0.0m)
                {
                    konto.Saldo += kontosumme;
                }
            }

            DataAccess.WriteBuchungen(alleBuchungenList);
            DataAccess.WriteKontos(kontoList);
        }
    }
}
