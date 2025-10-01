using System;
using System.Xml.Serialization;

namespace myMoney.DTO
{
    [XmlRoot("Konten")]
    [XmlType("Konto")]
    public class Konto
    {
        [XmlElement("Id")]
        public Guid Id { get; set; }

        [XmlElement("Institution")]
        public string Institution { get; set; } = string.Empty;

        [XmlElement("Nummer")]
        public string Nummer { get; set; } = string.Empty;

        [XmlElement("Bezeichnung")]
        public string Bezeichnung { get; set; } = string.Empty;

        [XmlElement("WaehrungId")]
        public string WaehrungId { get; set; } = string.Empty;

        [XmlElement("Saldo")]
        public decimal Saldo { get; set; }

        [XmlElement("Position")]
        public int Position { get; set; }
    }
}
