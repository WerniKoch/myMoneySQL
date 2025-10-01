using System;
using System.Xml.Serialization;

namespace myMoney.DTO
{
    [XmlRoot("PeriBuchungen")]
    [XmlType("PeriBuchung")]
    public class PeriBuchung
    {
        [XmlElement("Id")]
        public Guid Id { get; set; }

        [XmlElement("Periodizitaet")]
        public enPeriodizitaet Periodizitaet { get; set; }

        [XmlElement("StartDatum")]
        public DateTime StartDatum { get; set; }

        [XmlElement("LastDatum")]
        public DateTime LastDatum { get; set; }

        [XmlElement("Typ")]
        public enTyp Typ { get; set; }

        [XmlElement("Konto")]
        public Guid Konto { get; set; }

        [XmlElement("EmpfangsKonto")]
        public Guid EmpfangsKonto { get; set; }

        [XmlElement("WaehrungsId")]
        public string WaehrungsId { get; set; } = string.Empty;

        [XmlElement("Betrag")]
        public decimal Betrag { get; set; }

        [XmlElement("BuchText")]
        public string BuchText { get; set; } = string.Empty;

        [XmlElement("Kategorie")]
        public Guid Kategorie { get; set; }
    }
}

