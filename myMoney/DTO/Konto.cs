namespace myMoney.DTO
{
    public class Konto
    {
        public int Id { get; set; } = 0;
        public string Institution { get; set; } = string.Empty;
        public string Nummer { get; set; } = string.Empty;
        public string Bezeichnung { get; set; } = string.Empty;
        public string WaehrungId { get; set; } = string.Empty;
        public decimal Saldo { get; set; } = 0.00m;
        public int Position { get; set; } = 0;
    }
}
