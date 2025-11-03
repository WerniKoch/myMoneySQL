using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for StammPeriodischControl.xaml
    /// </summary>
    public partial class StammPeriodischControl : UserControl, INotifyPropertyChanged
    {
        public StammPeriodischControl()
        {
            InitializeComponent();
            DataContext = this;

            InitFelder();
            GetPeriBuchungData();
        }

        #region Init
        private void InitFelder()
        {
            // Periodizität
            List<string> periList = new List<string>();
            foreach (var item in Enum.GetValues(typeof(enPeriodizitaet)))
            {
                periList.Add(item.ToString() ?? "NULL");
            }
            cbPeriodizitaet.ItemsSource = periList;
            cbPeriodizitaet.SelectedIndex = 0;

            dteStartDatum.SelectedDate = DateTime.Today;
            //DteLastDatum.SelectedDate = DateTime.Today;

            // Konto
            var kontoList = DataAccess.ReadKontos();
            cbKonto.ItemsSource = kontoList;
            cbKonto.DisplayMemberPath = "Bezeichnung";
            cbKonto.SelectedValuePath = "Id";
            cbKonto.SelectedIndex = 0;

            // Empfangskonto
            var kontoList2 = DataAccess.ReadKontos();
            cbEmpfangKonto.ItemsSource = kontoList2;
            cbEmpfangKonto.DisplayMemberPath = "Bezeichnung";
            cbEmpfangKonto.SelectedValuePath = "Id";
            //cbEmpfangKonto.SelectedIndex = 0;

            // Kategorie
            var katList = DataAccess.ReadKategorien();
            cbKategorie.ItemsSource = katList;
            cbKategorie.DisplayMemberPath = "UnterOberKategorie";
            cbKategorie.SelectedValuePath = "Id";
        }
        #endregion Init

        #region Properties
        bool IsNew { get; set; }
        List<PeriBuchung> PeriBuchungList { get; set; } = new List<PeriBuchung>();

        private decimal _betrag;
        public decimal Betrag
        {
            get
            {
                return _betrag;
            }
            set
            {
                _betrag = value;
                OnPropertyChanged();
            }
        }

        private void GetPeriBuchungData()
        {
            PeriBuchungList = new List<PeriBuchung>();
            PeriBuchungList = DataAccess.ReadPeriBuchungen();
            DataGrid.ItemsSource = PeriBuchungList;

            BtnLoeschen.IsEnabled = true;
            BtnNew.IsEnabled = true;

            if (DataGrid.Items.Count > 0)
                DataGrid.SelectedItem = PeriBuchungList[0];
            else
                Button_Click_New(new object(), new RoutedEventArgs());
        }

        PeriBuchung SelectedPeriBuchung
        {
            get
            {
                return _selectedPeriBuchung;
            }
            set
            {
                _selectedPeriBuchung = value;
                SetData();
            }
        }
        PeriBuchung _selectedPeriBuchung = new PeriBuchung();

        string WaehrungsId
        {
            get { return DataAccess.ReadSetupWaehrung(); }
        }
        #endregion Properties

        #region Buttons
        private void Button_Click_New(object sender, System.Windows.RoutedEventArgs e)
        {
            IsNew = true;
            BtnLoeschen.IsEnabled = false;
            BtnNew.IsEnabled = false;
            SelectedPeriBuchung = new PeriBuchung() { StartDatum = DateTime.Now.Date, WaehrungsId = WaehrungsId, LastDatum = new DateTime(1,1,1)};
            SetData();
            dteStartDatum.Focus();
        }

        private void Button_Click_Delete(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsNew)
                return;

            if (MessageBox.Show("Periodische Buchung wirklich löschen?", "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            var item = DataGrid.SelectedItem as PeriBuchung;
            if (item == null)
                return;

            DataAccess.DeletePeriBuchung(SelectedPeriBuchung);

            GetPeriBuchungData();
        }

        private void Button_Click_Save(object sender, System.Windows.RoutedEventArgs e)
        {
            txtFehler.Width = 0;
            if (cbKonto.SelectedValue == null)
            {
                txtFehler.Text = "Das Konto fehlt";
                txtFehler.Width = 300;
                cbKonto.Focus();
                return;
            }

            if (raTypTransfer.IsChecked == true && cbEmpfangKonto.SelectedValue == null)
            {
                txtFehler.Text = "Das Empfangskonto fehlt";
                txtFehler.Width = 300;
                cbEmpfangKonto.Focus();
                return;
            }

            if (cbKategorie.SelectedValue == null)
            {
                txtFehler.Text = "Die Kategorie fehlt";
                txtFehler.Width = 300;
                cbKategorie.Focus();
                return;
            }

            if (raTypTransfer.IsChecked == true && cbKonto.SelectedValue == cbEmpfangKonto.SelectedValue)
            {
                txtFehler.Text = "Beide Kontos sind identisch";
                txtFehler.Width = 300;
                cbEmpfangKonto.Focus();
                return;
            }

            enTyp typ = enTyp.Gutschrift;
            if (raTypZahlung.IsChecked ?? false)
            {
                typ = enTyp.Zahlung;
            }
            else if (raTypTransfer.IsChecked ?? false)
            {
                typ = enTyp.TransferZahlung;
            }

            SelectedPeriBuchung.Konto = (int) cbKonto.SelectedValue;
            SelectedPeriBuchung.EmpfangsKonto = (int) (cbEmpfangKonto.SelectedValue ?? 0);
            SelectedPeriBuchung.Kategorie = (int)(cbKategorie.SelectedValue ?? 0);
            SelectedPeriBuchung.StartDatum = dteStartDatum.DisplayDate;
            SelectedPeriBuchung.LastDatum = dteLastDatum.DisplayDate;
            SelectedPeriBuchung.WaehrungsId = txtWhgId.Text;
            SelectedPeriBuchung.BuchText = txtBuchtext.Text;
            SelectedPeriBuchung.Betrag = Betrag;
            SelectedPeriBuchung.Typ = (int)typ;

            if (IsNew)
            {
                DataAccess.AddPeriBuchung(SelectedPeriBuchung);
            }
            else
            {
                DataAccess.UpdatePeriBuchung(SelectedPeriBuchung);
            }

            IsNew = false;
            GetPeriBuchungData();
        }
        #endregion Buttons

        #region Tools
        private void DataGridCell_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = DataGrid.SelectedItem as PeriBuchung;
            if (item == null)
                return;

            SelectedPeriBuchung = item;
            IsNew = false;
            BtnLoeschen.IsEnabled = true;
            BtnNew.IsEnabled = true;
        }

        private void SetData()
        {
            dteStartDatum.SelectedDate = SelectedPeriBuchung.StartDatum;
            dteLastDatum.SelectedDate = SelectedPeriBuchung.LastDatum;
            txtWhgId.Text = SelectedPeriBuchung.WaehrungsId;
            txtBuchtext.Text = SelectedPeriBuchung.BuchText;
            Betrag = SelectedPeriBuchung.Betrag;

            cbKonto.SelectedValue = SelectedPeriBuchung.Konto;
            cbEmpfangKonto.SelectedValue = SelectedPeriBuchung.EmpfangsKonto;
            cbKategorie.SelectedValue = SelectedPeriBuchung.Kategorie;

            if (SelectedPeriBuchung.Typ == (int)enTyp.Gutschrift)
            {
                raTypGutschrift.IsChecked = true;
            }
            else if (SelectedPeriBuchung.Typ == (int)enTyp.TransferZahlung)
            {
                raTypTransfer.IsChecked = true;
            }
            else
            {
                raTypZahlung.IsChecked = true;
            }
        }
        #endregion Tools

        #region Betrag
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void txtBetrag_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                int pos = txtBetrag.Text.IndexOf(".");
                txtBetrag.Select(++pos, 0);
                e.Handled = true;
                return;
            }

            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtBetrag_GotFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var c = new DispatcherTimer();
            c.Interval = TimeSpan.FromMilliseconds(100);
            c.Tick += (a1, a2) =>
            {
                c.IsEnabled = false;
                txtBetrag.SelectAll();
            };
            c.IsEnabled = true;
        }
        #endregion Betrag
    }
}
