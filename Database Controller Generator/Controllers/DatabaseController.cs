using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace Database_Controller_Generator.Controllers
{
    public class DatabaseController : Controller
    {
        private SqlConnection connect { get; set; }
        public DatabaseController() {
            connect = new SqlConnection();
            connect.Open();
        }

        public DatabaseController(string Hostname, string Username, string Password) {
            connect = new SqlConnection("Server=tcp:"+Hostname+",1433;Persist Security Info=False;User ID="+Username+";Password="+Password+";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            connect.Open();
        }

        public DatabaseController(string Hostname, string Database, string Username, string Password) {
            connect = new SqlConnection("Server=tcp:"+Hostname+",1433;Initial Catalog="+Database+";Persist Security Info=False;User ID="+Username+";Password="+Password+";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            connect.Open();
        }

        public DatabaseController(Models.DatabaseRequest dbRequest) {
            if (dbRequest.Database != "") {
                if (dbRequest.Username == null && dbRequest.Password == null)
                {
                    connect = new SqlConnection("Server=tcp:" + dbRequest.Hostname + ",1433;Initial Catalog=" + dbRequest.Database + ";Persist Security Info=False;Integrated Security=true;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
                    connect.Open();
                }
                else
                {
                    connect = new SqlConnection("Server=tcp:" + dbRequest.Hostname + ",1433;Initial Catalog=" + dbRequest.Database + ";Persist Security Info=False;User ID=" + dbRequest.Username + ";Password=" + dbRequest.Password + ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
                    connect.Open();
                }
            }
            else {
                if (dbRequest.Username == null && dbRequest.Password == null)
                {
                    connect = new SqlConnection("Server=tcp:" + dbRequest.Hostname + ",1433;Persist Security Info=False;Integrated Security=true;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
                    connect.Open();
                }
                else
                {
                    connect = new SqlConnection("Server=tcp:" + dbRequest.Hostname + ",1433;Persist Security Info=False;User ID=" + dbRequest.Username + ";Password=" + dbRequest.Password + ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
                    connect.Open();
                }
                
            }
        }

        // Collect Database Object Metadata
        public List<Models.DatabaseSchema> GetDatabaseSchemas()
        {
            List<Models.DatabaseSchema> Schemas = new List<Models.DatabaseSchema>();
            string query = "select schema_id SchemaID, name as SchemaName from sys.schemas where name not like 'db_%'";
            SqlCommand command = new SqlCommand(query, connect);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Models.DatabaseSchema schema = new Models.DatabaseSchema();
                schema.SchemaID = reader.GetInt32(0);
                schema.SchemaName = reader.GetString(1);
                Schemas.Add(schema);
            }
            reader.Close();
            return Schemas;
        }

        public List<Models.DatabaseTable> GetDatabaseTables(int SchemaID) {
            List<Models.DatabaseTable> Tables = new List<Models.DatabaseTable>();
            string query = $"select schema_id SchemaID, object_id TableID, name as TableName from sys.tables where schema_id = {SchemaID} and Name not like 'AspNet%' and Name not like 'sys%' order by Name";
            SqlCommand command = new SqlCommand(query, connect);
            SqlDataReader reader = command.ExecuteReader();
            while(reader.Read()) {
                Models.DatabaseTable table = new Models.DatabaseTable();
                table.SchemaID = reader.GetInt32(0);
                table.TableID = reader.GetInt32(1);
                table.TableName = reader.GetString(2);
                Tables.Add(table);
            }
            reader.Close();
            return Tables;
        }

        public List<Models.DatabaseColumn> GetDatabaseColumns(int TableID) {
            List<Models.DatabaseColumn> Columns = new List<Models.DatabaseColumn>();
            string query = $"select object_id TableID, cols.name as ColumnName, types.name as TypeName, cols.is_nullable as Nullable, cols.max_length as ByteLength, cols.precision as Precision, cols.scale as Scale from  sys.all_columns cols  INNER JOIN  sys.types types ON cols.system_type_id = types.user_type_id WHERE object_id = {TableID}";
            SqlCommand command = new SqlCommand(query, connect);
            SqlDataReader reader = command.ExecuteReader();
            while(reader.Read()) {
                Models.DatabaseColumn column = new Models.DatabaseColumn();
                column.TableID = reader.GetInt32(0);
                column.ColumnName = reader.GetString(1);
                column.TypeName = reader.GetString(2);
                column.Nullable = reader.GetBoolean(3);
                column.ByteLength = reader.GetInt32(4);
                column.Precision = reader.GetInt32(5);
                column.Scale = reader.GetInt32(6);
                Columns.Add(column);
            }
            reader.Close();
            return Columns;
        }

        // Get standard database value

        public string GetSqlDatabaseString(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetString(reader.GetOrdinal(columnName));
            }
            return "";
        }

        public int GetSqlDatabaseInt(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetInt32(reader.GetOrdinal(columnName));
            }
            return 0;
        }

        public double GetSqlDatabaseDouble(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetDouble(reader.GetOrdinal(columnName));
            }
            return 0.0;
        }

        public DateTime GetSqlDatabaseDateTime(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetDateTime(reader.GetOrdinal(columnName));
            }
            return new DateTime(DateTime.MinValue.Ticks);
        }

        public bool GetSqlDatabaseBoolean(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetBoolean(reader.GetOrdinal(columnName));
            }
            return new Boolean();
        }

        public Guid GetSqlDatabaseGuid(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return Guid.Parse(reader.GetString(reader.GetOrdinal(columnName)));
            }
            return Guid.Parse("00000000-0000-0000-0000-000000000000");
        }

        // Get nullable database value

        public string GetSqlDatabaseNullableString(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetString(reader.GetOrdinal(columnName));
            }
            return null;
        }

        public int? GetSqlDatabaseNullableInt(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetInt32(reader.GetOrdinal(columnName));
            }
            return null;
        }

        public double? GetSqlDatabaseNullableDouble(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetDouble(reader.GetOrdinal(columnName));
            }
            return null;
        }

        public DateTime? GetSqlDatabaseNullableDateTime(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetDateTime(reader.GetOrdinal(columnName));
            }
            return null;
        }

        public bool? GetSqlDatabaseNullableBoolean(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetBoolean(reader.GetOrdinal(columnName));
            }
            return null;
        }

        public Guid? GetSqlDatabaseNullableGuid(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return Guid.Parse(reader.GetString(reader.GetOrdinal(columnName)));
            }
            return null;
        }

        public Version GetSqlDatabaseVersion(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                try
                {
                    Version value;
                    Version.TryParse(reader.GetString(reader.GetOrdinal(columnName)), out value);
                    return value;
                }
                catch
                {
                    return Version.Parse("0.0");
                }
            }
            else
            {
                return Version.Parse("0.0");
            }
        }
    }
}