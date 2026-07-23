using System.Threading.Tasks;
using MySqlConnector;
using Testcontainers.MySql;
using Workwear.Test.Sql.Models;

namespace Workwear.Test.Sql.ScriptsTests {
	public abstract class MysqlUpdatesTestsBase : UpdatesTestsBase {
		private readonly string imageName;
		public MySqlContainer MySqlContainer;
		public string ConnectionString { get; private set; }

		protected MysqlUpdatesTestsBase(string imageName) {
			this.imageName = imageName;
		}
		
		
		/// <summary>
		/// Инициализация контейнера MySQL
		/// </summary>
		protected override async Task InitialiseContainer()
		{
			if (MySqlContainer != null)
				return;

			// Запуск контейнера MySQL без создания базы по умолчанию
			MySqlContainer = new MySqlBuilder()
				.WithImage(imageName)
				.WithUsername("root")
				.WithPassword("root")
				.WithCommand("--character-set-server=utf8mb4", "--collation-server=utf8mb4_general_ci")
				.Build();

			await MySqlContainer.StartAsync();
			
			ConnectionString = MySqlContainer.GetConnectionString();
			MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(ConnectionString);
			server = new SqlServer {
				Name = MySqlContainer.Name,
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
			if (MySqlContainer != null)
			{
				await MySqlContainer.DisposeAsync();
				MySqlContainer = null;
			}
		}
	}
}
