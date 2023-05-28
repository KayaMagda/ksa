using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ksa.Models;
using Microsoft.Data.Sqlite;

namespace ksa
{
    /// <summary>
    /// In dieser Klasse sind alle Methoden die auf die Datenbank zugreifen, geht es ums einschreiben 
    /// sollen alle imports die gleiche Methode zum einschreiben nutzen
    /// </summary>
    public class DataAccess
    {
        private static string dbFileName = "skl.db";

        public static void CreateDatabase()
        {
            if (!File.Exists(dbFileName))
            {
                using SqliteConnection connection = new SqliteConnection($"Data Source=skl.db;Version=3;");
                connection.Open();

                string query = @"CREATE TABLE Kunde (
                                    nr bigint Primary Key
                                                    );

                                  CREATE TABLE Objekt (
                                    nr VARCHAR(15) Primary KEY,
                                    kunde_nr bigint,
                                    str VARCHAR(40),
                                    haus_nr integer,
                                    plz int,
                                    ort VARCHAR(40),
                                    FOREIGN KEY(kunde_nr) REFERENCES Kunde (nr)
                                                        );
                                  
                                CREATE TABLE Abfallart (
                                    abfallart VARCHAR(10)
                                                        );

                                CREATE TABLE ObjektAbfallArt (
                                    objekt_nr VARCHAR(15),
                                    abfallart_id int,
                                    volumen int,
                                    anzahl int,
                                    PRIMARY KEY (objekt_nr, abfallart_id),
                                    FOREIGN KEY (objekt_nr) REFERENCES Objekt (nr),
                                    FOREIGN KEY (abfallart_id) REFERENCES Aballart (abfallart)
                                                              );

  
                                ";
            }
        }

        private static SqliteConnection GetOpenConnection()
        {
            var connection = new SqliteConnection($"Data Source=skl.db;Version=3;");
            connection.Open();
            return connection;
        }

        public static void InsertData(Dictionary<long, List<Objekt>> kundenUndObjekte)
        {
            var query = new StringBuilder("INSERT INTO Kunde (nr) VALUES ");

            var connection = GetOpenConnection();
            var command = new SqliteCommand(null, connection);

            List<string> kundenNrToAdd = new List<string>();
            List<string> objekteToAdd = new List<string>();
            List<string> objektAbfallArtenToAdd = new List<string>();

            foreach (var kundenNr in kundenUndObjekte.Keys)
            {
                kundenNrToAdd.Add("(@" + kundenNr.ToString() + ")");
                command.Parameters.AddWithValue(kundenNr.ToString(), kundenNr);

                List<Objekt> kundenObjekte = kundenUndObjekte[kundenNr];
                foreach (var (objekt, objektIndex) in kundenObjekte.Select((objekt, i) => (objekt, i)))
                {
                    objekteToAdd.Add($@"(@objektNr{objektIndex}, 
                                         @kundenNr{objektIndex},
                                         @str{objektIndex},
                                         @hausNr{objektIndex},
                                         @plz{objektIndex},   
                                         @ort{objektIndex}
                                         )");

                    command.Parameters.AddWithValue("objektNr" + objektIndex, objekt.Nr);
                    command.Parameters.AddWithValue("kundenNr" + objektIndex, objekt.Kunde_Nr);
                    command.Parameters.AddWithValue("str" + objektIndex, objekt.Straße);
                    command.Parameters.AddWithValue("hausNr" + objektIndex, objekt.HausNr);
                    command.Parameters.AddWithValue("plz" + objektIndex, objekt.PLZ);
                    command.Parameters.AddWithValue("ort" + objektIndex, objekt.Ort);

                    List<ObjektAbfallArt> objektAbfallArten = objekt.ObjektAbfallArt;

                    foreach (var (abfallArt, artIndex) in objektAbfallArten.Select((art, i) => (art, i)))
                    {
                        objektAbfallArtenToAdd.Add($@"(
                                                       @objekt_nr{artIndex},
                                                       @abfallart{artIndex},
                                                       @anzahl{artIndex},
                                                       @volumen{artIndex}
                                                       )");

                        command.Parameters.AddWithValue("objektNr" + artIndex, abfallArt.ObjNr);
                        command.Parameters.AddWithValue("abfallart" + artIndex, abfallArt.Abfallart);
                        command.Parameters.AddWithValue("anzahl" + artIndex, abfallArt.Anzahl);
                        command.Parameters.AddWithValue("volumen" + artIndex, abfallArt.Volumen);
                    }
                }
            }

            query.AppendLine(string.Join(", ", kundenNrToAdd));
            query.AppendLine("INSERT INTO Objekt (nr, kunde_nr, str, haus_nr, plz, ort) VALUES ");
            query.AppendLine(string.Join(", ", objekteToAdd));
            query.AppendLine("INSERT INTO ObjektAbfallArt (objekt_nr, abfallart, anzahl, volumen) VALUES ");
        }

    }
}
