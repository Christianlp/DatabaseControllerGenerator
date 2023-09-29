using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Database_Controller_Generator.Models;

namespace Database_Controller_Generator.Controllers
{
    public class ControllerCreator {
        public string CreateDatabaseController(DatabaseObjects DBObjects) {
            StringBuilder fileContents = new StringBuilder();
            fileContents.Append(WriteGenericControllerHeader());
            foreach(DatabaseSchema schema in DBObjects.Schemas)
            {
                foreach (DatabaseTable table in schema.Tables)
                {
                    fileContents.Append(WriteGetTableStatements(table));
                    fileContents.AppendLine("");
                    fileContents.Append(WriteInsertTableStatements(table));
                    fileContents.AppendLine("");
                    fileContents.Append(WriteUpdateTableStatements(table));
                    fileContents.AppendLine("");
                }
            }
            fileContents.Append(WriteGenericDBFunctions());
            return fileContents.ToString();
        }

        public string getNullableString(DatabaseColumn column) {
            if(column.Nullable) {
                return "Nullable";
            }
            return "";
        }

        public string getCSharpDataType(DatabaseColumn column) {
            switch (column.TypeName) {
                case "nchar":
                    if(column.ByteLength == 72)
                    {
                        return "Guid";
                    }
                    return "String";
                case "nvarchar":
                    return "String";
                case "sysname":
                    return "String";
                case "int":
                    return "Int";
                case "float":
                    return "Double";
                case "datetime":
                    return "DateTime";
                case "bit":
                    return "Boolean";
                default:
                    return "String";
            }
        }

        public string getDBSqlDbType(DatabaseColumn column) {
            switch (column.TypeName) {
                case "nchar":
                    return "NChar";
                case "nvarchar":
                    return "NVarChar";
                case "sysname":
                    return "NVarChar";
                case "int":
                    return "Int";
                case "float":
                    return "Float";
                case "datetime":
                    return "DateTime";
                case "bit":
                    return "Bit";
                default:
                    return "NVarChar";
            }
        }

        public string getTableNameNonPlural(DatabaseTable table) {
            string name = table.TableName;
            if (name[table.TableName.Length - 1] == 's') {
                return table.TableName.Substring(0, table.TableName.Length - 1);
            }
            else {
                return table.TableName;
            }
        }
    
        public StringBuilder WriteGenericControllerHeader() {
            StringBuilder fileContents = new StringBuilder();
            fileContents.AppendLine("using System;");
            fileContents.AppendLine("using System.Collections.Generic;");
            fileContents.AppendLine("using System.Linq;");
            fileContents.AppendLine("using System.Web;");
            fileContents.AppendLine("using System.Web.Mvc;");
            fileContents.AppendLine("using System.Data.SqlClient;");
            fileContents.AppendLine("using System.Configuration;");
            fileContents.AppendLine("");
            fileContents.AppendLine("namespace <PROJECT_NAME>.Controllers");
            fileContents.AppendLine("{");
            fileContents.AppendLine("\tpublic class DatabaseController : Controller");
            fileContents.AppendLine("\t{");
            return fileContents;
        }

