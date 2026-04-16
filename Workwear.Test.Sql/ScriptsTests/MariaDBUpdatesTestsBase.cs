using System.Threading.Tasks;
using MySqlConnector;
using Testcontainers.MariaDb;
using Workwear.Test.Sql.Models;

namespace Workwear.Test.Sql.ScriptsTests {
	public abstract class MariaDBUpdatesTestsBase : UpdatesTestsBase {
		private readonly string imageName;
		public MariaDbContainer MariaDbContainer;
		public string ConnectionString { get; private set; }

		protected MariaDBUpdatesTestsBase(string imageName) {
			this.imageName = imageName;
		}
		
		
		/// <summary>
		/// Инициализация контейнера MariaDB
		/// </summary>
		protected override async Task InitialiseContainer()
		{
			if (MariaDbContainer != null)
				return;

			// Запуск контейнера MariaDB без создания базы по умолчанию
			MariaDbContainer = new MariaDbBuilder()
				.WithImage(imageName)
				.WithUsername("root")
				.WithPassword("root")
				.WithCommand("--character-set-server=utf8mb4", "--collation-server=utf8mb4_general_ci")
				.Build();

			await MariaDbContainer.StartAsync();
			ConnectionString = MariaDbContainer.GetConnectionString();
			MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(ConnectionString);
			server = new SqlServer {
				Name = MariaDbContainer.Name,
				Address = builder.Server,
				Port = builder.Port,
				Login = builder.UserID,
				Password = builder.Password
			};
		}

		/// <summary>
		/// Очистка и остановка контейнера
		/// </summary>
		protected override async Task DisposeContainer()
		{
			if (MariaDbContainer != null)
			{
				await MariaDbContainer.DisposeAsync();
				MariaDbContainer = null;
			}
		}
	}
}
