using System;
using System.Linq;
using System.Threading;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace QS.DBScripts.Models
{
	public class SqlServer
	{
		private static int waitingConnection = 120; //Seconds
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
			RunCommand(CommandStart, true);
			var conStr = ConnectionStringBuilder.GetConnectionString(true);
			var endTime = DateTime.Now.AddSeconds(waitingConnection);
			
			Console.Write($"Connecting to {Name}");
			//Ниже пробуем подключится в течении какого то времени. Так как после выполнения команды запуска, серверу
			//обычно требуется какое то время, прежде чем к нему может быть возможно подключится.
			while (true)
			{
				try
				{
					using(var connect = new MySqlConnection(conStr)) {
						connect.Open();
						connect.Close();
						break;	
					}
				} catch (Exception e)
				{
					Console.Write('.');
					if(DateTime.Now > endTime) {
						throw new TimeoutException($"Не удалось подключится к серверу {Name} за {waitingConnection} секунд.", e);
					}
				}

				Thread.Sleep(1000);
			}
			Console.WriteLine();
		}
		
		public void Stop()
		{
			RunCommand(CommandStop, true);
		}

		private void RunCommand(string command, bool wait)
		{
			if(String.IsNullOrEmpty(command))
				return;
			
			Console.WriteLine($"Run command: " + command);
			var parts = command.Split(' ', 2);
			var filename = parts.First();
			var args = parts.Length > 1 ? parts[1] : null;
			
			var process = System.Diagnostics.Process.Start(filename, args);
			if(wait) {
				process?.WaitForExit();
				if(process?.ExitCode != 0)
					Console.WriteLine($"Ошибка выполнения команды: {process?.ExitCode}");
			}
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
