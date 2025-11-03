using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for StammKontoControl.xaml
    /// </summary>
    public partial class StammKontoControl : UserControl, INotifyPropertyChanged
    {
        public StammKontoControl()
        {
            InitializeComponent();
            DataContext = this;
            GetKontoData();
        }

        #region Properties
        List<Konto> KontoList { get; set; } = new List<Konto>();
        bool IsNew { get; set; }

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

        Konto SelectedKonto
        {
            get
            {
                _selectedKonto.Nummer = txtKonto.Text;
                _selectedKonto.Institution = txtInstitution.Text;
                _selectedKonto.Bezeichnung = txtBezeichnung.Text;
                _selectedKonto.WaehrungId = txtWhgId.Text;
                _selectedKonto.Saldo = Betrag;
                return _selectedKonto;
            }
            set
            {
                _selectedKonto = value;
                txtKonto.Text = _selectedKonto.Nummer;
                txtInstitution.Text = _selectedKonto.Institution;
                txtBezeichnung.Text = _selectedKonto.Bezeichnung;
                txtWhgId.Text = _selectedKonto.WaehrungId;
                Betrag = _selectedKonto.Saldo;
            }
        }
        Konto _selectedKonto = new Konto();

        string WaehrungsId
        {
            get { return DataAccess.ReadSetupWaehrung(); }
        }
        #endregion Properties

        #region Buttons
        private void Button_Click_New(object sender, RoutedEventArgs e)
        {
            IsNew = true;
            SelectedKonto = new Konto() { WaehrungId = WaehrungsId, Saldo=0.00m };
            BtnLoeschen.IsEnabled = false;
            BtnNew.IsEnabled = false;
            txtKonto.Focus();
        }

        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            txtFehler.Width = 0;

            if (IsNew)
                return;

            if (MessageBox.Show("Konto löschen?", "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            var item = DataGrid.SelectedItem as Konto;
            if (item == null)
                return;

            /* Konto darf nicht bebucht sein */
            /*            var buchungsListe = DataAccess.ReadBuchungen(item.Id);
                        if (buchungsListe.Count > 0)
                        {
                            txtFehler.Text = "Konto ist bebucht und kann nicht gelöscht werden";
                            txtFehler.Width = 300;
                            return;
                        }
            */
            DataAccess.DeleteKonto(SelectedKonto);
            GetKontoData();
        }

        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            if (!DataAccess.GetLizenzOK() && IsNew && KontoList.Count >= 3)
            {
                MessageBox.Show("In der Demoversion können maximal 3 Konti gespeichert werden", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            txtFehler.Width = 0;

            if (string.IsNullOrWhiteSpace(txtKonto.Text))
            {
                txtFehler.Text = "Das Konto fehlt";
                txtFehler.Width = 300;
                txtKonto.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtBezeichnung.Text))
            {
                txtFehler.Text = "Die Bezeichnung fehlt";
                txtFehler.Width = 300;
                txtBezeichnung.Focus();
                return;
            }

            if (IsNew)
            {
                DataAccess.AddKonto(SelectedKonto);
            }
            else
            {
                DataAccess.UpdateKonto(SelectedKonto);
            }

            IsNew = false;
            GetKontoData();
        }

        private void Click_Up_Button(object sender, RoutedEventArgs e)
        {
            int id = SelectedKonto.Id;
            int pos = KontoList.FindIndex(x => x.Id == SelectedKonto.Id);
            if (pos <= 0)
            {
                return;
            }

            KontoList.RemoveAt(pos);
            pos--;
            KontoList.Insert(pos, SelectedKonto);

            int count = 1;
            foreach (var item in KontoList)
            {
                item.Position = count;
                count++;    
            }

            foreach (var item in KontoList)
            {
                DataAccess.UpdateKonto(item);
            }

            IsNew = false;
            GetKontoData();

            DataGrid.SelectedItem = KontoList[pos];
        }

        private void Click_Down_Button(object sender, RoutedEventArgs e)
        {
            int pos = KontoList.FindIndex(x => x.Id == SelectedKonto.Id);
            if (pos >= KontoList.Count-1)
            {
                return;
            }

            KontoList.RemoveAt(pos);
            pos++;
            KontoList.Insert(pos, SelectedKonto);

            int count = 1;
            foreach (var item in KontoList)
            {
                item.Position = count;
                count++;
            }

            foreach (var item in KontoList)
            {
                DataAccess.UpdateKonto(item);
            }

            IsNew = false;
            GetKontoData();

            DataGrid.SelectedItem = KontoList[pos];
        }
        #endregion Buttons

        #region Tools
        private void GetKontoData()
        {
            KontoList = DataAccess.ReadKontos();
            DataGrid.ItemsSource = KontoList;

            BtnLoeschen.IsEnabled = true;
            BtnNew.IsEnabled = true;

            if (DataGrid.Items.Count > 0)
            {
                DataGrid.SelectedItem = KontoList[0];
            }
            else
            {
                Button_Click_New(new object(), new RoutedEventArgs());
            }
        }

        private void DataGridCell_Selected(object sender, RoutedEventArgs e)
        {
            var item = DataGrid.SelectedItem as Konto;
            if (item == null)
                return;

            SelectedKonto = item;
            IsNew = false;
            BtnLoeschen.IsEnabled = true;
            BtnNew.IsEnabled = true;
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
