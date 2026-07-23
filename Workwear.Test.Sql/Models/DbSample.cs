using System;

namespace Workwear.Test.Sql.Models
{
	public class DbSample
	{
		public string SqlFile { get; set; }
		public string Version { get; set; }
		public string DbName { get; set; }
		public Version TypedVersion => System.Version.Parse(Version);

		public override string ToString()
		{
			return System.IO.Path.GetFileName(SqlFile);
		}
	}
}
