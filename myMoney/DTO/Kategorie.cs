using System;
using System.Xml.Serialization;

namespace myMoney.DTO
{
    [XmlRoot("Kategorien")]
    [XmlType("Kategorie")]
    public class Kategorie
    {
        [XmlElement("Id")]
        public Guid Id { get; set; }

        [XmlElement("OberKategorie")]
        public string OberKategorie { get; set; } = string.Empty;

        [XmlElement("UnterKategorie")]
        public string UnterKategorie { get; set; } = string.Empty;

        [XmlElement("Inaktiv")]
        public bool Inaktiv { get; set; }

        [XmlIgnore]
        public string UnterOberKategorie
        {
            get { return OberKategorie + " - " + UnterKategorie; }
        }
    }
}
