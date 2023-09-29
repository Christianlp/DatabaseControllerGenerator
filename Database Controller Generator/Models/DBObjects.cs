using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Database_Controller_Generator.Models 
{
    public class DatabaseRequest {
        [Display(Name = "Hostname")]
        [Required(ErrorMessage = "Hostname is required.")]
        public string Hostname {get; set;}

        [Display(Name = "Database")]
        public string Database {get; set;}
       
        [Display(Name = "Username")]
        public string Username {get; set;}
       
        [Display(Name = "Password")]
        public string Password {get; set;}
    }

    public class DatabaseObjects {
        public string Hostname { get; set; }
        public string Database { get; set; }
        public List<DatabaseSchema> Schemas { get; set; }
    }

    public class DatabaseSchema
    {
        public int SchemaID { get; set; }
        public string SchemaName { get; set; }
        public List<DatabaseTable> Tables { get; set; }
    }

    public class DatabaseTable {
        public int SchemaID { get; set; }
        public int TableID { get; set; }
        public string TableName { get; set; }
        public List<DatabaseColumn> Columns { get; set; }
    }

    public class DatabaseColumn {
        public int TableID { get; set; }
        public int ColumnID { get; set; }
        public string ColumnName { get; set; }
        public string TypeName { get; set; }
        public bool Nullable { get; set; }
        public int ByteLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
    }

    public class ResultObjects {
        public DatabaseObjects DatabaseObjects { get; set; }
        public string FileContents { get; set; }
    }
}