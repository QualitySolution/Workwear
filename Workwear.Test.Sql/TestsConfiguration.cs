using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Workwear.Test.Sql
{
	public class TestsConfiguration
	{
		private static string testDbConfigFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
			".tests_resources/Workwear/databases.json");
		private static IConfiguration _config;

		public static IConfiguration Configuration
		{
			get
			{
				if (_config == null)
				{
					var builder = new ConfigurationBuilder().AddJsonFile(testDbConfigFile, optional: false);
					_config = builder.Build();
				}

				return _config;
			}
		}
	}
}