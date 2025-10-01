using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myMoney
{
    internal interface IDataAccess
    {
        List<Konto> ReadKontos();
        void WriteKontos(List<Konto> list);

        List<Kategorie> ReadKategorien(bool isAlle = true);
        List<string> ReadOberKategorien();
        void WriteKategorien(List<Kategorie> list);

        List<Buchung> ReadBuchungen(Guid konto);
        void WriteBuchungen(List<Buchung> list);
        Task<List<string>> GetBuchungstexte();
        int GetOldestJahr();

        List<PeriBuchung> ReadPeriBuchungen();
        void WritePeriBuchungen(List<PeriBuchung> list);

        List<string> GetDataFilesToBackup();

    }
}
