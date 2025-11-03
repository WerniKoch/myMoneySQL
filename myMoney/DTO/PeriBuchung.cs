using System;

namespace myMoney.DTO
{
    public class PeriBuchung
    {
        public int Id { get; set; }
        public enPeriodizitaet Periodizitaet { get; set; }
        public DateTime StartDatum { get; set; }
        public DateTime LastDatum { get; set; }
        public int Typ { get; set; }
        public int Konto { get; set; }
        public int EmpfangsKonto { get; set; }
        public string WaehrungsId { get; set; } = string.Empty;
        public decimal Betrag { get; set; }
        public string BuchText { get; set; } = string.Empty;
        public int Kategorie { get; set; }
    }
}

