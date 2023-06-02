using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using ksa.Models;

namespace ksa
{
    /// <summary>
    /// In dieser Klasse sind alle Methoden die auf die Datenbank zugreifen, geht es ums Einschreiben 
    /// sollen alle imports die gleiche Methode zum Einschreiben nutzen
    /// </summary>
    public class DataAccess
    {
        private static string dbFileName = "skl.db";

        public static void CreateDatabase()
        {
            if (!File.Exists(dbFileName))
            {
                SQLiteConnection.CreateFile("skl.db");
                using SQLiteConnection connection = new SQLiteConnection("Data Source=skl.db;Version=3;");
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

                                INSERT INTO Abfallart (abfallart)
                                VALUES ('Bio'), ('Papier'), ('Restmüll');

                                CREATE TABLE ObjektAbfallArt (
                                    objekt_nr VARCHAR(15),
                                    abfallart string,
                                    volumen int,
                                    anzahl int,
                                    PRIMARY KEY (objekt_nr, abfallart),
                                    FOREIGN KEY (objekt_nr) REFERENCES Objekt (nr),
                                    FOREIGN KEY (abfallart) REFERENCES Abfallart (abfallart)
                                                              );

  
                                ";

                using SQLiteCommand command = new SQLiteCommand(query, connection);
                command.ExecuteNonQuery();
            }
        }

        private static SQLiteConnection GetOpenConnection()
        {
            var connection = new SQLiteConnection("Data Source=skl.db;Version=3;");
            connection.Open();
            return connection;
        }

        public static void InsertData(Dictionary<long, List<Objekt>> kundenUndObjekte)
        {
            var query = new StringBuilder("INSERT OR IGNORE INTO Kunde (nr) VALUES ");

            using SQLiteConnection connection = GetOpenConnection();
            using SQLiteCommand command = new SQLiteCommand(null, connection);

            List<string> kundenNrToAdd = new List<string>();
            List<string> objekteToAdd = new List<string>();
            List<string> objektAbfallArtenToAdd = new List<string>();

            foreach (var kundenNr in kundenUndObjekte.Keys)
            {
                kundenNrToAdd.Add("(@" + kundenNr.ToString() + ")");
                command.Parameters.AddWithValue("@" + kundenNr.ToString(), kundenNr);

                List<Objekt> kundenObjekte = kundenUndObjekte[kundenNr];
                foreach (var objekt in kundenObjekte)
                {
                    objekteToAdd.Add($@"(@objektNr{objekteToAdd.Count}, 
                                         @kundenNr{objekteToAdd.Count},
                                         @str{objekteToAdd.Count},
                                         @hausNr{objekteToAdd.Count},
                                         @plz{objekteToAdd.Count},   
                                         @ort{objekteToAdd.Count}
                                         )");

                    command.Parameters.AddWithValue("@objektNr" + (objekteToAdd.Count - 1), objekt.Nr);
                    command.Parameters.AddWithValue("@kundenNr" + (objekteToAdd.Count - 1), objekt.Kunde_Nr);
                    command.Parameters.AddWithValue("@str" + (objekteToAdd.Count - 1), objekt.Straße);
                    command.Parameters.AddWithValue("@hausNr" + (objekteToAdd.Count - 1), objekt.HausNr);
                    command.Parameters.AddWithValue("@plz" + (objekteToAdd.Count - 1), objekt.PLZ);
                    command.Parameters.AddWithValue("@ort" + (objekteToAdd.Count - 1), objekt.Ort);

                    List<ObjektAbfallArt> objektAbfallArten = objekt.ObjektAbfallArt;

                    foreach (var abfallArt in objektAbfallArten)
                    {
                        objektAbfallArtenToAdd.Add($@"(
                                                       @objektAbfallNr{objektAbfallArtenToAdd.Count},
                                                       @abfallart{objektAbfallArtenToAdd.Count},
                                                       @anzahl{objektAbfallArtenToAdd.Count},
                                                       @volumen{objektAbfallArtenToAdd.Count}
                                                       )");

                        command.Parameters.AddWithValue("@objektAbfallNr" + (objektAbfallArtenToAdd.Count -1), abfallArt.ObjNr);
                        command.Parameters.AddWithValue("@abfallart" + (objektAbfallArtenToAdd.Count -1), abfallArt.Abfallart);
                        command.Parameters.AddWithValue("@anzahl" + (objektAbfallArtenToAdd.Count -1), abfallArt.Anzahl);
                        command.Parameters.AddWithValue("@volumen" + (objektAbfallArtenToAdd.Count -1), abfallArt.Volumen);
                    }
                }
            }

            query.AppendLine(string.Join(", ", kundenNrToAdd));
            query.Append(" ;");

            query.AppendLine("INSERT OR IGNORE INTO Objekt (nr, kunde_nr, str, haus_nr, plz, ort) VALUES ");
            query.AppendLine(string.Join(", ", objekteToAdd));
            query.Append(" ;");

            query.AppendLine("INSERT OR IGNORE INTO ObjektAbfallArt (objekt_nr, abfallart, anzahl, volumen) VALUES ");
            query.AppendLine(string.Join(", ", objektAbfallArtenToAdd));
            query.Append(" ;");

            command.CommandText = query.ToString();

            command.ExecuteNonQuery();
        }

        private static List<StickerData> GetStickerData()
        {
            List<StickerData> data = new List<StickerData>();

            using SQLiteConnection connection = GetOpenConnection();
            using SQLiteCommand command = connection.CreateCommand();

            string query = @"SELECT o.nr, 
                                    o.str, 
                                    o.haus_nr,
                                    o.plz,
                                    o.ort,
                                    a.abfallart,
                                    a.volumen,
                                    a.anzahl
                            FROM
                                Objekt o
                            JOIN ObjektAbfallArt a ON o.nr = a.objekt_nr";

            command.CommandText = query;
            using var rd = command.ExecuteReader();

            while (rd.Read())
            {
                var d = new StickerData();

                d.ObjektNr = rd.GetString(0);
                d.Straße = rd.GetString(1);
                d.HausNr = rd.GetInt32(2);
                d.PLZ = rd.GetInt32(3);
                d.Ort = rd.GetString(4);
                d.Abfallsorte = rd.GetString(5);
                d.Volumen = rd.GetInt32(6);
                d.Anzahl = rd.GetInt32(7);

                data.Add(d);
            }

            return data;
        }

        public static List<GarbageSticker> GetGarbageSticker()
        {
            List<GarbageSticker> data = new List<GarbageSticker>();

            List<StickerData> sticker = GetStickerData();

            int tonnenNr = 6456785; // todo TonnenNr

            foreach (var st in sticker)
            {
                for (int i = 0; i < st.Anzahl; i++)
                {
                    var g = new GarbageSticker();

                    g.ObjektNr = st.ObjektNr;
                    g.Straße = st.Straße;
                    g.HausNr = st.HausNr;
                    g.PLZ = st.PLZ;
                    g.Ort = st.Ort;
                    g.Tonnennummer = tonnenNr++;
                    g.Abfallsorte = st.Abfallsorte;
                    g.Volumen = st.Volumen;

                    data.Add(g);
                }
            }

            return data;
        }
    }
}
