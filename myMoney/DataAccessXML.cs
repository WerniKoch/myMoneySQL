using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


public class DataAccessXML
{
#if DEBUG
    private static string XMLKonto = Environment.CurrentDirectory + "\\Kontos.xml";
    private static string XMLKategorien = Environment.CurrentDirectory + "\\Kategorien.xml";
    private static string XMLBuchungen = Environment.CurrentDirectory + "\\Buchungen.xml";
    private static string XMLPeriBuchungen = Environment.CurrentDirectory + "\\PeriBuchungen.xml";
    private static string XMLSetup = Environment.CurrentDirectory + "\\Setup.xml";
#else
        private static string XMLKonto = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\Kontos.xml";
        private static string XMLKategorien = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\Kategorien.xml";
        private static string XMLBuchungen = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\Buchungen.xml";
        private static string XMLPeriBuchungen = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\PeriBuchungen.xml";
        private static string XMLSetup = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myMoney\\Daten\\Setup.xml";
#endif


    public DataAccessXML()
    {

    }

    #region Kontos
    public List<Konto> ReadKontos()
    {
        if (!File.Exists(XMLKonto))
        {
            return new List<Konto>();
        }

        List<Konto> kontoList;
        kontoList = Deserialize<Konto>(XMLKonto).OrderBy(x => x.Position).ToList();
        return kontoList;
    }

    public void WriteKontos(List<Konto> list)
    {
        // GUID setzen bei neuen Kategorien
        foreach (var item in list)
        {
            if (item.Id == 0)
            {
            }

            // Position neu vergeben
            if (item.Position == 0)
            {
                item.Position = list.Max(x => x.Position) + 1;
            }
        }

        Serialize<Konto>(list, XMLKonto);
    }
    #endregion Kontos

    #region Kategorien
    public List<Kategorie> ReadKategorien(bool isAlle = true)
    {
        if (!File.Exists(XMLKategorien))
        {
            return new List<Kategorie>();
        }

        List<Kategorie> kategorienList;

        if (isAlle)
        {
            kategorienList = Deserialize<Kategorie>(XMLKategorien).OrderBy(x => x.OberKategorie).ThenBy(n => n.UnterKategorie).ToList();
        }
        else
        {
            kategorienList = Deserialize<Kategorie>(XMLKategorien).Where(x => x.Inaktiv == 0).OrderBy(x => x.OberKategorie).ThenBy(n => n.UnterKategorie).ToList();
        }

        return kategorienList;
    }

    public List<string> ReadOberKategorien()
    {
        string oldKategorie = string.Empty;
        var kategorienList = ReadKategorien();

        List<string> oberKategorienList = new List<string>();
        var list = kategorienList.GroupBy(x => x.OberKategorie).ToList();
        foreach (var item in list)
        {
            if (oldKategorie != item.Key.ToString())
            {
                oberKategorienList.Add(item.Key.ToString());
            }

            oldKategorie = item.Key.ToString();
        }

        return oberKategorienList;
    }

    public void WriteKategorien(List<Kategorie> list)
    {
        Serialize<Kategorie>(list, XMLKategorien);
    }
    #endregion Kategorien

    #region Buchungen
    public List<Buchung> ReadBuchungen(int konto)
    {
        if (!File.Exists(XMLBuchungen))
        {
            return new List<Buchung>();
        }

        List<Buchung> buchungsList = new List<Buchung>();
        /*
                if (konto == Guid.Empty)
                    buchungsList = Deserialize<Buchung>(XMLBuchungen).OrderBy(x => x.Datum).ToList();
                else
                    buchungsList = Deserialize<Buchung>(XMLBuchungen).Where(x => x.Konto == konto).OrderByDescending(x => x.Datum).ToList();

                foreach (var item in buchungsList)
                {
                    if (item.Typ == enTyp.Zahlung || item.Typ == enTyp.TransferZahlung)
                    {
                        item.BetragMitVorzeichen = item.Betrag * -1;
                        item.Zahlung = item.Betrag;
                    }
                    else
                    {
                        item.BetragMitVorzeichen = item.Betrag;
                        item.Gutschrift = item.Betrag;
                    }
                }
        */
        return buchungsList;
    }

    public void WriteBuchung(Buchung buchung)
    {
    }

    // Jeden Text nur einmal 
    public async Task<List<string>> GetBuchungstexte()
    {
        return await Task.FromResult(new List<string>(GetData())
        {
        });
    }

    private List<string> GetData()
    {
        if (!File.Exists(XMLBuchungen))
        {
            return new List<string>();
        }

        List<string> textList;
        textList = Deserialize<Buchung>(XMLBuchungen).Select(x => x.BuchText).Distinct().ToList();
        return textList.OrderBy(x => x).ToList();
    }

    public int GetOldestJahr()
    {
        var buchList = ReadBuchungen(0);
        if (buchList == null || buchList.Count == 0)
            return 0;

        Buchung buchung = buchList[0];

        return buchung.Datum.Year;
    }
    #endregion Buchungen

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

    #region PeriBuchungen
    public List<PeriBuchung> ReadPeriBuchungen()
    {
        if (!File.Exists(XMLPeriBuchungen))
        {
            return new List<PeriBuchung>();
        }

        List<PeriBuchung> periBuchungList;
        periBuchungList = Deserialize<PeriBuchung>(XMLPeriBuchungen).OrderBy(x => x.Id).ToList();

        return periBuchungList;
    }

    public void WritePeriBuchungen(List<PeriBuchung> list)
    {
        Serialize<PeriBuchung>(list, XMLPeriBuchungen);
    }
    #endregion PeriBuchungen

    #region Backup
    public List<string> GetDataFilesToBackup()
    {
        List<string> liste = new List<string>();
        liste.Add(XMLKonto);
        liste.Add(XMLKategorien);
        liste.Add(XMLBuchungen);
        liste.Add(XMLPeriBuchungen);
        liste.Add(XMLSetup);

        return liste;
    }
    #endregion Backup



}
