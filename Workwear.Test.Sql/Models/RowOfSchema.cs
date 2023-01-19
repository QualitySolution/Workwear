using System.Data;

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
					FullName = Name = (string)row[2];
					break;
				case "Columns":
					Table = (string)row[2];
					Name = (string)row[3];
					FullName = $"{Table}.{Name}";
					break;
				case "Foreign Keys":
					FullName = Name = (string)row[2];
					break;
				case "Indexes":
					Table = (string)row[3];
					Name = (string)row[2];
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
		delegate void Message(string str);

		public bool IsDiff(RowOfSchema another, Log log) {
			bool result = false;
			if(FullName != another.FullName) {
				log($@"{Schema} - {DataBase}.{FullName} не соответствует {another.DataBase}.{another.FullName}");
				return true;
			}

			if(Row.ItemArray.Length == another.Row.ItemArray.Length)
				for(int i = 0; i < Row.ItemArray.Length; i++) {
					if(Row.ItemArray[i].ToString() != another.Row.ItemArray[i].ToString()) { //есть некоторые не строковые параметры, но их пока опускаю
						log($@"{Schema} - У {DataBase}.{FullName} и {another.DataBase}.{another.FullName} отличаются {Row.Table.Columns[i]}");
						result = true;
					}
				}
			else {
				log("GetSchem Вернул разное колличество параметров.");
				return true;
			}

			return result;
		}

		public bool IsDiff(DataRow another, Log log) {
			return this.IsDiff(new RowOfSchema(Schema,DataBase,another),log);
		}
	}
}
