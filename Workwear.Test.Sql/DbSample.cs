namespace Workwear.Test.Sql
{
	public class DbSample
	{
		public string SqlFile { get; set; }
		public string Version { get; set; }
		public string DbName { get; set; }

		public override string ToString()
		{
			return $"{SqlFile}";
		}
	}
}