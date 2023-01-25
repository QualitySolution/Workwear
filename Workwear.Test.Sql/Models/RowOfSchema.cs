using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QS.DBScripts.Models {
	public class RowOfSchema {

		public delegate void Log(string message);

		public RowOfSchema(string schema, string table, string name, DataRow row) {
			Schema = schema;
			Table = table;
			Name = name;
			Row = row;
		}
		
		public RowOfSchema(string schema, string db, DataRow row) {
			Schema = schema;
			DataBase = db;
			Row = row;
			
			switch(schema) {
				
				case "Tables":
					FullName = Name = (string)row[row.Table.Columns.IndexOf("TABLE_NAME")]; 
					break;
				case "Columns":
					Table = (string)row[row.Table.Columns.IndexOf("TABLE_NAME")]; 
					Name = (string)row[row.Table.Columns.IndexOf("COLUMN_NAME")];
					FullName = $"{Table}.{Name}";
					break;
				case "Foreign Keys":
					FullName = Name = (string)row[row.Table.Columns.IndexOf("CONSTRAINT_NAME")];
					break;
				case "Indexes":
					Table = (string)row[row.Table.Columns.IndexOf("TABLE_NAME")];
					Name = (string)row[row.Table.Columns.IndexOf("INDEX_NAME")];
					FullName = Name + Table;
					break;
				case "IndexColumns":
					Table = (string)row[row.Table.Columns.IndexOf("TABLE_NAME")];
					Name = (string)row[row.Table.Columns.IndexOf("COLUMN_NAME")] +"."+ (string)row[row.Table.Columns.IndexOf("INDEX_NAME")];
					FullName = Name + Table;
					break;

			}
			
		}

		public string Schema { get; }
		public string DataBase { get; }
		public string Table { get; }
		public string Name { get; }
		public string FullName { get; } 
		private DataRow Row { get; }

		private static readonly Dictionary<string, string[]> skippedFields = new Dictionary<string, string[]>() {
			{"Tables", new string[] {"CREATE_TIME", "TABLE_SCHEMA", "UPDATE_TIME","AVG_ROW_LENGTH","TABLE_ROWS","AUTO_INCREMENT","INDEX_LENGTH","DATA_LENGTH", "TABLE_COLLATION"}},
			{"Foreign Keys", new string[] {"TABLE_SCHEMA", "CONSTRAINT_SCHEMA", "REFERENCED_TABLE_SCHEMA" }},
			{"Indexes", new string[] {"CONSTRAINT_SCHEMA", "INDEX_SCHEMA",}},
			{"IndexColumns", new string[] {"CONSTRAINT_SCHEMA", "INDEX_SCHEMA" }},
			{"Columns", new string[] {"TABLE_SCHEMA", "COLLATION_NAME" }}
		};

		public bool IsDiff(RowOfSchema another, Log log) {
			bool result = false;
			if(FullName != another.FullName) {
				log($@"{Schema} - {DataBase}.{FullName} не соответствует {another.DataBase}.{another.FullName}");
				return true;
			}

			if(Row.ItemArray.Length == another.Row.ItemArray.Length)
				for(int i = 0; i < Row.ItemArray.Length; i++) {
					if(!skippedFields[Schema].Contains(Row.Table.Columns[i].ToString().ToUpper()) && Row.ItemArray[i].ToString() != another.Row.ItemArray[i].ToString()) { 
						log($@"{Schema} - Отличается {Row.Table.Columns[i].ToString().ToUpper()}
		{DataBase}.{FullName}.{Row.Table.Columns[i]} = {Row.ItemArray[i]}
		{another.DataBase}.{another.FullName}.{Row.Table.Columns[i]} = {another.Row.ItemArray[i]}");
						result = true;
					}
				}
			else {
				log("GetSchema вернул разное количество параметров.");
				return true;
			}

			return result;
		}
	}
}