        public StringBuilder WriteGetTableStatements(DatabaseTable table) {
            StringBuilder fileContents = new StringBuilder();
            fileContents.AppendLine("\t\tpublic List<Models." + getTableNameNonPlural(table) + "> Get" + getTableNameNonPlural(table) + "s()");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\tSqlConnection connect = GetSqlDataConnection();");
            fileContents.AppendLine("\t\t\tSqlCommand command = new SqlCommand(\"SELECT * FROM " + table.TableName + "\", connect);");
            fileContents.AppendLine("\t\t\tSqlDataReader reader = command.ExecuteReader();");
            fileContents.AppendLine("");
            fileContents.AppendLine("\t\t\tList<Models." + getTableNameNonPlural(table) + "> " + getTableNameNonPlural(table) + "s = new List<Models." + getTableNameNonPlural(table) + ">();");
            fileContents.AppendLine("\t\t\twhile (reader.Read())");
            fileContents.AppendLine("\t\t\t{");
            fileContents.AppendLine("\t\t\t\tModels." + getTableNameNonPlural(table) + " tmp = new Models." + getTableNameNonPlural(table) + "();");
            foreach(DatabaseColumn column in table.Columns) {
                fileContents.Append(WriteGetColumnStatement(column));
            }
            fileContents.AppendLine("\t\t\t\t" + getTableNameNonPlural(table) + "s.Add(tmp);");
            fileContents.AppendLine("\t\t\t}");
            fileContents.AppendLine("");
            fileContents.AppendLine("\t\t\tconnect.Close();");
            fileContents.AppendLine("\t\t\treturn " + getTableNameNonPlural(table) + "s;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("");
            return fileContents;
        }

        public StringBuilder WriteGetColumnStatement(DatabaseColumn column) {
            StringBuilder fileContents = new StringBuilder();
            fileContents.AppendLine("\t\t\t\ttmp." + column.ColumnName + " = GetSqlDatabase" + getNullableString(column) + getCSharpDataType(column) + "(reader, \"" + column.ColumnName + "\");");
            return fileContents;
        }

        public StringBuilder WriteInsertTableStatements(DatabaseTable table) {
            StringBuilder fileContents = new StringBuilder();
            fileContents.AppendLine("\t\tpublic int Insert" + getTableNameNonPlural(table) + "(Models." + getTableNameNonPlural(table) + " new" + table.TableName + ")");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\tSqlConnection connect = GetSqlDataConnection();");
            fileContents.Append(WriteInsertTableQuery(table));
            fileContents.AppendLine("");
            fileContents.AppendLine("");
            foreach(DatabaseColumn column in table.Columns) {
                fileContents.Append(WriteInsertColumnNewParameterStatement(table.TableName, column));
            }
            fileContents.AppendLine("");
            foreach(DatabaseColumn column in table.Columns) {
                fileContents.Append(WriteInsertColumnSetParameterStatement(table.TableName, column));
            }
            fileContents.AppendLine("");
            fileContents.AppendLine("\t\t\tint returnValue = " + table.TableName + "Insert.ExecuteNonQuery();");
            fileContents.AppendLine("");
            fileContents.AppendLine("\t\t\tconnect.Close();");
            fileContents.AppendLine("\t\t\treturn returnValue;");
            fileContents.AppendLine("\t\t}");
            return fileContents;
        }

        public StringBuilder WriteInsertTableQuery(DatabaseTable table) {
            StringBuilder fileContents = new StringBuilder();
            fileContents.AppendLine("\t\t\tSqlCommand " + table.TableName + "Insert = new SqlCommand(\"INSERT INTO " + table.TableName + "\" +");
            fileContents.Append("\t\t\t\"(");
            fileContents.Append(string.Join(", ", table.Columns.Select(p => p.ColumnName)));
            fileContents.Append(") \" +");
            fileContents.AppendLine("");
            fileContents.Append("\t\t\t\"VALUES(");
            fileContents.Append(string.Join(", ", table.Columns.Select(p => "@" + p.ColumnName)));
            fileContents.Append(")\", connect);");
            return fileContents;
        }

        public StringBuilder WriteUpdateTableStatements(DatabaseTable table) {
            StringBuilder fileContents = new StringBuilder();
            fileContents.AppendLine("\t\tpublic int Update" + getTableNameNonPlural(table) + "(Models." + getTableNameNonPlural(table) + " new" + table.TableName + ")");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\tSqlConnection connect = GetSqlDataConnection();");
            fileContents.Append(WriteUpdateTableQuery(table));
            fileContents.AppendLine("");
            fileContents.AppendLine("");
            foreach(DatabaseColumn column in table.Columns) {
                fileContents.Append(WriteUpdateColumnNewParameterStatement(table.TableName, column));
            }
            fileContents.AppendLine("");
            foreach(DatabaseColumn column in table.Columns) {
                fileContents.Append(WriteUpdateColumnSetParameterStatement(table.TableName, column));
            }
            fileContents.AppendLine("");
            fileContents.AppendLine("\t\t\tint returnValue = " + table.TableName + "Update.ExecuteNonQuery();");
            fileContents.AppendLine("");
            fileContents.AppendLine("\t\t\tconnect.Close();");
            fileContents.AppendLine("\t\t\treturn returnValue;");
            fileContents.AppendLine("\t\t}");
            return fileContents;
        }

        public StringBuilder WriteUpdateTableQuery(DatabaseTable table) {
            StringBuilder fileContents = new StringBuilder();
            fileContents.AppendLine("\t\t\tSqlCommand " + table.TableName + "Update = new SqlCommand(\"UPDATE " + table.TableName + " SET \" +");
            fileContents.Append("\t\t\t\"");
            fileContents.Append(string.Join(", ", table.Columns.Where(p => p.ColumnName != "guid" && p.ColumnName != "CreateDT").Select(q => q.ColumnName + " = @" + q.ColumnName)));
            fileContents.AppendLine(" \" +");
            fileContents.AppendLine("\t\t\t\"WHERE guid = @guid\", connect);");
            return fileContents;
        }

        public StringBuilder WriteInsertColumnNewParameterStatement(string TableName, DatabaseColumn column) {
            StringBuilder fileContents = new StringBuilder();
            // Write Insert Statements New Parameter Definitions
            fileContents.AppendLine("\t\t\t" + TableName + "Insert.Parameters.Add(\"@" + column.ColumnName + "\", System.Data.SqlDbType." + getDBSqlDbType(column) + ");");
            return fileContents;
        }

        public StringBuilder WriteInsertColumnSetParameterStatement(string TableName, DatabaseColumn column) {
            StringBuilder fileContents = new StringBuilder();
            // Write Insert Statements Setting Parameter Values
            if (column.Nullable) {
                // Write Insert Statements for a column that could be null
                fileContents.AppendLine("\t\t\tif (new" + TableName + "." + column.ColumnName + " == null)");
                fileContents.AppendLine("\t\t\t{");
                fileContents.AppendLine("\t\t\t\t" + TableName + "Insert.Parameters[\"@" + column.ColumnName + "\"].Value = DBNull.Value;");
                fileContents.AppendLine("\t\t\t}");
                fileContents.AppendLine("\t\t\telse");
                fileContents.AppendLine("\t\t\t{");
                if (column.TypeName == "nchar") {// field is a guid type
                    fileContents.AppendLine("\t\t\t\t" + TableName + "Insert.Parameters[\"@" + column.ColumnName + "\"].Value = Convert.ToString(new" + TableName + "." + column.ColumnName + ");");
                }
                else {
                    fileContents.AppendLine("\t\t\t\t" + TableName + "Insert.Parameters[\"@" + column.ColumnName + "\"].Value = new" + TableName + "." + column.ColumnName + ";");
                }
                fileContents.AppendLine("\t\t\t}");
            }
            else {
                if (column.TypeName == "nchar") {// field is a guid type
                    fileContents.AppendLine("\t\t\t" + TableName + "Insert.Parameters[\"@" + column.ColumnName + "\"].Value = Convert.ToString(new" + TableName + "." + column.ColumnName + ");");
                }
                else {
                    fileContents.AppendLine("\t\t\t" + TableName + "Insert.Parameters[\"@" + column.ColumnName + "\"].Value = new" + TableName + "." + column.ColumnName + ";");
                }
            }
            return fileContents;
        }

        public StringBuilder WriteUpdateColumnNewParameterStatement(string TableName, DatabaseColumn column) {
            if (column.ColumnName == "CreateDT") {
                return new StringBuilder();
            }
            StringBuilder fileContents = new StringBuilder();
            // Write Insert Statements New Parameter Definitions
            fileContents.AppendLine("\t\t\t" + TableName + "Update.Parameters.Add(\"@" + column.ColumnName + "\", System.Data.SqlDbType." + getDBSqlDbType(column) + ");");
            return fileContents;
        }

        public StringBuilder WriteUpdateColumnSetParameterStatement(string TableName, DatabaseColumn column) {
            if (column.ColumnName == "CreateDT") {
                return new StringBuilder();
            }
            StringBuilder fileContents = new StringBuilder();
            // Write Insert Statements Setting Parameter Values
            if (column.Nullable) {
                // Write Insert Statements for a column that could be null
                fileContents.AppendLine("\t\t\tif (new" + TableName + "." + column.ColumnName + " == null)");
                fileContents.AppendLine("\t\t\t{");
                fileContents.AppendLine("\t\t\t\t" + TableName + "Update.Parameters[\"@" + column.ColumnName + "\"].Value = DBNull.Value;");
                fileContents.AppendLine("\t\t\t}");
                fileContents.AppendLine("\t\t\telse");
                fileContents.AppendLine("\t\t\t{");
                if (column.TypeName == "nchar") {// field is a guid type
                    fileContents.AppendLine("\t\t\t\t" + TableName + "Update.Parameters[\"@" + column.ColumnName + "\"].Value = Convert.ToString(new" + TableName + "." + column.ColumnName + ");");
                }
                else {
                    fileContents.AppendLine("\t\t\t\t" + TableName + "Update.Parameters[\"@" + column.ColumnName + "\"].Value = new" + TableName + "." + column.ColumnName + ";");
                }
                fileContents.AppendLine("\t\t\t}");
            }
            else {
                
                if (column.TypeName == "nchar") {// field is a guid type
                    fileContents.AppendLine("\t\t\t" + TableName + "Update.Parameters[\"@" + column.ColumnName + "\"].Value = Convert.ToString(new" + TableName + "." + column.ColumnName + ");");
                }
                else {
                    fileContents.AppendLine("\t\t\t" + TableName + "Update.Parameters[\"@" + column.ColumnName + "\"].Value = new" + TableName + "." + column.ColumnName + ";");
                }
            }
            return fileContents;
        }

        public StringBuilder WriteGenericDBFunctions() {
            StringBuilder fileContents = new StringBuilder();
            fileContents.AppendLine("\t\tpublic SqlConnection GetSqlDataConnection()");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tSqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings[\"<CONNECTION_STRING_NAME\"].ConnectionString);");
            fileContents.AppendLine("\t\t\t\t\tconnect.Open();");
            fileContents.AppendLine("\t\t\t\t\treturn connect;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\t// Get standard database value");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic string GetSqlDatabaseString(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetString(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn \"\";");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic int GetSqlDatabaseInt(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetInt32(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn 0;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic double GetSqlDatabaseDouble(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetDouble(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn 0.0;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic DateTime GetSqlDatabaseDateTime(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetDateTime(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn new DateTime(DateTime.MinValue.Ticks);");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic bool GetSqlDatabaseBoolean(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetBoolean(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn new Boolean();");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic Guid GetSqlDatabaseGuid(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn Guid.Parse(reader.GetString(reader.GetOrdinal(columnName)));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn Guid.Parse(\"00000000-0000-0000-0000-000000000000\");");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\t// Get nullable database value");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic string GetSqlDatabaseNullableString(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetString(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn null;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic int? GetSqlDatabaseNullableInt(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetInt32(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn null;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic double? GetSqlDatabaseNullableDouble(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetDouble(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn null;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic DateTime? GetSqlDatabaseNullableDateTime(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetDateTime(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn null;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic bool? GetSqlDatabaseNullableBoolean(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn reader.GetBoolean(reader.GetOrdinal(columnName));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn null;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic Guid? GetSqlDatabaseNullableGuid(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn Guid.Parse(reader.GetString(reader.GetOrdinal(columnName)));");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\treturn null;");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t\t");
            fileContents.AppendLine("\t\tpublic Version GetSqlDatabaseVersion(SqlDataReader reader, string columnName)");
            fileContents.AppendLine("\t\t{");
            fileContents.AppendLine("\t\t\t\t\tif (!reader.IsDBNull(reader.GetOrdinal(columnName)))");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\ttry");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\tVersion value;");
            fileContents.AppendLine("\t\t\t\t\tVersion.TryParse(reader.GetString(reader.GetOrdinal(columnName)), out value);");
            fileContents.AppendLine("\t\t\t\t\treturn value;");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\tcatch");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn Version.Parse(\"0.0\");");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t\t\t\telse");
            fileContents.AppendLine("\t\t\t\t\t{");
            fileContents.AppendLine("\t\t\t\t\treturn Version.Parse(\"0.0\");");
            fileContents.AppendLine("\t\t\t\t\t}");
            fileContents.AppendLine("\t\t}");
            fileContents.AppendLine("\t}");
            fileContents.AppendLine("}");
            return fileContents;
        }
    }
}