using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for BuchenControl.xaml
    /// </summary>
    public partial class BuchenControl : UserControl, INotifyPropertyChanged
    {
        public BuchenControl()
        {
            Init();
        }

        // Aufrufendes Konto selektieren
        public BuchenControl(Guid kontoId)
        {
            Init();
            cbKontoSelektion.SelectedValue = kontoId;
        }

        private async void Init()
        {
            InitializeComponent();
            LastDate = DateTime.Now.Date;
            StackPanelRadio.IsEnabled = false;
            FillKonti();
            DataContext = this;
            BuchungList = DataAccess.ReadBuchungen(Guid.Empty);
            TextList = await DataAccess.GetBuchungstexte();

            if (BuchungList.Count == 0)
            {
                Button_Click_New(null, null);
            }
        }

        #region Properties
        bool IsNew { get; set; }
        List<Buchung> BuchungList { get; set; }
        List<string> TextList { get; set; }
        DateTime LastDate { get; set; }

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

        Buchung SelectedBuchung
        {
            get
            {
                return _selectedBuchung;
            }
            set
            {
                _selectedBuchung = value;
                SetData();
            }
        }
        Buchung _selectedBuchung = new Buchung();


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
            BtnNeu.IsEnabled = false;

            SelectedBuchung = new Buchung() { Datum = LastDate, WaehrungsId = WaehrungsId };
            SetData();
            dteDatum.Focus();

            StackPanelRadio.IsEnabled = true;
        }

        private void Button_Click_Delete(object sender, System.Windows.RoutedEventArgs e)
        {

            if (IsNew)
                return;

            if (MessageBox.Show("Buchung wirklich löschen?", "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            var item = DataGrid.SelectedItem as Buchung;
            if (item == null)
                return;

            DeleteBuchung(item.Id);

            DataAccess.WriteBuchungen(BuchungList);
            FillGrid(SelectedBuchung.Konto);
        }

        private void DeleteBuchung(Guid id)
        {
            var item = BuchungList.FirstOrDefault(x => x.Id == id);
            if (item == null)
                return;

            BuchungList.Remove(item);

            // Bei Transfer Gegenbuchung auch löschen
            if (item.Typ == enTyp.TransferZahlung || item.Typ == enTyp.TransferGutschrift)
            {
                var transItem = BuchungList.FirstOrDefault(x => x.Id == item.TransferId);
                if (transItem != null)
                {
                    BuchungList.Remove(transItem);
                }
            }
        }

        private void Button_Click_Save(object sender, System.Windows.RoutedEventArgs e)
        {
            txtFehler.Width = 0;

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

            if (raTypTransfer.IsChecked == true && cbKontoSelektion.SelectedValue == cbEmpfangKonto.SelectedValue)
            {
                txtFehler.Text = "Beide Kontos sind identisch";
                txtFehler.Width = 300;
                cbEmpfangKonto.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtBuchtext.Text))
            {
                txtFehler.Text = "Buchungstext fehlt";
                txtFehler.Width = 300;
                txtBuchtext.Focus();
                return;
            }

            if (Betrag == 0m)
            {
                txtFehler.Text = "Betrag fehlt";
                txtFehler.Width = 300;
                txtBetrag.Focus();
                return;
            }

            if (!IsNew)
            {
                var item = DataGrid.SelectedItem as Buchung;
                if (item == null)
                    return;

                DeleteBuchung(item.Id);
            }

            Guid transferGuid = Guid.Empty;
            enTyp typ = enTyp.Gutschrift;
            if (raTypZahlung.IsChecked ?? false)
            {
                typ = enTyp.Zahlung;
            }
            else if (raTypTransfer.IsChecked ?? false)
            {
                typ = enTyp.TransferZahlung;
                transferGuid = Guid.NewGuid();
            }

            Buchung buchung = new Buchung();
            buchung.Id = Guid.NewGuid();
            buchung.Konto = (Guid)cbKontoSelektion.SelectedValue;
            buchung.Kategorie = (Guid)(cbKategorie.SelectedValue ?? Guid.Empty);
            buchung.Datum = dteDatum.DisplayDate;
            buchung.WaehrungsId = txtWhgId.Text;
            buchung.BuchText = txtBuchtext.Text;
            buchung.Betrag = Betrag;
            buchung.TransferId = transferGuid;
            buchung.Typ = typ;

            BuchungList.Add(buchung);

            // Bei Transfer Gegenbuchung auch schreiben
            if (typ == enTyp.TransferZahlung)
            {
                Buchung transBuchung = new Buchung();
                transBuchung.Id = transferGuid;
                transBuchung.Konto = (Guid) cbEmpfangKonto.SelectedValue;
                transBuchung.Kategorie = (Guid)(cbKategorie.SelectedValue ?? Guid.Empty);
                transBuchung.Datum = dteDatum.DisplayDate;
                transBuchung.WaehrungsId = txtWhgId.Text;
                transBuchung.BuchText = txtBuchtext.Text;
                transBuchung.Betrag = Betrag;
                transBuchung.TransferId = buchung.Id;
                transBuchung.Typ = enTyp.TransferGutschrift;

                BuchungList.Add(transBuchung);
            }

            LastDate = dteDatum.DisplayDate;
            IsNew = false;
            StackPanelRadio.IsEnabled = false;
            DataAccess.WriteBuchungen(BuchungList);
            FillGrid(buchung.Konto);
        }

        private void Button_Click_Kategorie(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = new Window
            {
                Title = "Kategorien",
                Content = new StammKategorienControl(false),
                Width = 500,
                Height = 500
            };

            window.ShowDialog();

            // Combo neu fuellen
            var katList = DataAccess.ReadKategorien(false);
            cbKategorie.ItemsSource = katList;
            cbKategorie.DisplayMemberPath = "UnterOberKategorie";
            cbKategorie.SelectedValuePath = "Id";
        }
        #endregion Buttons

        #region Tools
        private void FillKonti()
        {
            var kontoList = DataAccess.ReadKontos();
            cbKontoSelektion.ItemsSource = kontoList;
            cbKontoSelektion.DisplayMemberPath = "Bezeichnung";
            cbKontoSelektion.SelectedValuePath = "Id";
            cbKontoSelektion.SelectedValue = DataAccess.ReadVorschlagKonto();

            var kontoList2 = DataAccess.ReadKontos();
            cbEmpfangKonto.ItemsSource = kontoList2;
            cbEmpfangKonto.DisplayMemberPath = "Bezeichnung";
            cbEmpfangKonto.SelectedValuePath = "Id";

            // Kategorie
            var katList = DataAccess.ReadKategorien(false);
            cbKategorie.ItemsSource = katList;
            cbKategorie.DisplayMemberPath = "UnterOberKategorie";
            cbKategorie.SelectedValuePath = "Id";
        }

        // Grid Selektion
        private void DataGridCell_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = DataGrid.SelectedItem as Buchung;
            if (item == null)
                return;

            SelectedBuchung = item;
            
            IsNew = false;
            StackPanelRadio.IsEnabled = false;
            BtnLoeschen.IsEnabled = true;
            BtnNeu.IsEnabled = true;
            txtFehler.Width = 0;
        }

        private void SetData()
        {
            dteDatum.SelectedDate = SelectedBuchung.Datum;
            txtWhgId.Text = SelectedBuchung.WaehrungsId;
            txtBuchtext.Text = SelectedBuchung.BuchText;
            Betrag = SelectedBuchung.Betrag;
            cbEmpfangKonto.SelectedValue = SelectedBuchung.TransferId;
            cbKategorie.SelectedValue = SelectedBuchung.Kategorie;

            if (SelectedBuchung.Typ == enTyp.Gutschrift)
            {
                raTypGutschrift.IsChecked = true;
            }
            else if (SelectedBuchung.Typ == enTyp.TransferGutschrift || SelectedBuchung.Typ == enTyp.TransferZahlung)
            {
                raTypTransfer.IsChecked = true;
            }
            else
            {
                raTypZahlung.IsChecked = true;
            }

            // Bei Tansfer Buchung holen und Konto setzen
            if (raTypTransfer.IsChecked == true)
            {
                var transBuchung = DataAccess.ReadBuchungen(Guid.Empty).FirstOrDefault(x => x.Id == SelectedBuchung.TransferId);
                if (transBuchung != null)
                {
                    cbEmpfangKonto.SelectedValue = transBuchung.Konto;
                }
            }
        }

        #endregion Tools

        #region ComboSelektion
        // Kontoauswahl 
        private void cbKontoSelektion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as ComboBox;
            if (item == null)
                return;

            Guid kontoId = (Guid)item.SelectedValue;
            FillGrid(kontoId);
        }

        private void FillGrid(Guid kontoId)
        { 
            var kontoList = DataAccess.ReadKontos();
            var buchungsList = DataAccess.ReadBuchungen(kontoId);
            var kategorieList = DataAccess.ReadKategorien(false);

            decimal saldo = kontoList?.FirstOrDefault(x => x.Id == kontoId)?.Saldo ?? 0.0m;
            decimal saldoHeute = 0.0m;

            buchungsList.Reverse();

            foreach (var buchung in buchungsList)
            {

                if (buchung.Typ == enTyp.Zahlung || buchung.Typ == enTyp.TransferZahlung)
                {
                    saldo -= buchung.Betrag;
                }
                else
                {
                    saldo += buchung.Betrag;
                }

                buchung.Saldo = saldo;

                if (buchung.Datum <= DateTime.Now) 
                    saldoHeute = saldo;

                if (buchung.Kategorie == Guid.Empty)
                    continue;

                buchung.KategorieText = kategorieList?.FirstOrDefault(x => x.Id == buchung.Kategorie)?.UnterOberKategorie ?? string.Empty;
            }

            buchungsList.Reverse();

            DataGrid.ItemsSource = buchungsList;

            //            if (DataGrid.Items.Count > 0)
            //                DataGrid.SelectedItem = buchungsList[0];
            //            else

            txtSaldoHeute.Text = saldoHeute.ToString("###,###,##0.00");
            txtSaldoZukunft.Text = saldo.ToString("###,###,##0.00");

            // Immer im New-Modus starten
            Button_Click_New(new object(), new RoutedEventArgs());

        }
        #endregion ComboSelektion

        #region Kategorie suchen
        private void TxtBuchtext_LostFocus(object sender, RoutedEventArgs e)
        {
            autoCompletorListPopup.IsOpen = false;

            if (!IsNew)
                return;
            if (cbKategorie.SelectedValue != null)
                return;

            var kategorie = BuchungList.FirstOrDefault(x => x.BuchText == txtBuchtext.Text)?.Kategorie;

            if (kategorie == null)
                return;

            cbKategorie.SelectedValue = kategorie;
        }
        #endregion Kategorie suchen

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

        #region AutoComplete       
        private void AutoCompletorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (autoCompletorList.SelectedItem != null)
                {
                    txtBuchtext.Text = autoCompletorList.SelectedValue.ToString();
                    autoCompletorListPopup.IsOpen = false;

                    var kategorie = BuchungList.FirstOrDefault(x => x.BuchText == txtBuchtext.Text)?.Kategorie;

                    if (kategorie == null)
                        return;

                    cbKategorie.SelectedValue = kategorie;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TxtBuchtext_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter) // Aktueller Wert uebernehmen
                {
                    if (autoCompletorList?.Items?.CurrentItem != null)
                    {
                        txtBuchtext.Text = autoCompletorList.Items.CurrentItem.ToString();
                        autoCompletorListPopup.IsOpen = false;
                    }

                    txtBetrag.Focus();
                }
                else
                {
                    if (txtBuchtext.Text.Trim() != "")
                    {
                        autoCompletorListPopup.IsOpen = true;
                        autoCompletorListPopup.Visibility = Visibility.Visible;
                        autoCompletorList.ItemsSource = TextList.Where(td => td.Trim().ToLower().Contains(txtBuchtext.Text.Trim().ToLower()));
                    }
                    else
                    {
                        autoCompletorListPopup.IsOpen = false;
                        autoCompletorListPopup.Visibility = Visibility.Collapsed;
                        autoCompletorList.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion AutoComplete       

        #region Grid
        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Buchung? buchung = e.Row.DataContext as Buchung;
            if (buchung != null && buchung.Datum > DateTime.Now)
            {
                e.Row.Background = new SolidColorBrush(Colors.BlanchedAlmond);
            }
        }
        #endregion Grid
    }
}
