using System;

namespace myMoney.DTO
{
    public class Kategorie
    {
        public int Id { get; set; }
        public string OberKategorie { get; set; } = string.Empty;
        public string UnterKategorie { get; set; } = string.Empty;
        public int Inaktiv { get; set; }
        public string UnterOberKategorie
        {
            get { return OberKategorie + " - " + UnterKategorie; }
        }
    }
}
