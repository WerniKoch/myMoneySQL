using System;

namespace myMoney.DTO
{
    public class Uebersicht
    {
        public Guid KontoId { get; set; }
		public string Konto { get; set; }
        public string WaehrungId { get; set; }
        public decimal Saldo { get; set; }
        public decimal SaldoToday { get; set; }
    }
}
