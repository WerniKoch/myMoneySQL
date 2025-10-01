using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using myMoney.DTO;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace myMoney
{
    public static class DataAccess
    {
#if DEBUG
        private static string XMLSetup = Environment.CurrentDirectory + "\\Setup.xml";
#else
        private static string XMLSetup = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\Setup.xml";
#endif

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
        public static List<string> GetDataFilesToBackup()
        {
            if (ReadDatenhaltung() == "XML")
            {
                DataAccessXML dataAccessXML = new DataAccessXML();
                return dataAccessXML.GetDataFilesToBackup();
            }
            else
            {
                MessageBox.Show("SQL not implemented yet!");
                return new List<string>();
            }
        }
        #endregion Backup

        #region Kontos
        public static List<Konto> ReadKontos()
        {
            if (ReadDatenhaltung() == "XML")
            {
                DataAccessXML dataAccessXML = new DataAccessXML();
                return dataAccessXML.ReadKontos();
            }
            else
            {
                DataAccessSQL dataAccessSQL = new DataAccessSQL();
                return dataAccessSQL.ReadKontos();
            }
        }

        public static void WriteKontos(List<Konto> list)
        {
            if (ReadDatenhaltung() == "XML")
            {
                DataAccessXML dataAccessXML = new DataAccessXML();
                dataAccessXML.WriteKontos(list);
            }
            else
            {
                DataAccessSQL dataAccessSQL = new DataAccessSQL();
                dataAccessSQL.WriteKontos(list);
            }
        }
        #endregion Kontos

        #region Kategorien
        public static List<Kategorie> ReadKategorien(bool isAlle=true)
        {
            if (ReadDatenhaltung() == "XML")
            {
                DataAccessXML dataAccessXML = new DataAccessXML();
                return dataAccessXML.ReadKategorien(isAlle);
            }
            else
            {
                DataAccessSQL dataAccessSQL = new DataAccessSQL();
                return dataAccessSQL.ReadKategorien();
            }
        }

        public static List<string> ReadOberKategorien()
        {
            if (ReadDatenhaltung() == "XML")
            {
                DataAccessXML dataAccessXML = new DataAccessXML();
                return dataAccessXML.ReadOberKategorien();
            }
            else
            {
                DataAccessSQL dataAccessSQL = new DataAccessSQL();
                return dataAccessSQL.ReadOberKategorien();
            }
        }

        public static void WriteKategorien(List<Kategorie> list)
        {
            if (ReadDatenhaltung() == "XML")
            {
                DataAccessXML dataAccessXML = new DataAccessXML();
                dataAccessXML.WriteKategorien(list);
            }
            else
            {
                DataAccessSQL dataAccessSQL = new DataAccessSQL();
                dataAccessSQL.WriteKategorien(list);
            }
        }
        #endregion Kategorien

        #region Buchungen
        public static List<Buchung> ReadBuchungen(Guid konto)
        {
            DataAccessXML dataAccessXML = new DataAccessXML();
            return dataAccessXML.ReadBuchungen(konto);
        }

        public static void WriteBuchungen(List<Buchung> list)
        {
            DataAccessXML dataAccessXML = new DataAccessXML();
            dataAccessXML.WriteBuchungen(list);
        }

        // Jeden Text nur einmal 
        public static async Task<List<string>> GetBuchungstexte()
        {
            DataAccessXML dataAccessXML = new DataAccessXML();
            return await dataAccessXML.GetBuchungstexte();
        }

        public static int GetOldestJahr()
        {
            DataAccessXML dataAccessXML = new DataAccessXML();
            return dataAccessXML.GetOldestJahr();
        }
        #endregion Buchungen

        #region PeriBuchungen
        public static List<PeriBuchung> ReadPeriBuchungen()
        {
            if (ReadDatenhaltung() == "XML")
            {
                DataAccessXML dataAccessXML = new DataAccessXML();
                return dataAccessXML.ReadPeriBuchungen();
            }
            else
            {
                DataAccessSQL dataAccessSQL = new DataAccessSQL();
                return dataAccessSQL.ReadPeriBuchungen();
            }
        }

        public static void WritePeriBuchungen(List<PeriBuchung> list)
        {
            if (ReadDatenhaltung() == "XML")
            {
                DataAccessXML dataAccessXML = new DataAccessXML();
                dataAccessXML.WritePeriBuchungen(list);
            }
            else
            {
                DataAccessSQL dataAccessSQL = new DataAccessSQL();
                dataAccessSQL.WritePeriBuchungen(list);
            }
        }
        #endregion PeriBuchungen

        #region Generisch
        private static List<T> Deserialize<T>(string xML)
        {
            List<T> list;
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (TextReader reader = new StreamReader(xML))
            {
                list = (List<T>)serializer.Deserialize(reader);
            }

            return list ?? new List<T> { };
        }

        private static void Serialize<T>(List<T> list, string xMl)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true; // CR LF im XML
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (XmlWriter writer = XmlWriter.Create(xMl, settings))
            {
                serializer.Serialize(writer, list);
            }
        }
        #endregion Generisch

        #region Setup
        public static void WriteSetup(string whgId, string anzahlTage, enSprache sprache, string font, string passwort, bool passwortAktiv, Guid vorschlagKonto, enStartDiagramm startDiagramm,
            string datenhaltung, string datenbank, string dbUser, string dbPasswort)
        {
            string enPasswort = EncodePasswort(passwort, PWEncodeDecode);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true; // CR LF im XML

            XmlWriter xmlWriter = XmlWriter.Create(XMLSetup, settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Setup");

            xmlWriter.WriteStartElement("Waehrung");
            xmlWriter.WriteString(whgId);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("AnzahlTage");
            xmlWriter.WriteString(anzahlTage);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Sprache");
            xmlWriter.WriteString(sprache.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Font");
            xmlWriter.WriteString(font);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Passwort");
            xmlWriter.WriteString(enPasswort);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("PasswortAktiv");
            xmlWriter.WriteString(passwortAktiv ? "True" : "False");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("VorschlagKonto");
            xmlWriter.WriteString(vorschlagKonto.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("StartDiagramm");
            xmlWriter.WriteString(startDiagramm.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("datenhaltung");
            xmlWriter.WriteString(datenhaltung.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("datenbank");
            xmlWriter.WriteString(datenbank.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("dbUser");
            xmlWriter.WriteString(dbUser.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("dbPasswort");
            xmlWriter.WriteString(dbPasswort.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        public static string ReadSetupWaehrung()
        {
            string whgid = "CHF";

            if (!File.Exists(XMLSetup))
                return whgid;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "Waehrung")
                {
                    whgid = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return whgid;
        }

        public static string ReadSetupAnzahlTage()
        {
            string anzahlTage = "10";

            if (!File.Exists(XMLSetup))
                return anzahlTage;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "AnzahlTage")
                {
                    anzahlTage = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return anzahlTage;
        }

        public static enSprache ReadSprache()
        {
            enSprache sprache = enSprache.DE;

            if (!File.Exists(XMLSetup))
                return sprache;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "Sprache")
                {
                    string sprachetext = xmlReader.ReadString();
                    if (sprachetext == "FR")
                        sprache = enSprache.FR;
                    else
                        sprache = enSprache.DE;

                    break;
                }
            }

            xmlReader.Close();

            return sprache;
        }

        public static string ReadFont()
        {
            string font = "Arial";

            if (!File.Exists(XMLSetup))
                return font;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "Font")
                {
                    font = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return font;
        }

        public static string ReadPasswort()
        {
            string passwort = "";

            if (!File.Exists(XMLSetup))
                return passwort;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "Passwort")
                {
                    passwort = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return DecodePasswort(passwort, PWEncodeDecode);
        }

        public static bool ReadPasswortAktiv()
        {
            if (!File.Exists(XMLSetup))
                return false;

            string passwortAktiv=string.Empty;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "PasswortAktiv")
                {
                    passwortAktiv = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return passwortAktiv == "True";
        }

        public static enStartDiagramm ReadStartDiagramm()
        {
            if (!File.Exists(XMLSetup))
                return enStartDiagramm.enEinAusgaben;

            string startDiagramm = string.Empty;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "StartDiagramm")
                {
                    startDiagramm = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            if (startDiagramm == "enKontosaldi")
                return enStartDiagramm.enKontosaldi;

            return enStartDiagramm.enEinAusgaben;
        }

        public static Guid ReadVorschlagKonto()
        {
            Guid konto = Guid.Empty;

            if (!File.Exists(XMLSetup))
                return konto;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "VorschlagKonto")
                {
                    konto = new Guid(xmlReader.ReadString());
                    break;
                }
            }

            xmlReader.Close();

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

        public static string ReadDatenhaltung()
        {
            string datenhaltung = "XML";

            if (!File.Exists(XMLSetup))
                return datenhaltung;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "datenhaltung")
                {
                    datenhaltung = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return datenhaltung;
        }

        public static string ReadDatenbank()
        {
            string datenbank = string.Empty;

            if (!File.Exists(XMLSetup))
                return datenbank;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "datenbank")
                {
                    datenbank = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return datenbank;
        }

        public static string ReadDBUser()
        {
            string dbUser = string.Empty;

            if (!File.Exists(XMLSetup))
                return dbUser;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "dbUser")
                {
                    dbUser = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return dbUser;
        }

        public static string ReadDBPasswort()
        {
            string dbPasswort = string.Empty;

            if (!File.Exists(XMLSetup))
                return dbPasswort;

            XmlReader xmlReader = XmlReader.Create(XMLSetup);
            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name.ToString() == "dbPasswort")
                {
                    dbPasswort = xmlReader.ReadString();
                    break;
                }
            }

            xmlReader.Close();

            return dbPasswort;
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

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\myMoney");
            if (key != null)
            {
                code = key.GetValue("code")?.ToString() ?? string.Empty;
                key.Close();
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
