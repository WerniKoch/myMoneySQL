using Microsoft.Data.Sqlite;
using myMoney.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace myMoney
{
    public class DataAccessSQL
    {
        string SQLDatenbank { get; set; }

        public DataAccessSQL()
        {
#if RELEASE
            SQLDatenbank = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\myGuest\\Daten";
            if (!Directory.Exists(SQLDatenbank))
            {
                var ret = Directory.CreateDirectory(SQLDatenbank);
            }
#endif

#if DEBUG
            SQLDatenbank = AppDomain.CurrentDomain.BaseDirectory;
#endif
            SQLDatenbank += @"\myMoney.db";
        }

        public string GetDataFile()
        {
            return SQLDatenbank;
        }

        #region Buchungen
        // Jeden Text nur einmal 
        public async Task<List<string>> GetBuchungstexte()
        {
            return await Task.FromResult(new List<string>(GetBuchungen())
            {
            });
        }

        private List<string> GetBuchungen()
        {
            List<string> textList;
            textList = ReadBuchungen(0).Select(x => x.BuchText).Distinct().ToList();
            return textList.OrderBy(x => x).ToList();
        }

        public List<Buchung> ReadBuchungen(int konto)
        {
            string connectionString = "Data Source=" + SQLDatenbank;
            List<Buchung> buchungListe = new List<Buchung>();

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "select id, datum, typ, konto, waehrungsid, betrag, buchtext, kategorie, transferid from buchungen where $konto = 0 or $konto = konto order by datum desc";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("$konto", konto);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Buchung buchung = new Buchung();
                                buchung.Id = reader.GetInt32(0);
                                buchung.Datum = reader.GetDateTime(1);
                                buchung.Typ = reader.GetInt32(2);
                                buchung.Konto = reader.GetInt32(3);
                                buchung.WaehrungsId = reader.GetString(4);
                                buchung.Betrag = reader.GetDecimal(5);
                                buchung.BuchText = reader.GetString(6);
                                buchung.Kategorie = reader.GetInt32(7);
                                buchung.TransferId = reader.GetInt32(8);
                                buchung.BetragMitVorzeichen = buchung.Typ == (int)enTyp.Zahlung  ? -buchung.Betrag : buchung.Betrag;

                                buchungListe.Add(buchung);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }

                foreach (var buchung in buchungListe)
                {
                    if (buchung.Typ == (int)enTyp.Zahlung || buchung.Typ == (int)enTyp.TransferZahlung)
                    {
                        buchung.Zahlung = buchung.Betrag;
                    }
                    else
                    {
                        buchung.Gutschrift = buchung.Betrag;
                    }
                }

                return buchungListe;
            }
        }

        public int WriteBuchung(Buchung buchung)
        {
            int insertedId = 0;
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            INSERT INTO buchungen(datum, typ, konto, waehrungsid, betrag, buchtext, kategorie, transferid)
            VALUES ($datum, $typ, $konto, $waehrungsid, $betrag, $buchtext, $kategorie, $transferid); 
            SELECT last_insert_rowid()
            ";

                command.Parameters.AddWithValue("$datum", buchung.Datum);
                command.Parameters.AddWithValue("$typ", buchung.Typ);
                command.Parameters.AddWithValue("$konto", buchung.Konto);
                command.Parameters.AddWithValue("$waehrungsid", buchung.WaehrungsId);
                command.Parameters.AddWithValue("$betrag", buchung.Betrag);
                command.Parameters.AddWithValue("$buchtext", buchung.BuchText);
                command.Parameters.AddWithValue("$kategorie", buchung.Kategorie);
                command.Parameters.AddWithValue("$transferid", buchung.TransferId);

                var ret = (command.ExecuteScalar() ?? 0);
                insertedId = Convert.ToInt32(ret);
            }
            catch (Exception)
            {
                MessageBox.Show("Die Buchung kann nicht hinzugefügt werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            connection.Close();
            return insertedId;
        }

        public void UpdateBuchung(Buchung buchung)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
                UPDATE buchungen set datum = $datum,
                       typ = $typ, 
                       konto = $konto,
                       waehrungsid = $waehrungsid,
                       betrag = $betrag,
                       buchtext = $buchtext,
                       kategorie = $kategorie,
                       transferid = $transferid
                WHERE id = $id
            ";

                command.Parameters.AddWithValue("$datum", buchung.Datum);
                command.Parameters.AddWithValue("$typ", buchung.Typ);
                command.Parameters.AddWithValue("$konto", buchung.Konto);
                command.Parameters.AddWithValue("$waehrungsid", buchung.WaehrungsId);
                command.Parameters.AddWithValue("$betrag", buchung.Betrag);
                command.Parameters.AddWithValue("$buchtext", buchung.BuchText);
                command.Parameters.AddWithValue("$kategorie", buchung.Kategorie);
                command.Parameters.AddWithValue("$transferid", buchung.TransferId);
                command.Parameters.AddWithValue("$id", buchung.Id);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Die Buchung kann nicht hinzugefügt werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public void DeleteBuchung(Buchung buchung)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
                DELETE FROM buchungen where id = $id
                ";

                command.Parameters.AddWithValue("$id", buchung.Id);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Die Buchung kann nicht gelöscht werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion Buchungen


        #region Kategorie
        public List<string> ReadOberKategorien()
        {
            string connectionString = "Data Source=" + SQLDatenbank;
            List<string> oberKategorieListe = new List<string>();

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "select distinct oberkategorie from kategorien";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string oberKategorie = reader.GetString(0);
                                oberKategorieListe.Add(oberKategorie);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            return oberKategorieListe;
        }

        public List<Kategorie> ReadKategorien()
        {
            string connectionString = "Data Source=" + SQLDatenbank;
            List<Kategorie> kategorieListe = new List<Kategorie>();

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "select id, oberkategorie, unterkategorie, inaktiv from kategorien";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Kategorie kategorie = new Kategorie();
                                kategorie.Id = reader.GetInt32(0);
                                kategorie.OberKategorie = reader.GetString(1);
                                kategorie.UnterKategorie = reader.GetString(2);
                                kategorie.Inaktiv = reader.GetInt32(3);
                                kategorieListe.Add(kategorie);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            return kategorieListe;
        }

        public void AddKategorie(Kategorie kategorie)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            INSERT INTO kategorien(oberkategorie, unterkategorie, inaktiv)
            VALUES ($oberkategorie, $unterkategorie, $inaktiv)
            ";

                command.Parameters.AddWithValue("$oberkategorie", kategorie.OberKategorie);
                command.Parameters.AddWithValue("$unterkategorie", kategorie.UnterKategorie);
                command.Parameters.AddWithValue("$inaktiv", kategorie.Inaktiv);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Die Kategorie kann nicht hinzugefügt werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }

        }

        public void UpdateKategorie(Kategorie kategorie)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            UPDATE kategorien set oberkategorie = $oberkategorie,
            unterkategorie = $unterkategorie,
            inaktiv = $inaktiv
            where id = $id
            ";

                command.Parameters.AddWithValue("$id", kategorie.Id);
                command.Parameters.AddWithValue("$oberkategorie", kategorie.OberKategorie);
                command.Parameters.AddWithValue("$unterkategorie", kategorie.UnterKategorie);
                command.Parameters.AddWithValue("$inaktiv", kategorie.Inaktiv);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Die Kategorie kann nicht mutiert werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            connection.Close();
        }

        public void DeleteKategorie(Kategorie kategorie)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
                DELETE FROM kategorien where id = $id
                ";

                command.Parameters.AddWithValue("$id", kategorie.Id);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Die Kategorie kann nicht gelöscht werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion Kategorie

        #region Konto
        public List<Konto> ReadKontos()
        {
            string connectionString = "Data Source=" + SQLDatenbank;
            List<Konto> kontoListe = new List<Konto>();

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "select id, institution, nummer, bezeichnung, waehrungsid, saldo, position from kontos order by position";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Konto konto = new Konto();
                                konto.Id = reader.GetInt32(0);
                                konto.Institution = reader.GetString(1);
                                konto.Nummer = reader.GetString(2);
                                konto.Bezeichnung = reader.GetString(3);
                                konto.WaehrungId = reader.GetString(4);
                                konto.Saldo = reader.GetDecimal(5);
                                konto.Position = reader.GetInt32(6);

                                kontoListe.Add(konto);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            return kontoListe;
        }

        public void AddKonto(Konto konto)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            INSERT INTO kontos (institution, nummer, bezeichnung, waehrungsid, saldo, position)
            VALUES ($institution, $nummer, $bezeichung, $waehrungsid, $saldo, $position)
            ";

                command.Parameters.AddWithValue("$institution", konto.Institution);
                command.Parameters.AddWithValue("$nummer", konto.Nummer);
                command.Parameters.AddWithValue("$bezeichung", konto.Bezeichnung);
                command.Parameters.AddWithValue("$waehrungsid", konto.WaehrungId);
                command.Parameters.AddWithValue("$saldo", konto.Saldo);
                command.Parameters.AddWithValue("$position", konto.Position);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Das Konto kann nicht hinzugefügt werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public void UpdateKonto(Konto konto)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            UPDATE kontos set institution = $institution,
            nummer = $nummer,
            bezeichnung = $bezeichung,
            waehrungsid = $waehrungsid,
            saldo = $saldo,
            position = $position
            where id = $id
            ";

                command.Parameters.AddWithValue("$id", konto.Id);
                command.Parameters.AddWithValue("$institution", konto.Institution);
                command.Parameters.AddWithValue("$nummer", konto.Nummer);
                command.Parameters.AddWithValue("$bezeichung", konto.Bezeichnung);
                command.Parameters.AddWithValue("$waehrungsid", konto.WaehrungId);
                command.Parameters.AddWithValue("$saldo", konto.Saldo);
                command.Parameters.AddWithValue("$position", konto.Position);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Das Konto kann nicht mutiert werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public void DeleteKonto(Konto konto)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
                DELETE FROM kontos where id = $id
                ";

                command.Parameters.AddWithValue("$id", konto.Id);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Das Konto kann nicht gelöscht werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }

        }
        #endregion Konto

        #region Setup
        public int GetOldestJahr()
        {
            var buchList = ReadBuchungen(0);
            if (buchList == null || buchList.Count == 0)
                return 0;

            Buchung buchung = buchList[0];

            return buchung.Datum.Year;
        }

        public Setup ReadSetup()
        {
            string connectionString = "Data Source=" + SQLDatenbank;

            Setup setup = new Setup();

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "select id, ifnull(waehrungsid, ''), ifnull(anzahltage, 0), ifnull(sprache, 'DE'), " +
                        "ifnull(font, ''), ifnull(passwort, ''), ifnull(passwortaktiv, 0), ifnull(vorschlagkonto, 0), ifnull(startdiagramm,0) from setup";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                setup.Id = reader.GetInt32(0);
                                setup.WaehrungId = reader.GetString(1);
                                setup.AnzahlTage = reader.GetInt32(2);
                                setup.Sprache = reader.GetString(3);
                                setup.Font = reader.GetString(4);
                                setup.Passwort = reader.GetString(5);
                                setup.PasswortAktiv = reader.GetInt32(6) == 0 ? false : true;
                                setup.Vorschlagkonto = reader.GetInt32(7);
                                setup.StartDiagramm = reader.GetInt32(8);
                                return setup;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }

                return setup;
            }
        }

        public void SetSetup(Setup setup)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                UPDATE setup set waehrungsid = $waehrungsid,
                anzahltage = $anzahltage,
                sprache = $sprache,
                font = $font,
                passwort = $passwort,
                passwortaktiv = $passwortaktiv,
                vorschlagkonto = $vorschlagkonto,
                startdiagramm = $startdiagramm
                ";

                command.Parameters.AddWithValue("$waehrungsid", setup.WaehrungId);
                command.Parameters.AddWithValue("$anzahltage", setup.AnzahlTage);
                command.Parameters.AddWithValue("$sprache", setup.Sprache);
                command.Parameters.AddWithValue("$font", setup.Font);
                command.Parameters.AddWithValue("$passwort", setup.Passwort);
                command.Parameters.AddWithValue("$passwortaktiv", setup.PasswortAktiv);
                command.Parameters.AddWithValue("$vorschlagkonto", setup.Vorschlagkonto);
                command.Parameters.AddWithValue("$startdiagramm", setup.StartDiagramm);
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Das Setup kann nicht mutiert werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public List<string> GetDataFilesToBackup()
        {
            return new List<string>();
        }
        #endregion Setup

        #region PeriBuchungen
        public void WritePeriBuchungen(List<PeriBuchung> list)
        {
        }

        public List<PeriBuchung> ReadPeriBuchungen()
        {
            string connectionString = "Data Source=" + SQLDatenbank;
            List<PeriBuchung> peribuchungListe = new List<PeriBuchung>();

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "select id, periodizitaet, startdatum, lastdatum, typ, konto, empfangskonto, waehrungsid, betrag, buchtext, kategorie from peribuchungen";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PeriBuchung peribuchung = new PeriBuchung();
                                peribuchung.Id = reader.GetInt32(0);
                                peribuchung.Periodizitaet = (enPeriodizitaet)reader.GetInt32(1);
                                peribuchung.StartDatum = reader.GetDateTime(2);
                                peribuchung.LastDatum = reader.GetDateTime(3);
                                peribuchung.Typ = reader.GetInt32(4);
                                peribuchung.Konto = reader.GetInt32(5);
                                peribuchung.EmpfangsKonto = reader.GetInt32(6);
                                peribuchung.WaehrungsId = reader.GetString(7);
                                peribuchung.Betrag = reader.GetDecimal(8);
                                peribuchung.BuchText = reader.GetString(9);
                                peribuchung.Kategorie = reader.GetInt32(10);
                                peribuchungListe.Add(peribuchung);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            return peribuchungListe;
        }

        public void AddPeriBuchung(PeriBuchung peribuchung)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            INSERT INTO peribuchungen (periodizitaet, startdatum, lastdatum, typ, konto, empfangskonto, waehrungsid, betrag, buchtext, kategorie)
            VALUES ($periodizitaet, $startdatum, $lastdatum, $typ, $konto, $empfangskonto, $waehrungsid, $betrag, $buchtext, $kategorie)
            ";

                command.Parameters.AddWithValue("$periodizitaet", peribuchung.Periodizitaet);
                command.Parameters.AddWithValue("$startdatum", peribuchung.StartDatum);
                command.Parameters.AddWithValue("$lastdatum", peribuchung.LastDatum);
                command.Parameters.AddWithValue("$typ", peribuchung.Typ);
                command.Parameters.AddWithValue("$konto", peribuchung.Konto);
                command.Parameters.AddWithValue("$empfangskonto", peribuchung.EmpfangsKonto);
                command.Parameters.AddWithValue("$waehrungsid", peribuchung.WaehrungsId);
                command.Parameters.AddWithValue("$betrag", peribuchung.Betrag);
                command.Parameters.AddWithValue("$buchtext", peribuchung.BuchText);
                command.Parameters.AddWithValue("$kategorie", peribuchung.Kategorie);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Das Konto kann nicht hinzugefügt werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public void UpdatePeriBuchung(PeriBuchung peribuchung)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            UPDATE peribuchungen set periodizitaet = $periodizitaet,
				startdatum = $startdatum,
				lastdatum = $lastdatum,
				typ = $typ,
				konto = $konto,
				empfangskonto = $empfangskonto,
				waehrungsid = $waehrungsid,
				betrag = $betrag,
				buchtext = $buchtext,
				kategorie = $kategorie
            where id = $id
            ";

                command.Parameters.AddWithValue("$id", peribuchung.Id);
                command.Parameters.AddWithValue("$periodizitaet", peribuchung.Periodizitaet);
                command.Parameters.AddWithValue("$startdatum", peribuchung.StartDatum);
                command.Parameters.AddWithValue("$lastdatum", peribuchung.LastDatum);
                command.Parameters.AddWithValue("$typ", peribuchung.Typ);
                command.Parameters.AddWithValue("$konto", peribuchung.Konto);
                command.Parameters.AddWithValue("$empfangskonto", peribuchung.EmpfangsKonto);
                command.Parameters.AddWithValue("$waehrungsid", peribuchung.WaehrungsId);
                command.Parameters.AddWithValue("$betrag", peribuchung.Betrag);
                command.Parameters.AddWithValue("$buchtext", peribuchung.BuchText);
                command.Parameters.AddWithValue("$kategorie", peribuchung.Kategorie);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Das Konto kann nicht mutiert werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }

        }
        public void UpdatePeriBuchungDatum(int id, DateTime datum)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
            UPDATE peribuchungen set lastdatum = $lastdatum
            where id = $id
            ";

                command.Parameters.AddWithValue("$id", id);
                command.Parameters.AddWithValue("$lastdatum", datum);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Das Datum der Periodischen Buchung konnte nicht mutiert werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }

        }

        public void DeletePeriBuchung(PeriBuchung peribuchung)
        {
            using var connection = new SqliteConnection("Data Source=" + SQLDatenbank);

            try
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText =
                @"
                DELETE FROM peribuchungen where id = $id
                ";

                command.Parameters.AddWithValue("$id", peribuchung.Id);

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Die periodischen Buchung kann nicht gelöscht werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
        #endregion PeriBuchungen
    }
}
