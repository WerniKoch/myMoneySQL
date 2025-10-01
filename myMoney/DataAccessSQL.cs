using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myMoney
{
    internal class DataAccessSQL : IDataAccess
    {
        public Task<List<string>> GetBuchungstexte()
        {
            return Task.FromResult(new List<string>());
        }

        public List<string> GetDataFilesToBackup()
        {
            return new List<string>();
        }

        public int GetOldestJahr()
        {
            return 0;
        }

        public List<Buchung> ReadBuchungen(Guid konto)
        {
            return new List<Buchung>();
        }

        public List<Kategorie> ReadKategorien(bool isAlle = true)
        {
            return new List<Kategorie>();
        }

        public List<Konto> ReadKontos()
        {
            return new List<Konto>();
        }

        public List<string> ReadOberKategorien()
        {
            return new List<string>();
        }

        public List<PeriBuchung> ReadPeriBuchungen()
        {
            return new List<PeriBuchung>();
        }

        public void WriteBuchungen(List<Buchung> list)
        {
        }

        public void WriteKategorien(List<Kategorie> list)
        {
        }

        public void WriteKontos(List<Konto> list)
        {
        }

        public void WritePeriBuchungen(List<PeriBuchung> list)
        {
        }
    }
}
