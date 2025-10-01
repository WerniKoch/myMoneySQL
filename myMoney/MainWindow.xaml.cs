using DeviceId;
using myMoney.Controls;
using myMoney.Database;
using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace myMoney
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer Timer;
        readonly double PanelWidth;
        bool IsHidden;

        public MainWindow()
        {
            try
            {
                CreateNewTables.CreateTables();

            }
            catch (Exception)
            {
                MessageBox.Show("Datenbank Fehler. Bitte stellen sie sicher, dass die Applikation Schreibrechte im Programmverzeichnis hat.", "Datenbank Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            SetSprache();

            if (!CheckLizenz())
            {
                Close();
            }

            if (!CheckPasswort())
            {
                Close();
            }


            InitializeComponent();

            DataAccess.CreateDirectory();

            Timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 1)
            };

            Timer.Tick += Timer_Tick;
            PanelWidth = sidePanel.Width;

            ClickOnUebersicht(this, null);

            VersionsNummer.Text = "Version " + ProgramVersion;
        }

        public string ProgramVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() ?? string.Empty; }
        }

        #region Timer
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (IsHidden)
            {
                sidePanel.Width += 5;
                if (sidePanel.Width >= PanelWidth)
                {
                    Timer.Stop();
                    IsHidden = false;
                }
            }
            else
            {
                sidePanel.Width -= 5;
                if (sidePanel.Width <= 35)
                {
                    Timer.Stop();
                    IsHidden = true;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Timer.Start();
        }

        private void PanelHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        #endregion Timer

        #region Buttons

        private void ClickOnUebersicht(object sender, MouseButtonEventArgs? e)
        {
            var cUebersicht = new UebersichtControl();
            Control.Content = cUebersicht;
        }

        private void ClickOnBuchen(object sender, MouseButtonEventArgs e)
        {
            var cBuchen = new BuchenControl();
            Control.Content = cBuchen;
        }

        private void ClickOnPeriodisch(object sender, MouseButtonEventArgs e)
        {
            var cPeriodisch = new PeriodischControl();
            Control.Content = cPeriodisch;
        }
        
        private void ClickOnStammKonto(object sender, MouseButtonEventArgs e)
        {
            var cStammKonto = new StammKontoControl();
            Control.Content = cStammKonto;
        }

        private void ClickOnStammKategorien(object sender, MouseButtonEventArgs e)
        {
            var cStammKategorien = new StammKategorienControl();
            Control.Content = cStammKategorien;
        }

        private void ClickOnStammReportPDF(object sender, MouseButtonEventArgs e)
        {
            var cReport = new ReportControl();
            Control.Content = cReport;
        }

        private void ClickOnStammReportKategorie(object sender, MouseButtonEventArgs e)
        {
            var cReport = new ReportKategorieControl();
            Control.Content = cReport;
        }

        private void ClickOnStammSetup(object sender, MouseButtonEventArgs e)
        {
            var cSetup = new SetupControl();
            Control.Content = cSetup;
        }

        private void ClickOnStammInfo(object sender, MouseButtonEventArgs e)
        {
            var cInfo = new InfoControl();
            Control.Content = cInfo;
        }

        private void ClickOnStammPeriodisch(object sender, MouseButtonEventArgs e)
        {
            var cStammPeriodisch = new StammPeriodischControl();
            Control.Content = cStammPeriodisch;
        }

        private void ClickOnJahresabschluss(object sender, MouseButtonEventArgs e)
        {
            var cJahresabschluss = new JahresabschlussControl();
            Control.Content = cJahresabschluss;
        }

        private void ClickOnButtonClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ClickOnButtonMinimaze(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            BackupData();
        }
        #endregion Buttons

        #region Backup
        private void BackupData()
        {
            List<string> liste = DataAccess.GetDataFilesToBackup();
#if DEBUG
            string backupDirectory = Environment.CurrentDirectory + "\\Backup\\";
#else
            string backupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\Backup\\";
#endif

            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            string backupDatei = backupDirectory + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_Backup.zip";
            using (ZipArchive zip = ZipFile.Open(backupDatei, ZipArchiveMode.Create))
            {
                foreach (var item in liste)
                {
                    string fileName = Path.GetFileName(item);
                    if (File.Exists(fileName))
                    {
                        zip.CreateEntryFromFile(item, fileName);
                    }
                }
            }
        }
        #endregion Backup

        #region Sprache
        private void SetSprache()
        {
            enSprache sprache = DataAccess.ReadSprache();
            if (sprache == enSprache.FR)
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr");
        }
        #endregion Sprache

        #region Passwort
        private bool CheckPasswort()
        {
            if (!DataAccess.ReadPasswortAktiv())
                return true;

            PasswortWindow win = new PasswortWindow();
            if (win.ShowDialog() ?? false)
                return true;

            return false;
        }
        #endregion Passwort

        #region Lizenz
        private bool CheckLizenz()
        {
#if DEBUG
            return true;
#endif
            string deviceId = new DeviceIdBuilder()
                                .AddMachineName()
                                .AddOsVersion()
                                .AddFileToken("Lizenz4myMoney")
                                .ToString();

            string encodedStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(deviceId));
            string freischaltcode = DataAccess.ReadCode();

            if (freischaltcode == encodedStr)
            {
                DataAccess.SetLizenzOK();
                return true;
            }

            if (!string.IsNullOrEmpty(freischaltcode))
            {
                MessageBox.Show("Der gespeicherte Freischaltcode ist ungültig. Beantragen sie einen neuen Code.", "Freischaltcode", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            //string inputStr = Encoding.UTF8.GetString(Convert.FromBase64String(encodedStr));

            LizenzWindow win = new LizenzWindow(deviceId);
            if (win.ShowDialog() ?? false)
                return true;

            return false;
        }
        #endregion Lizenz
    }
}
