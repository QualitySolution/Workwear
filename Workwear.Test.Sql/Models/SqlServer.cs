using System;
using MySql.Data.MySqlClient;

namespace QS.DBScripts.Models
{
	public class SqlServer
	{
		public string Name { get; set; }
		public string Address { get; set; }
		public uint Port { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string CommandStart { get; set; }
		public string CommandStop { get; set; }
		public string Group { get; set; }
		
		public string AddressAndPort => $"{Address}:{Port}";
		
		public MySqlConnectionStringBuilder ConnectionStringBuilder => new MySqlConnectionStringBuilder {
			Server = Address,
			Port = Port,
			UserID = Login,
			Password = Password
		};

		#region Commands
		public void Start()
		{
			RunCommand(CommandStart);
		}
		
		public void Stop()
		{
			RunCommand(CommandStop);
		}

		private void RunCommand(string command)
		{
			if(String.IsNullOrEmpty(command))
				return;
			System.Diagnostics.Process.Start(command);
		}
		#endregion
		
		public override bool Equals(object server)
		{
			if (server == null)
				return false;
			
			if (server is SqlServer sqlServer)
				return sqlServer.Name == Name;

			return false;
		}

		public override string ToString() => Name;
	}
}