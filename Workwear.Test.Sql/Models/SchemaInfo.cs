using System.Collections.Generic;

namespace Workwear.Test.Sql.Models {
	public class SchemaInfo {
		public string Name { get; private set; }
		public string DataBaseColumn { get; set; }
		
		public readonly List<string> NameColumns = new List<string>();
		public readonly HashSet<string> SkipFields = new HashSet<string>();

		public static readonly SchemaInfo[] Schemas = new[] {
			new SchemaInfo {
				Name = "Tables",
				DataBaseColumn = "TABLE_SCHEMA",
				NameColumns = {"TABLE_NAME"},
				SkipFields = {"CREATE_TIME", "TABLE_SCHEMA", "UPDATE_TIME","AVG_ROW_LENGTH","TABLE_ROWS","AUTO_INCREMENT","INDEX_LENGTH","DATA_LENGTH", "DATA_FREE"}
			},
			new SchemaInfo {
				Name = "Columns",
				DataBaseColumn = "TABLE_SCHEMA",
				NameColumns = {"TABLE_NAME", "COLUMN_NAME"},
				SkipFields = {"TABLE_SCHEMA"}
			},
			new SchemaInfo {
				Name = "ReferentialConstraints",
				DataBaseColumn = "CONSTRAINT_SCHEMA",
				NameColumns = {"TABLE_NAME", "CONSTRAINT_NAME"},
				SkipFields = {"CONSTRAINT_SCHEMA", "UNIQUE_CONSTRAINT_SCHEMA"}
			},
			new SchemaInfo {
				Name = "Procedures",
				DataBaseColumn = "ROUTINE_SCHEMA",
				NameColumns = {"ROUTINE_NAME"},
				SkipFields = {"ROUTINE_SCHEMA", "CREATED", "LAST_ALTERED"}
			},
			new SchemaInfo {
				Name = "TableConstraints",
				DataBaseColumn = "CONSTRAINT_SCHEMA",
				NameColumns = {"TABLE_NAME", "CONSTRAINT_NAME"},
				SkipFields = {"CONSTRAINT_SCHEMA", "TABLE_SCHEMA"}
			},
			new SchemaInfo {
				Name = "KeyColumnUsage",
				DataBaseColumn = "CONSTRAINT_SCHEMA",
				NameColumns = {"TABLE_NAME", "COLUMN_NAME", "CONSTRAINT_NAME"},
				SkipFields = {"CONSTRAINT_SCHEMA", "TABLE_SCHEMA", "REFERENCED_TABLE_SCHEMA"}
			},
		};
	}
}
