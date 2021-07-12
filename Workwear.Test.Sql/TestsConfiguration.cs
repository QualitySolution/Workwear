using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Workwear.Test.Sql
{
	public class TestsConfiguration
	{
		private static string basePath = ".tests_resources/Workwear/";
		private static string testDbConfigFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), basePath, "databases.json");
		
		public static string MakeSQLScriptPath(string filename) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), basePath, filename);
		
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