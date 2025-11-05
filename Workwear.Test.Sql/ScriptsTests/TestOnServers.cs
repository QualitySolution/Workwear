namespace Workwear.Test.Sql.ScriptsTests {
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
}
