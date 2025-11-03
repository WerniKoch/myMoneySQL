using System;

namespace myMoney.DTO
{
    public class Periodisch
    {
        public int Id { get; set; }
        public bool IsSelektiert { get; set; }
        public DateTime Datum { get; set; }
        public int KontoId { get; set; }
        public string Konto { get; set; } = string.Empty;
        public string BuchText { get; set; } = string.Empty;
        public string Kategorie { get; set; } = string.Empty;
        public string WaehrungsId { get; set; } = string.Empty;
        public decimal Betrag { get; set; }
        public int Typ { get; set; }
        public int KategorieId { get; set; }
        public int GegenKontoId { get; set; }
    }
}
