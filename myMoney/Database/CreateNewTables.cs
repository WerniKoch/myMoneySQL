using Microsoft.Data.Sqlite;
using System;

namespace myMoney.Database
{
    public static class CreateNewTables
    {
        public static void CreateTables()
        {
            try
            {
                using var connection = new SqliteConnection("Data Source=myMoney.db");
                connection.Open();

                using var command = connection.CreateCommand();

                // Tabelle: buchungen
                command.CommandText =
                @"
            CREATE TABLE IF NOT EXISTS buchungen (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                datum DATE,
                typ INTEGER,
                konto INTEGER,
                waehrungsid TEXT,
                betrag NUMERIC,
                buchtext TEXT,
                kategorie INTEGER,
                transferid INTEGER
            );
            ";
                command.ExecuteNonQuery();


                // Tabelle: kategorien
                command.CommandText =
                @"
            CREATE TABLE IF NOT EXISTS kategorien (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                oberkategorie TEXT,
                unterkategorie TEXT,
                inaktiv INTEGER
            );
            ";
                command.ExecuteNonQuery();
                Console.WriteLine("Tabelle 'kategorien' erfolgreich erstellt.");

                // Tabelle: kontos
                command.CommandText =
                @"
            CREATE TABLE IF NOT EXISTS kontos (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                institution TEXT,
                nummer TEXT,
                bezeichnung TEXT,
                waehrungsid TEXT,
                saldo NUMERIC,
                position INTEGER
            );
            ";
                command.ExecuteNonQuery();

                // Tabelle: perioden
                command.CommandText =
                @"
            CREATE TABLE IF NOT EXISTS peribuchungen (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                periodizitaet INTEGER,
                startdatum DATE,
                lastdatum DATE,
                typ INTEGER,
                konto INTEGER,
                empfangskonto INTEGER,
                waehrungsid TEXT,
                betrag NUMERIC,
                buchtext TEXT,
                kategorie INTEGER
            );
            ";
                command.ExecuteNonQuery();

                // Tabelle: setup
                command.CommandText =
                @"
            CREATE TABLE IF NOT EXISTS setup (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                waehrungsid TEXT,
                anzahltage INTEGER,
                sprache TEXT,
                font TEXT,
                passwort TEXT,
                passwortaktiv INTEGER,
                vorschlagkonto INTEGER,
                startdiagramm INTEGER
            );
            INSERT INTO setup (waehrungsid, anzahltage, sprache, font, passwortaktiv, startdiagramm)
            SELECT 'CHF', 10, 'DE', 'Arial', 0, 0
            WHERE NOT EXISTS (
            SELECT 1 FROM setup
            );
            ";
                command.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Erstellen der Tabellen: {ex.Message}");
            }
        }
    }
}
