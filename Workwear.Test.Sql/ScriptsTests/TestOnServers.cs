namespace Workwear.Test.Sql.ScriptsTests {
	#region MariaDB
	public class MariaDb_10_6 : MariaDBUpdatesTestsBase {
		public MariaDb_10_6() : base("mariadb:10.6") {
		}
	}
	
	public class MariaDb_10_latest : MariaDBUpdatesTestsBase {
		public MariaDb_10_latest() : base("mariadb:10") {
		}
	}
	
	public class MariaDb_11_latest : MariaDBUpdatesTestsBase {
		public MariaDb_11_latest() : base("mariadb:11") {
		}
	}
	
	public class MariaDb_latest : MariaDBUpdatesTestsBase {
		public MariaDb_latest() : base("mariadb:latest") {
		}
	}
	#endregion

	#region MySQL
	public class Mysql_8_0_24 : MysqlUpdatesTestsBase {
		public Mysql_8_0_24() : base("mysql:8.0.24") {
		}
	}
	
	public class Mysql_8_latest : MysqlUpdatesTestsBase {
		public Mysql_8_latest() : base("mysql:8") {
		}
	}
	
	public class Mysql_latest : MysqlUpdatesTestsBase {
		public Mysql_latest() : base("mysql:latest") {
		}
	}
	#endregion
}
