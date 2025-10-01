using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.IO.Compression;
using myMoney.DTO;
using System.Drawing.Text;

namespace myMoney.Controls
{
    /// <summary>
    /// Interaction logic for SetupControl.xaml
    /// </summary>
    public partial class SetupControl : UserControl
    {
#if DEBUG
        private string BackupDirectory = Environment.CurrentDirectory + "\\Backup\\";
        private string ProgrammVerzeichnis = Environment.CurrentDirectory + "\\";
#else
        private string BackupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\Backup\\";
        private string ProgrammVerzeichnis = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\";
#endif

        public SetupControl()
        {
            InitializeComponent();
            LoadData();
        }

        #region Buttons
        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            enStartDiagramm startDiagramm = enStartDiagramm.enEinAusgaben;
            if (radioKontoSaldi.IsChecked == true)
                startDiagramm = enStartDiagramm.enKontosaldi;

            Guid konto = Guid.Empty;
            if (cbKonto.SelectedValue != null)
            {
                konto = (Guid)cbKonto.SelectedValue;
            }

            string datenhaltung = (radioXML.IsChecked ?? true) ? "XML" : "SQL";

            DataAccess.WriteSetup(txtWhgId.Text, txtAnzahlTage.Text, (enSprache) cbSprache.SelectedValue, (string) cbFont.SelectedValue,
                txtPasswort.Text, checkBoxPasswort.IsChecked ?? false, konto, startDiagramm, datenhaltung, txtDatenbank.Text, txtDBUser.Text, txtDBPasswort.Text);
        }

        private void Button_Click_Restore(object sender, RoutedEventArgs e)
        {
            BackupData selItem = (BackupData) cbBackup.SelectedItem;
            if (selItem == null)
                return;

            RestoreBackup(selItem.DateiZIP);
        }
        #endregion Buttons

        #region Tools
        private void LoadData()
        {
            txtWhgId.Text = DataAccess.ReadSetupWaehrung();
            txtAnzahlTage.Text = DataAccess.ReadSetupAnzahlTage();
            txtPasswort.Text = DataAccess.ReadPasswort();
            checkBoxPasswort.IsChecked = DataAccess.ReadPasswortAktiv();

            enStartDiagramm startDiagramm = DataAccess.ReadStartDiagramm();
            if (startDiagramm == enStartDiagramm.enKontosaldi)
                radioKontoSaldi.IsChecked = true;
            if (startDiagramm == enStartDiagramm.enEinAusgaben)
                radioEinAusgaben.IsChecked = true;

            LoadBackups();
            LoadSprachen();
            LoadFonts();
            LoadKontos();

            string datenhaltung = DataAccess.ReadDatenhaltung();
            if (datenhaltung == "XML")
            {
                radioXML.IsChecked = true;
            }
            else
            {
                radioSQL.IsChecked = true;
            }

            txtDatenbank.Text = DataAccess.ReadDatenbank();
            txtDBUser.Text = DataAccess.ReadDBUser();
            txtDBPasswort.Text = DataAccess.ReadDBPasswort();
        }

        private void LoadBackups()
        {
            List<BackupData> backupList = new List<BackupData>();

            if (!Directory.Exists(BackupDirectory))
            {
                Directory.CreateDirectory(BackupDirectory);
            }

            var directoryAndFileList = Directory.GetFiles(BackupDirectory, "*.zip", SearchOption.TopDirectoryOnly);
            
            foreach (var dirFile in directoryAndFileList)
            {
                var file = Path.GetFileName(dirFile);
                var version = "Backup vom " + file.Substring(6, 2) + "." + file.Substring(4, 2) + "." + file.Substring(0, 4) + " " + file.Substring(9, 2) + ":" + file.Substring(11, 2) + ":" + file.Substring(13, 2);

                BackupData data = new BackupData() { DateiZIP = file, DateiAnzeigen = version };
                backupList.Insert(0, data);
            }

            cbBackup.DisplayMemberPath = "DateiAnzeigen";
            cbBackup.ItemsSource = backupList;
        }

        private void LoadSprachen()
        {
            Dictionary<enSprache, string> dictionary = new Dictionary<enSprache, string>
            {
                { enSprache.DE, Properties.Resources.Text_Deutsch },
                { enSprache.FR, Properties.Resources.Text_Franzoesisch }
            };

            cbSprache.ItemsSource = dictionary;
            cbSprache.DisplayMemberPath = "Value";
            cbSprache.SelectedValuePath = "Key";
            cbSprache.SelectedValue = DataAccess.ReadSprache();
        }

        private void LoadFonts()
        {
            List<string> fontsList = new List<string>();

            using (InstalledFontCollection col = new InstalledFontCollection())
            {
                foreach (System.Drawing.FontFamily fa in col.Families)
                {
                    fontsList.Add(fa.Name);
                }
            }

            cbFont.ItemsSource = fontsList;
            cbFont.SelectedValue = DataAccess.ReadFont();
        }

        private void LoadKontos()
        {
            var kontoList = DataAccess.ReadKontos();
            cbKonto.ItemsSource = kontoList;
            cbKonto.DisplayMemberPath = "Bezeichnung";
            cbKonto.SelectedValuePath = "Id";
            cbKonto.SelectedValue = DataAccess.ReadVorschlagKonto();
        }

        private void RestoreBackup(string zip)
        {
            if (MessageBox.Show("ACHTUNG: Die Daten werden überschrieben. Backup zurückholen?", "Restore", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            using (ZipArchive zipItem = ZipFile.OpenRead(BackupDirectory + zip))
            {
                foreach (var item in zipItem.Entries)
                {
                    item.ExtractToFile(ProgrammVerzeichnis + item.FullName, true);
                }
            }

            LoadData();
        }
        #endregion Tools

        #region BackupData
        public class BackupData
        {
            public string DateiZIP { get; set; } = string.Empty;    
            public string DateiAnzeigen { get; set; } = string.Empty;
        }
        #endregion BackupData
    }
}
