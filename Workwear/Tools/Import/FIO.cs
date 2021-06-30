using System;
namespace workwear.Tools.Import
{
	public class FIO
	{
		public string LastName;
		public string FirstName;
		public string Patronymic;

		public string GetHash() => LastName + FirstName + Patronymic;
	}
}
