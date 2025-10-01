using System;

namespace myMoney.DTO
{
    public class Periodisch
    {
        public Guid Id { get; set; }
        public bool IsSelektiert { get; set; }
        public DateTime Datum { get; set; }
        public Guid KontoId { get; set; }
        public string Konto { get; set; }
        public string BuchText { get; set; }
        public string Kategorie { get; set; }
        public string WaehrungsId { get; set; }
        public decimal Betrag { get; set; }
        public enTyp Typ { get; set; }
        public string? TypText { get; set; }
        public Guid KategorieId { get; set; }
        public Guid GegenKontoId { get; set; }
    }
}
