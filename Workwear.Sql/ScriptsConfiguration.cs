using System;
using System.Reflection;
using QS.DBScripts.Models;
using QS.Updater.DB;

namespace Workwear.Sql
{
	public class ScriptsConfiguration
	{
		public static CreationScript MakeCreationScript()
		{
			return new CreationScript {
				Version = new Version(2, 4),
				ResourceName = "Workwear.Sql.Scripts.new_empty.sql",
				ResourceAssembly = Assembly.GetAssembly(typeof(ScriptsConfiguration))
			};
		}

		public static UpdateConfiguration MakeUpdateConfiguration()
		{
			var configuration = new UpdateConfiguration();

			configuration.AddMicroUpdate (
				new Version (1, 0),
				new Version (1, 0, 4),
				"Workwear.Sql.Scripts.1.0.4.sql");
			configuration.AddMicroUpdate (
				new Version (1, 0, 4),
				new Version (1, 0, 5),
				"Workwear.Sql.Scripts.1.0.5.sql");
			configuration.AddUpdate (
				new Version (1, 0),
				new Version (1, 1),
				"Workwear.Sql.Scripts.Update to 1.1.sql");
			configuration.AddUpdate (
				new Version (1, 1),
				new Version (1, 2),
				"Workwear.Sql.Scripts.Update to 1.2.sql");
			configuration.AddMicroUpdate (
				new Version (1, 2),
				new Version (1, 2, 1),
				"Workwear.Sql.Scripts.1.2.1.sql");
			configuration.AddMicroUpdate (
				new Version (1, 2, 1),
				new Version (1, 2, 2),
				"Workwear.Sql.Scripts.1.2.2.sql");
			configuration.AddMicroUpdate (
				new Version (1, 2, 2),
				new Version (1, 2, 4),
				"Workwear.Sql.Scripts.1.2.4.sql");
			configuration.AddUpdate (
				new Version (1, 2),
				new Version (2, 0),
				"Workwear.Sql.Scripts.2.0.sql");
			configuration.AddMicroUpdate(
				new Version(2, 0),
				new Version(2, 0, 2),
				"Workwear.Sql.Scripts.2.0.2.sql");
			configuration.AddUpdate(
				new Version(2, 0),
				new Version(2, 1),
				"Workwear.Sql.Scripts.2.1.sql");
			configuration.AddMicroUpdate(
				new Version(2, 1),
				new Version(2, 1, 1),
				"Workwear.Sql.Scripts.2.1.1.sql");
			configuration.AddUpdate(
				new Version(2, 1),
				new Version(2, 2),
				"Workwear.Sql.Scripts.2.2.sql");
			configuration.AddUpdate(
				new Version(2, 2),
				new Version(2, 3),
				"Workwear.Sql.Scripts.2.3.sql");
			configuration.AddMicroUpdate(
				new Version(2, 3),
				new Version(2, 3, 3),
				"Workwear.Sql.Scripts.2.3.3.sql");
			configuration.AddUpdate(
				new Version(2, 3),
				new Version(2, 4),
				"Workwear.Sql.Scripts.2.4.sql");
			configuration.AddMicroUpdate(
				new Version(2, 4),
				new Version(2, 4, 1),
				"Workwear.Sql.Scripts.2.4.1.sql");
			configuration.AddMicroUpdate(
				new Version(2, 4, 1),
				new Version(2, 4, 3),
				"Workwear.Sql.Scripts.2.4.3.sql");
			configuration.AddUpdate(
				new Version(2, 4, 3),
				new Version(2, 5),
				"Workwear.Sql.Scripts.2.5.sql");

			return configuration;
		}
	}
}