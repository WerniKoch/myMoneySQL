using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;
using LiveCharts;
using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for ReportKategorieControl.xaml
    /// </summary>
    public partial class ReportKategorieControl : UserControl
    {
        #region Properties
        public SeriesCollection Items { get; set; }
        #endregion Properties

        public ReportKategorieControl()
        {
            InitializeComponent();

            SetDatum();
            FillKategorie();
        }

        #region SetData
        private void SetDatum()
        {
            int year = DateTime.Now.Year;
            dteDatumVon.SelectedDate = new DateTime(year, 1, 1);
            dteDatumBis.SelectedDate = new DateTime(year, 12, 31);
        }

        private void FillKategorie()
        {
            var kategorieListe = DataAccess.ReadOberKategorien();
            cbKategorie.ItemsSource = kategorieListe;
        }
        #endregion SetData

        #region Events
        private void Button_Anzeigen(object sender, System.Windows.RoutedEventArgs e)
        {
            string selKategorie = cbKategorie?.SelectedItem?.ToString() ?? string.Empty;
            if (selKategorie == string.Empty)
            {
                return;
            }

            // Alle Buchungen im Zeitraum
            var listBuchungen = DataAccess.ReadBuchungen(Guid.Empty).Where(x => x.Datum >= dteDatumVon.SelectedDate && x.Datum <= dteDatumBis.SelectedDate);
            // Alle Kategoriene mit selekierter Oberkategorie
            var kategorieList = DataAccess.ReadKategorien().Where(x => x.OberKategorie == selKategorie);

            List<ReportKategorie> reportKategories = new List<ReportKategorie>();

            foreach (var item in listBuchungen)
            {
                // Buchung aus der korrekten Kategorie
                var kat = kategorieList.Where(x => x.Id == item.Kategorie).FirstOrDefault();
                if (kat == null)
                    continue;

                var repKat = reportKategories.Where(x => x.Kategorie == kat.UnterKategorie).FirstOrDefault();
                if (repKat == null)
                {
                    ReportKategorie reportKategorie = new ReportKategorie
                    {
                        Kategorie = kat.UnterKategorie,
                        Summe = item.BetragMitVorzeichen
                    };
                    reportKategories.Add(reportKategorie);
                }
                else
                {
                    repKat.Summe += item.BetragMitVorzeichen;
                }
            }

            Items = new SeriesCollection();

            Dictionary<string, decimal> data = new Dictionary<string, decimal>();

            foreach (ReportKategorie item in reportKategories)
            {
                data.Add(item.Kategorie, Math.Abs(item.Summe));
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

            Chart.Series = Items;

            // Grid
            // DataContext = this;
            GridReportKategorie.ItemsSource = reportKategories;
            txtEinnahmen.Text = reportKategories.Where(x => x.Summe >= 0.0m).Sum(x => x.Summe).ToString();
            txtAusgaben.Text = reportKategories.Where(x => x.Summe < 0.0m).Sum(x => x.Summe).ToString();
            txtUmsatz.Text = (reportKategories.Where(x => x.Summe >= 0.0m).Sum(x => x.Summe) + reportKategories.Where(x => x.Summe < 0.0m).Sum(x => x.Summe)).ToString();
        }
        #endregion Events

    }
}

