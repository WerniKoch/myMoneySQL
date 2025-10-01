using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for PeriodischControl.xaml
    /// </summary>
    public partial class PeriodischControl : UserControl
    {
        public PeriodischControl()
        {
            InitializeComponent();

            int anzahlTage;
            int.TryParse(DataAccess.ReadSetupAnzahlTage(), out anzahlTage);
            dtePerDatum.SelectedDate = DateTime.Now.AddDays(anzahlTage);
            BtnVerbuchen.IsEnabled = false;
        }

        #region Properties
        List<Periodisch> BuchungList { get; set; } = new List<Periodisch>();
        #endregion Properties

        #region Buttons
        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            var periBuchungen = DataAccess.ReadPeriBuchungen();
            List<Konto> kontoListe = DataAccess.ReadKontos();
            var kategorieList = DataAccess.ReadKategorien();

            BuchungList = new List<Periodisch>();

            foreach (var item in periBuchungen)
            {
                var buchung = new Periodisch();
                buchung.IsSelektiert = true;
                buchung.Datum = SetDatum(item.StartDatum, item.LastDatum);
                buchung.Konto = kontoListe?.Find(x => x.Id == item.Konto)?.Bezeichnung ?? string.Empty;
                buchung.BuchText = item.BuchText;
                buchung.Kategorie = kategorieList?.Find(x => x.Id == item.Kategorie)?.UnterOberKategorie ?? string.Empty;
                buchung.WaehrungsId = item.WaehrungsId;
                buchung.Betrag = item.Betrag;
                buchung.TypText = GetTyp(item.Typ);

                // Nicht im Grid angezeigt
                buchung.KontoId = item.Konto;
                buchung.Typ = item.Typ;
                buchung.KategorieId = item.Kategorie;
                buchung.GegenKontoId = item.EmpfangsKonto;
                buchung.Id = item.Id;

                if (buchung.Datum <= dtePerDatum.SelectedDate)
                {
                    BuchungList.Add(buchung);
                }
            }

            DataGrid.ItemsSource = BuchungList;
            BtnVerbuchen.IsEnabled = true;

            Cursor = Cursors.Arrow;
        }

        private void Button_Click_Verbuchen(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            var buchungListe = DataAccess.ReadBuchungen(Guid.Empty);
            var gridListe = DataGrid.ItemsSource;
            var periBuchungen = DataAccess.ReadPeriBuchungen();


            bool isVerbucht = false;
            Guid transferGuid = Guid.Empty;

            foreach (Periodisch item in gridListe)
            {
                if (!item.IsSelektiert)
                    continue;

                Buchung buchung = new Buchung();
                buchung.Id = Guid.NewGuid();
                buchung.Konto = item.KontoId;
                buchung.Kategorie = item.KategorieId;
                buchung.Datum = item.Datum;
                buchung.WaehrungsId = item.WaehrungsId;
                buchung.BuchText = item.BuchText;
                buchung.Betrag = item.Betrag;
                buchung.TransferId = transferGuid;
                buchung.Typ = item.Typ;

                buchungListe.Add(buchung);
                isVerbucht = true;

                // Bei Transfer Gegenbuchung auch schreiben
                if (item.Typ == enTyp.TransferZahlung)
                {
                    Buchung transBuchung = new Buchung();
                    transBuchung.Id = transferGuid;
                    transBuchung.Konto = item.GegenKontoId;
                    transBuchung.Kategorie = item.KategorieId;
                    transBuchung.Datum = item.Datum;
                    transBuchung.WaehrungsId = item.WaehrungsId;
                    transBuchung.BuchText = item.BuchText;
                    transBuchung.Betrag = item.Betrag;
                    transBuchung.TransferId = buchung.Id;
                    transBuchung.Typ = enTyp.TransferGutschrift;

                    buchungListe.Add(transBuchung);
                }

                // Letzte Ausführung setzen (LastDatum)
                var actPeriBhg = periBuchungen.Find(x => x.Id == item.Id);
                if (actPeriBhg != null)
                {
                    actPeriBhg.LastDatum = item.Datum;
                }
            }

            Cursor = Cursors.Arrow;

            if (isVerbucht)
            {
                DataAccess.WriteBuchungen(buchungListe);
                DataAccess.WritePeriBuchungen(periBuchungen);

                MessageBox.Show("Periodische Buchungen wurden verbucht", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnVerbuchen.IsEnabled = false;
            }
        }
        #endregion Buttons

        #region Tools
        private string GetTyp(enTyp typ)
        {
            if (typ == enTyp.Zahlung)
                return "Zahlung";
            if (typ == enTyp.Gutschrift)
                return "Gutschrift";

            return "Transfer";
        }

        // Monat und Jahr ersetzen 
        // Falls Monatsende Tag finden und setzen
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S6562:Always set the \"DateTimeKind\" when creating new \"DateTime\" instances", Justification = "<Pending>")]
        private DateTime SetDatum(DateTime startDatum, DateTime lastDatum)
        {
            if (lastDatum == new DateTime(1, 1, 1))
            {
                return startDatum;
            }

            // War es beim letzen Mal ende Monat?
            if (DateTime.DaysInMonth(startDatum.Year, startDatum.Month) == lastDatum.Day)
            {
                DateTime datum = lastDatum.AddMonths(1);
                int day = DateTime.DaysInMonth(datum.Year, datum.Month);
                return new DateTime(datum.Year, datum.Month, day);
            }

            return lastDatum.AddMonths(1);
        }
        #endregion Tools
    }
}
