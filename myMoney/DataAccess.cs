using Microsoft.Win32;
using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace myMoney
{
    public static class DataAccess
    {
        const string PWEncodeDecode = "MoreMoney4All";
        private static bool IsDemoLizenz = true;

        public static void CreateDirectory()
        {
#if RELEASE
            string verz = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myGuest\\Daten";
            if (!Directory.Exists(verz))
            {
                var ret = Directory.CreateDirectory(verz);
            }
#endif
        }

        #region Backup
        // Liste aller Files zum Backupen
        public static string GetDataFileToBackup()
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.GetDataFile();
        }
        #endregion Backup

        #region Kontos
        public static List<Konto> ReadKontos()
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.ReadKontos();
        }

        public static void AddKonto(Konto konto)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.AddKonto(konto);
        }

        public static void UpdateKonto(Konto konto)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.UpdateKonto(konto);
        }

        public static void DeleteKonto(Konto konto)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.DeleteKonto(konto);
        }
        #endregion Kontos

        #region Kategorien
        public static void AddKategorie(Kategorie kategorie)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.AddKategorie(kategorie);
        }

        public static void UpdateKategorie(Kategorie kategorie)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.UpdateKategorie(kategorie);
        }

        public static void DeleteKategorie(Kategorie kategorie)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.DeleteKategorie(kategorie);
        }

        public static List<Kategorie> ReadKategorien(bool isAlle=true)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.ReadKategorien();
        }

        public static List<string> ReadOberKategorien()
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.ReadOberKategorien();
        }
        #endregion Kategorien

        #region Buchungen
        public static List<Buchung> ReadBuchungen(int konto)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.ReadBuchungen(konto);
        }

        public static int WriteBuchung(Buchung buchung)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.WriteBuchung(buchung);
        }

        public static void UpdateBuchung(Buchung buchung)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.UpdateBuchung(buchung);
        }

        public static void DeleteBuchung(Buchung buchung)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.DeleteBuchung(buchung);
        }

        // Jeden Text nur einmal 
        public static async Task<List<string>> GetBuchungstexte()
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return await dataAccessSQL.GetBuchungstexte();
        }

        public static int GetOldestJahr()
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.GetOldestJahr();
        }
        #endregion Buchungen

        #region PeriBuchungen
        public static List<PeriBuchung> ReadPeriBuchungen()
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.ReadPeriBuchungen();
        }

        public static void WritePeriBuchungen(List<PeriBuchung> list)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.WritePeriBuchungen(list);
        }

        public static void DeletePeriBuchung(PeriBuchung peribuchung)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.DeletePeriBuchung(peribuchung);
        }

        public static void AddPeriBuchung(PeriBuchung peribuchung)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.AddPeriBuchung(peribuchung);
        }

        public static void UpdatePeriBuchung(PeriBuchung peribuchung)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.UpdatePeriBuchung(peribuchung);
        }

        public static void UpdatePeriBuchungDatum(int id, DateTime datum)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.UpdatePeriBuchungDatum(id, datum);
        }
        #endregion PeriBuchungen

        #region Setup
        public static Setup ReadSetup()
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            return dataAccessSQL.ReadSetup();
        }

        public static void SetSetup(Setup setup)
        {
            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            dataAccessSQL.SetSetup(setup);
        }

        public static string ReadSetupWaehrung()
        {
            string whgid = "CHF";

            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            Setup setup = dataAccessSQL.ReadSetup();

            if (setup != null && !string.IsNullOrEmpty(setup.WaehrungId))
            {
                whgid = setup.WaehrungId;
            }

            return whgid;
        }

        public static string ReadSetupAnzahlTage()
        {
            string anzahlTage = "10";

            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            Setup setup = dataAccessSQL.ReadSetup();

            if (setup != null)
            {
                anzahlTage = setup.AnzahlTage.ToString();
            }

            return anzahlTage;
        }

        public static enSprache ReadSprache()
        {
            enSprache sprache = enSprache.DE;

            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            Setup setup = dataAccessSQL.ReadSetup();

            if (setup != null && !string.IsNullOrEmpty(setup.Sprache))
            {
                if (setup.Sprache == "FR")
                    sprache = enSprache.FR;
                else
                    sprache = enSprache.DE;
            }

            return sprache;
        }

        public static string ReadFont()
        {
            string font = "Arial";

            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            Setup setup = dataAccessSQL.ReadSetup();

            if (setup != null && !string.IsNullOrEmpty(setup.Font))
            {
                font = setup.Font;
            }

            return font;
        }

        public static string ReadPasswort()
        {
            string passwort = "";

            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            Setup setup = dataAccessSQL.ReadSetup();

            if (setup != null && !string.IsNullOrEmpty(setup.Passwort))
            {
                passwort = setup.Font;
            }

            return DecodePasswort(passwort, PWEncodeDecode);
        }

        public static bool ReadPasswortAktiv()
        {
            bool passwortAktiv = false;

            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            Setup setup = dataAccessSQL.ReadSetup();

            if (setup != null)
            {
                passwortAktiv = setup.PasswortAktiv;
            }

            return passwortAktiv;
        }

        public static enStartDiagramm ReadStartDiagramm()
        {
            enStartDiagramm startDiagramm = enStartDiagramm.enEinAusgaben;

            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            Setup setup = dataAccessSQL.ReadSetup();

            if (setup != null)
            {
                if (setup.StartDiagramm == (int)enStartDiagramm.enKontosaldi)
                {
                    startDiagramm = enStartDiagramm.enKontosaldi;
                }
            }

            return startDiagramm;
        }

        public static int ReadVorschlagKonto()
        {
            int konto = 0;

            DataAccessSQL dataAccessSQL = new DataAccessSQL();
            Setup setup = dataAccessSQL.ReadSetup();

            if (setup != null)
            {
                konto = setup.Vorschlagkonto;
            }

            return konto;
        }

        private static string EncodePasswort(string plainText, string? password=null)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return plainText;

            try
            {
                var data = Encoding.Default.GetBytes(plainText);
                var pwd = !string.IsNullOrEmpty(password) ? Encoding.Default.GetBytes(password) : Array.Empty<byte>();
                var cipher = ProtectedData.Protect(data, pwd, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(cipher);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return string.Empty;
        }

        private static string DecodePasswort(string cipherText, string? password = null)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return cipherText;

            try
            {
                var cipher = Convert.FromBase64String(cipherText);
                var pwd = !string.IsNullOrEmpty(password) ? Encoding.Default.GetBytes(password) : Array.Empty<byte>();
                var data = ProtectedData.Unprotect(cipher, pwd, DataProtectionScope.CurrentUser);
                return Encoding.Default.GetString(data);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return string.Empty;
        }
        #endregion Setup

        #region Freischaltcode
        // Freischaltcode wird in die Registry geschrieben
        public static void WriteCode(string code)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\myMoney");

            //storing the values  
            key.SetValue("code", code);
            key.Close();
        }

        public static string ReadCode()
        {
            string code = string.Empty;

            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\myMoney"))
            {
                if (key != null)
                {
                    code = key.GetValue("code")?.ToString() ?? string.Empty;
                }
            }

            return code;
        }

        public static void SetLizenzOK()
        {
            IsDemoLizenz = false;
        }

        public static bool GetLizenzOK()
        {
            return !IsDemoLizenz;
        }
        #endregion Freischaltcode
    }
}
