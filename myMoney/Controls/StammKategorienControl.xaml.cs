using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for StammKategorienControl.xaml
    /// </summary>
    public partial class StammKategorienControl : UserControl
    {
        public StammKategorienControl(bool isShowAll=true)
        {
            InitializeComponent();
            IsShowAll = isShowAll;
            FillKategorien();
            txtOberkategorie.IsEnabled = false;
            txtUnterkategorie.IsEnabled = false;
            IsNew = true;
            BtnLoeschen.IsEnabled = false;
            BtnNew.IsEnabled = false;

            if (KategorieList.Count == 0)
            {
                Button_Click_New(null, null);
            }
        }

        #region Properties
        List<Kategorie> KategorieList { get; set; } = new List<Kategorie>();
        bool IsNew { get; set; }
        Guid SaveKategorieId { get; set; }
        bool IsShowAll { get; set; }
        #endregion Properties

        #region Tree
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SaveKategorieId = Guid.Empty;

            var tree = sender as TreeView;
            if (tree == null)
                return;

            IsNew = false;

            if (tree.SelectedItem is TreeViewItem)
            {
                var item = tree.SelectedItem as TreeViewItem;
                if (item != null && item.Tag != null)
                {
                    Guid id = (Guid)item.Tag;
                    SaveKategorieId = id;
                    FillOneKategorie(id);
                }
            }

            BtnLoeschen.IsEnabled = true;
            BtnNew.IsEnabled = true;
            txtOberkategorie.IsEnabled = true;
            txtUnterkategorie.IsEnabled = true;
        }
        #endregion Tree

        #region Buttons
        private void Button_Click_New(object sender, System.Windows.RoutedEventArgs e)
        {
            IsNew = true;
            BtnLoeschen.IsEnabled = false;
            BtnNew.IsEnabled = false;
            txtUnterkategorie.Text = string.Empty;
            txtOberkategorie.IsEnabled = true;
            txtUnterkategorie.IsEnabled = true;
            cbInaktiv.IsChecked = false;
            txtUnterkategorie.Focus();
        }

        private void Button_Click_Delete(object sender, System.Windows.RoutedEventArgs e)
        {
            txtFehler.Width = 0;

            if (MessageBox.Show("Kategorie wirlich löschen?", "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            var item = TreeViewKategorie.SelectedItem as TreeViewItem;
            if (item == null)
                return;

            Kategorie? kategorie = KategorieList.FirstOrDefault(x => x.Id == (Guid)item.Tag);
            if (kategorie == null)
                return;

            /* Kategorie darf nicht bebucht sein */
            var kategorienListe = DataAccess.ReadBuchungen(Guid.Empty).Where(x => x.Kategorie == kategorie.Id).ToList();
            if (kategorienListe.Count > 0)
            {
                txtFehler.Text = "Kategorie ist bebucht und kann nicht gelöscht werden";
                txtFehler.Width = 300;
                return;
            }

            KategorieList.Remove(kategorie);
            DataAccess.WriteKategorien(KategorieList);
            FillKategorien();
        }

        private void Button_Click_Save(object sender, System.Windows.RoutedEventArgs e)
        {
            txtFehler.Width = 0;

            if (string.IsNullOrEmpty(txtOberkategorie.Text))
            {
                txtOberkategorie.Focus();
                txtFehler.Text = "Oberkategorie fehlt";
                txtFehler.Width = 300;
                return;
            }

            if (string.IsNullOrEmpty(txtUnterkategorie.Text))
            {
                txtUnterkategorie.Focus();
                txtFehler.Text = "Unterkategorie fehlt";
                txtFehler.Width = 300;
                return;
            }

            if (IsNew)
            {
                Kategorie kategorie = new Kategorie() { OberKategorie = txtOberkategorie.Text, UnterKategorie = txtUnterkategorie.Text, Inaktiv = cbInaktiv.IsChecked ?? false };
                KategorieList.Add(kategorie);
            }
            else
            {
                var item = KategorieList.FirstOrDefault(x => x.Id == SaveKategorieId);
                if (item != null)
                {
                    item.OberKategorie = txtOberkategorie.Text;
                    item.UnterKategorie = txtUnterkategorie.Text;
                    item.Inaktiv = cbInaktiv.IsChecked ?? false;
                }
            }

            DataAccess.WriteKategorien(KategorieList);
            FillKategorien();
            IsNew = false;
            BtnLoeschen.IsEnabled = true;
            BtnNew.IsEnabled = true;
        }
        #endregion Buttons

        #region Tools
        private void FillKategorien()
        {
            KategorieList = DataAccess.ReadKategorien(IsShowAll);
            TreeKategorien.Items.Clear();

            Kategorie? oldKategorie = null;
            TreeViewItem item = new TreeViewItem();

            foreach (var kategorie in KategorieList)
            {
                if (oldKategorie == null || oldKategorie.OberKategorie != kategorie.OberKategorie)
                {
                    item = new TreeViewItem();

                    StackPanel stack = new StackPanel { Orientation = Orientation.Horizontal};
                    stack.Children.Add(new Image { Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\Icons\menu.png")), Width = 16, Height = 16 });

                    stack.Children.Add(new TextBlock { Text = kategorie.OberKategorie });
                    item.Header = stack;
                    item.Tag = kategorie.Id;

                    TreeKategorien.Items.Add(item);
                }

                var subItem1 = new TreeViewItem();
                subItem1.Header = kategorie.UnterKategorie;
                subItem1.Tag = kategorie.Id;
                item.Items.Add(subItem1);

                oldKategorie = kategorie;
            }
        }

        private void FillOneKategorie(Guid id)
        {
            Kategorie? kategorie = KategorieList.FirstOrDefault(x => x.Id == id);
            if (kategorie == null)
                return;

            txtOberkategorie.Text = kategorie.OberKategorie;
            txtUnterkategorie.Text = kategorie.UnterKategorie;
            cbInaktiv.IsChecked = kategorie.Inaktiv;
        }
        #endregion Tools
    }
}
