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
            BuchungList = new List<Buchung>();
            TextList = new List<string>();
            
            Init();
        }

        // Aufrufendes Konto selektieren
        public BuchenControl(int kontoId)
        {
            BuchungList = new List<Buchung>();
            TextList = new List<string>();
            
            Init();
            cbKontoSelektion.SelectedValue = kontoId;
        }

        private async void Init()
        {
            InitializeComponent();
            LastDate = DateTime.Now.Date;
            StackPanelRadio.IsEnabled = false;
            FillCombos();
            DataContext = this;
            BuchungList = DataAccess.ReadBuchungen(0);
            TextList = await DataAccess.GetBuchungstexte();

            if (BuchungList.Count == 0)
            {
                New();
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
            New();
        }

        private void New()
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

            DeleteBuchung(SelectedBuchung);
            FillGrid(SelectedBuchung.Konto);
        }

        private void DeleteBuchung(Buchung buchung)
        {
            DataAccess.DeleteBuchung(buchung);

            // Bei Transfer Gegenbuchung auch löschen
            if (buchung.Typ == (int)enTyp.TransferZahlung || buchung.Typ == (int)enTyp.TransferGutschrift)
            {
                Buchung transbuchung = new Buchung() { Id = buchung.TransferId };
                DataAccess.DeleteBuchung(transbuchung);
            }

            FillGrid(buchung.Konto);
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

            enTyp typ = enTyp.Gutschrift;
            if (raTypZahlung.IsChecked ?? false)
            {
                typ = enTyp.Zahlung;
            }
            else if (raTypTransfer.IsChecked ?? false)
            {
                typ = enTyp.TransferZahlung;
            }

            Buchung buchung = new Buchung();
            buchung.Konto = (int)cbKontoSelektion.SelectedValue;
            buchung.Kategorie = (int)(cbKategorie.SelectedValue ?? Guid.Empty);
            buchung.Datum = dteDatum.DisplayDate;
            buchung.WaehrungsId = txtWhgId.Text;
            buchung.BuchText = txtBuchtext.Text;
            buchung.Betrag = Betrag;
            buchung.TransferId = 0;
            buchung.Typ = (int)typ;

            int transferId = 0;
            if (IsNew)
            {
                transferId = DataAccess.WriteBuchung(buchung);
                buchung.Id = transferId;
            }
            else
            {
                buchung.Id = SelectedBuchung.Id;
                buchung.TransferId = SelectedBuchung.TransferId;
                DataAccess.UpdateBuchung(buchung);
            }

            // Bei Transfer Gegenbuchung auch schreiben
            if (typ == enTyp.TransferZahlung)
            {
                Buchung transBuchung = new Buchung();
                transBuchung.Konto = (int) cbEmpfangKonto.SelectedValue;
                transBuchung.Kategorie = (int)(cbKategorie.SelectedValue ?? 0);
                transBuchung.Datum = dteDatum.DisplayDate;
                transBuchung.WaehrungsId = txtWhgId.Text;
                transBuchung.BuchText = txtBuchtext.Text;
                transBuchung.Betrag = Betrag;
                transBuchung.TransferId = transferId;
                transBuchung.Typ = (int)enTyp.TransferGutschrift;

                if (IsNew)
                {
                    transferId = DataAccess.WriteBuchung(transBuchung);

                    buchung.TransferId = transferId;
                    DataAccess.UpdateBuchung(buchung);
                }
                else
                {
                    transBuchung.Id = SelectedBuchung.TransferId;
                    transBuchung.TransferId = buchung.Id;
                    DataAccess.UpdateBuchung(transBuchung);
                }

            }

            LastDate = dteDatum.DisplayDate;
            IsNew = false;
            StackPanelRadio.IsEnabled = false;
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
        private void FillCombos()
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

            if (SelectedBuchung.Typ == (int)enTyp.Gutschrift)
            {
                raTypGutschrift.IsChecked = true;
            }
            else if (SelectedBuchung.Typ == (int)enTyp.TransferGutschrift || SelectedBuchung.Typ == (int)enTyp.TransferZahlung)
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
                var transBuchung = DataAccess.ReadBuchungen(0).FirstOrDefault(x => x.Id == SelectedBuchung.TransferId);
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

            int kontoId = (int)item.SelectedValue;
            FillGrid(kontoId);
        }

        private void FillGrid(int kontoId)
        { 
            var kontoList = DataAccess.ReadKontos();
            var buchungsList = DataAccess.ReadBuchungen(kontoId);
            var kategorieList = DataAccess.ReadKategorien(false);

            decimal saldo = kontoList?.FirstOrDefault(x => x.Id == kontoId)?.Saldo ?? 0.0m;
            decimal saldoHeute = 0.0m;

            buchungsList.Reverse();

            foreach (var buchung in buchungsList)
            {
                if (buchung.Typ == (int)enTyp.Zahlung || buchung.Typ == (int)enTyp.TransferZahlung)
                {
                    saldo -= buchung.Betrag;
                    buchung.Zahlung = buchung.Betrag;
                }
                else
                {
                    saldo += buchung.Betrag;
                    buchung.Gutschrift = buchung.Betrag;
                }

                buchung.Saldo = saldo;

                if (buchung.Datum <= DateTime.Now) 
                    saldoHeute = saldo;

                if (buchung.Kategorie == 0)
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
            New();
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
