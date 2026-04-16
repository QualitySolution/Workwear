using MySqlConnector;

namespace Workwear.Test.Sql.Models
{
	public class SqlServer
	{
		public string Name { get; set; }
		public string Address { get; set; }
		public uint Port { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		
		public string AddressAndPort => $"{Address}:{Port}";
		
		public MySqlConnectionStringBuilder ConnectionStringBuilder => new MySqlConnectionStringBuilder {
			Server = Address,
			Port = Port,
			UserID = Login,
			Password = Password
		};
		
	}
}
