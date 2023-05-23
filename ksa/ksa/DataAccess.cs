using System.Collections.Generic;
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
        private static SqliteConnection GetOpenConnection()
        {
            var connection = new SqliteConnection($"Data Source=skl.db;Version=3;");
            connection.Open();
            return connection;
        }

        public void InsertData(Dictionary<long, Objekt> kundenUndObjekte)
        {
            var query = new StringBuilder("INSERT INTO Kunde (nr) VALUES ");

            var connection = GetOpenConnection();
            var command = new SqliteCommand(null, connection);

            List<string> kundenNrToAdd = new List<string>();

            foreach (var (kundenNr, index) in kundenUndObjekte.Keys.Select((key, i) => (key, i)))
            {
                kundenNrToAdd.Add("(@" + kundenNr.ToString() + index.ToString() + ")");
                command.Parameters.AddWithValue(kundenNr.ToString() + index.ToString(), kundenNr);
            }

            query.AppendLine(string.Join(", ", kundenNrToAdd));
            query.AppendLine("INSERT INTO Objekt (nr, kunde_nr, str, haus_nr, plz, ort) VALUES ");
        }

    }
}
