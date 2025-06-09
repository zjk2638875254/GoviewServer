using System.Data.SQLite;

namespace GoViewServer
{
    public class SearchData
    {
        public static string get_data(string dbPath, string connectionString)
        {
            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = "SELECT value FROM dem WHERE name = 'test'";
                using var command = new SQLiteCommand(query, connection);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return reader["value"].ToString();
                }
                return "not found";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string find_data(SQLiteConnection connection, string project_name, string name)
        {
            try
            {
                string query = "SELECT value FROM " + project_name + " WHERE name = '" + name + "'";
                using var command = new SQLiteCommand(query, connection);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return reader["value"].ToString();
                }
                return "not found";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }




    }
}
