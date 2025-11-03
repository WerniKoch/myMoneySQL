using System;

namespace myMoney.DTO
{
    public class Uebersicht
    {
        public int KontoId { get; set; }
		public string Konto { get; set; } = string.Empty;
        public string WaehrungId { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public decimal SaldoToday { get; set; }
    }
}
