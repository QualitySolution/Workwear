using System.Data;

namespace QS.DBScripts.Models {
	public class ColumnOfTable {

		public delegate void Log(string message);

		public ColumnOfTable(string table, string name, DataRow row) {
			Table = table;
			Name = name;
			Row = row;
		}

		public string Table { get; }
		public string Name { get; }
		public string FullName => $"{Table}.{Name}";
		private DataRow Row { get; }
		delegate void Message(string str);

		public bool IsDiff(DataRow another, Log log) {
			bool result = false;
			if(Row[2].ToString() != another[2].ToString() || Row[3].ToString() != another[3].ToString()) {
				log($@"Колонка {Row[1]}.{FullName} не соответствует {another[1]}.{another[2]}.{another[3]}");
				return true;
			}

			for(int i = 4; i < 20; i++) {
				if(Row[i].ToString() != another[i].ToString()) {
					log($@"У {Row[1]}.{FullName} и {another[1]}.{another[2]}.{another[3]} отличаются {Row.Table.Columns[i]}");
					result = true;
				}
			}

			return result;
		}

		public bool IsDiff(ColumnOfTable another, Log log) {
			return this.IsDiff(another.Row, log);
		}
	}
}
