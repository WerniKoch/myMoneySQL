using System;
using System.Xml.Serialization;

namespace myMoney.DTO
{
    [XmlRoot("Buchungen")]
    [XmlType("Buchung")]
    public class Buchung
    {
        [XmlElement("Id")]
        public Guid Id { get; set; }

        [XmlElement("Datum")]
        public DateTime Datum { get; set; }

        [XmlElement("Typ")]
        public enTyp Typ { get; set; }

        [XmlElement("Konto")]
        public Guid Konto { get; set; }

        [XmlElement("WaehrungsId")]
        public string WaehrungsId { get; set; } = string.Empty;

        [XmlElement("Betrag")]
        public decimal Betrag { get; set; }

        [XmlElement("BuchText")]
        public string BuchText { get; set; } = string.Empty;

        [XmlElement("Kategorie")]
        public Guid Kategorie { get; set; }

        [XmlElement("TransferId")]
        public Guid TransferId { get; set; }

        [XmlIgnore]
        public decimal Saldo { get; set; }

        [XmlIgnore]
        public string KategorieText { get; set; }
        [XmlIgnore]
        public decimal Gutschrift { get; set; }
        [XmlIgnore]
        public decimal Zahlung { get; set; }
        [XmlIgnore]
        public decimal BetragMitVorzeichen { get; set; }
    }
}
