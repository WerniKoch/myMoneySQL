using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myMoney.DTO
{
    public class Setup
    {
        public int Id { get; set; } = 0;
        public string WaehrungId { get; set; } = string.Empty;
        public int AnzahlTage { get; set; } = 0;
        public string Sprache { get; set; } = "DE";
        public string Font { get; set; } = "Arial";
        public string Passwort { get; set; } = "";
        public bool PasswortAktiv { get; set; } = false;
        public int Vorschlagkonto { get; set; } = 0;
        public int StartDiagramm { get; set; } = 0;
    }
}
