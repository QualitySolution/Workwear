using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Workwear.Test.Sql.Models;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using System;

namespace Workwear.Test.Sql
{
	[TestFixture]
	[NonParallelizable]
	public class ServerFunctionsTests
	{
		private SqlServer testServer;
		
		[OneTimeSetUp]
		public void SetUp()
		{
			var configuration = TestsConfiguration.Configuration;
			
			// Получаем сервер с именем, начинающимся с "Plutus"
			var servers = configuration.GetSection("SQLServers").Get<List<SqlServer>>();
			testServer = servers?.FirstOrDefault(s => s.Name?.StartsWith("Plutus") == true);
			
			Assert.IsNotNull(testServer, "Сервер с именем, начинающимся с 'Plutus', не найден в конфигурации");
		}
		
		[TestCase(1, 12, "2023-01-01", "2023-01-01", "2025-12-31", null, null, 3, TestName = "Базовый сценарий: 3 выдачи за 3 года")]
		[TestCase(1, 0, "2023-01-01", "2023-01-01", "2023-12-31", null, null, 0, TestName = "Нулевой период должен вернуть 0")]
		[TestCase(1, 12, null, "2023-01-01", "2023-12-31", null, null, 0, TestName = "Null next_issue должен вернуть 0")]
		[TestCase(1, 12, "2024-01-01", "2023-01-01", "2023-12-31", null, null, 0, TestName = "next_issue после end_date должен вернуть 0")]
		[TestCase(2, 6, "2022-06-01", "2023-01-01", "2023-12-31", null, null, 4, TestName = "next_issue до begin_date - учет только в периоде")]
		[TestCase(3, 12, "2023-01-01", "2023-01-01", "2024-12-31", null, null, 6, TestName = "Количество больше 1: amount=3, 2 выдачи = 6")]
		[TestCase(1, 3, "2023-01-01", "2023-01-01", "2023-12-31", null, null, 4, TestName = "Короткий период: каждые 3 месяца = 4 выдачи за год")]
		[TestCase(1, 3, "2023-02-01", "2023-01-01", "2023-12-31", 3, 10, 3, TestName = "Сезонный период март-октябрь, 3 выдачи: март, июнь, сентябрь")]
		[TestCase(1, 3, "2023-05-01", "2023-01-01", "2023-12-31", 3, 10, 2, TestName = "Сезонный период март-октябрь, 2 выдачи: май, август")]
		[TestCase(1, 3, "2023-01-01", "2023-01-01", "2024-12-31", 11, 3, 4, TestName = "Сезонный период ноябрь-март через год, 4 выдачи: январь 23, ноябрь 23, февраль 24, ноябрь 24")]
		[TestCase(1, 3, "2023-04-01", "2023-01-01", "2024-12-31", 11, 3, 3, TestName = "Сезонный период ноябрь-март через год, 3 выдачи: ноябрь 23, февраль 24, ноябрь 24")]
		public void Test_count_issue(int amount, int normPeriod, string nextIssue, string beginDate, string endDate, int? beginIssuePeriod, int? endIssuePeriod, int expectedResult)
		{
			var connectionStringBuilder = testServer.ConnectionStringBuilder;
			connectionStringBuilder.Database = UpdatesTests.CurrentDdName;
			using(var connection = new MySqlConnection(connectionStringBuilder.ConnectionString)) {
				connection.Open();

				var sql =
					"SELECT count_issue(@amount, @norm_period, @next_issue, @begin_date, @end_date, @begin_Issue_Period, @end_Issue_Period)";
				using var command = new MySqlCommand(sql, connection);

				command.Parameters.AddWithValue("@amount", amount);
				command.Parameters.AddWithValue("@norm_period", normPeriod);
				command.Parameters.AddWithValue("@next_issue", nextIssue);
				command.Parameters.AddWithValue("@begin_date", beginDate);
				command.Parameters.AddWithValue("@end_date", endDate);
				command.Parameters.AddWithValue("@begin_Issue_Period", beginIssuePeriod);
				command.Parameters.AddWithValue("@end_Issue_Period", endIssuePeriod);

				var result = command.ExecuteScalar();
				Assert.AreEqual(expectedResult, Convert.ToInt32(result));
			}
		}
	}
}
