using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;
using LiveCharts;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for UebersichtControl.xaml
    /// </summary>
    public partial class UebersichtControl : UserControl
    {
        public UebersichtControl()
        {
            Items = new SeriesCollection();
            InitializeComponent();
            FillUebersichtGrid();

            if (DataAccess.ReadStartDiagramm() == enStartDiagramm.enEinAusgaben)
            {
                HeaderGrafik.Header = "Ein- und Ausgaben";
                FillChartEinAusgaben();
            }
            else
            {
                HeaderGrafik.Header = "Kontosaldo";
                FillChartKontoSaldo();
            }
        }

        #region Properties
        string WaehrungsId
        {
            get { return DataAccess.ReadSetupWaehrung(); }
        }

        public SeriesCollection Items { get; set; }
        #endregion Properties

        private void FillUebersichtGrid()
        {
            var kontoList = DataAccess.ReadKontos();
            var uebersichtList = new List<Uebersicht>();
            var buchungsList = DataAccess.ReadBuchungen(0);
            decimal totalSaldoHeute = 0m;
            decimal totalSaldo = 0m;

            foreach (var konto in kontoList)
            {
                Uebersicht uebersicht = new Uebersicht();
                uebersicht.Konto = konto.Bezeichnung;
                uebersicht.WaehrungId = WaehrungsId;
				uebersicht.KontoId = konto.Id;

                // Saldo
                decimal saldoGutschrift = buchungsList.Where(x => x.Konto == konto.Id && (x.Typ == (int)enTyp.Gutschrift || x.Typ == (int)enTyp.TransferGutschrift)).Sum(x => x.Betrag);
                decimal saldoZahlung = buchungsList.Where(x => x.Konto == konto.Id && (x.Typ == (int)enTyp.Zahlung || x.Typ == (int)enTyp.TransferZahlung)).Sum(x => x.Betrag);
                uebersicht.Saldo = konto.Saldo + saldoGutschrift - saldoZahlung;

                decimal saldoGutschriftToday = buchungsList.Where(x => x.Konto == konto.Id && x.Datum <= DateTime.Today &&  (x.Typ == (int)enTyp.Gutschrift || x.Typ == (int)enTyp.TransferGutschrift)).Sum(x => x.Betrag);
                decimal saldoZahlungToday = buchungsList.Where(x => x.Konto == konto.Id && x.Datum <= DateTime.Today && (x.Typ == (int)enTyp.Zahlung || x.Typ == (int)enTyp.TransferZahlung)).Sum(x => x.Betrag);
                uebersicht.SaldoToday = konto.Saldo + saldoGutschriftToday - saldoZahlungToday;

                uebersichtList.Add(uebersicht);

                totalSaldoHeute += uebersicht.SaldoToday;
                totalSaldo += uebersicht.Saldo;
            }

            GridUebersicht.ItemsSource = uebersichtList;

            TotalSaldoHeute.Text = totalSaldoHeute.ToString("#,#.00");
            TotalSaldo.Text = totalSaldo.ToString("#,#.00");
        }

        private void FillChartKontoSaldo()
        {
            // https://lvcharts.net/App/examples/v1/Wpf/Install

            DataContext = this;
            Items = new SeriesCollection();

            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            foreach (Uebersicht item in GridUebersicht.ItemsSource)
            {
                // Nur Konti mit positivem Saldo anzeigen
                if (item.Saldo >= 0)
                {
                    data.Add(item.Konto, item.Saldo);
                }
            }

            Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y.ToString("###,###,##0.00"), chartPoint.Participation);

            foreach (var item in data)
            {
                var curValues = new List<decimal>() { item.Value };
                ISeriesView series = new PieSeries
                {
                    Title = item.Key,
                    Values = new ChartValues<decimal>(curValues),
                    DataLabels = true,
                    PushOut = 1, 
                    LabelPoint = labelPoint
                };
                Items.Add(series);
            }
        }

        private void FillChartEinAusgaben()
        {
            // https://lvcharts.net/App/examples/v1/Wpf/Install

            DataContext = this;
            Items = new SeriesCollection();

            var buchungsListe = DataAccess.ReadBuchungen(0).Where(x => x.Datum.Year == DateTime.Now.Year);

            decimal einnahmen = (from betrag in buchungsListe select betrag.Gutschrift).Sum();
            decimal ausgaben = (from betrag in buchungsListe select betrag.Zahlung).Sum();

            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            data.Add("Einnahmen", einnahmen);
            data.Add("Ausgaben", ausgaben);
            
            Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y.ToString("###,###,##0.00"), chartPoint.Participation);

            foreach (var item in data)
            {
                var curValues = new List<decimal>() { item.Value };
                ISeriesView series = new PieSeries
                {
                    Title = item.Key,
                    Values = new ChartValues<decimal>(curValues),
                    DataLabels = true,
                    PushOut = 1,
                    LabelPoint = labelPoint
                };
                Items.Add(series);
            }
        }

        private void GridUebersicht_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Uebersicht item = (Uebersicht) GridUebersicht.SelectedItem;
            if (item == null)
                return;

            var cBuchen = new BuchenControl(item.KontoId);
            this.Content = cBuchen;
        }
    }
}