using System.Data;
using System.Linq;

namespace Workwear.Test.Sql.Models {
	public class RowOfSchema {

		public delegate void Log(string message);

		public RowOfSchema(SchemaInfo schema, DataRow row) {
			Schema = schema;
			DataBase = (string)row[row.Table.Columns.IndexOf(schema.DataBaseColumn)];
			Row = row;
			
			var nameParts = schema.NameColumns.Select(x => (string)row[row.Table.Columns.IndexOf(x)]).ToArray();
			FullName = string.Join(".", nameParts);
		}

		public SchemaInfo Schema { get; }
		public string DataBase { get; }
		public string FullName { get; } 
		private DataRow Row { get; }

		public bool IsDiff(RowOfSchema another, Log log) {
			bool result = false;
			if(Row.ItemArray.Length != another.Row.ItemArray.Length) {
				log("GetSchema вернул разное количество параметров.");
				return true;
			}
			
			for(int i = 0; i < Row.ItemArray.Length; i++) {
				if(!Schema.SkipFields.Contains(Row.Table.Columns[i].ToString().ToUpper()) && Row.ItemArray[i].ToString() != another.Row.ItemArray[i].ToString()) { 
					log($@"{Schema.Name} - Отличается {Row.Table.Columns[i].ToString().ToUpper()}
		{DataBase}.{FullName}.{Row.Table.Columns[i]} = {Row.ItemArray[i]}
		{another.DataBase}.{another.FullName}.{Row.Table.Columns[i]} = {another.Row.ItemArray[i]}");
					result = true;
				}
			}

			return result;
		}
	}
}
