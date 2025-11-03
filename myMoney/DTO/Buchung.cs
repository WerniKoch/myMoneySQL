using System;

namespace myMoney.DTO
{
    public class Buchung
    {
        public int Id { get; set; }
        public DateTime Datum { get; set; }
        public int Typ { get; set; }
        public int Konto { get; set; }
        public string WaehrungsId { get; set; } = string.Empty;
        public decimal Betrag { get; set; }
        public string BuchText { get; set; } = string.Empty;
        public int Kategorie { get; set; }
        public int TransferId { get; set; }
        public decimal Saldo { get; set; }

        public string KategorieText { get; set; } = string.Empty;
        public decimal Gutschrift { get; set; }
        public decimal Zahlung { get; set; }
        public decimal BetragMitVorzeichen { get; set; }
    }
}
