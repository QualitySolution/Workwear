using System;
using NUnit.Framework;
using Workwear.Domain.Visits;

namespace Workwear.Test.Domain {
	[TestFixture(TestOf = typeof(DaySchedule))]
	public class DayScheduleTest {

		[Test(Description = "Проверяем проверку рассписания на опеределение будней")]
		[TestCase("", 4, "09:00:00", "17:00:00", 10, ExpectedResult = true)]
		[TestCase("", 4, "09:00:00", "17:00:00", 0, ExpectedResult = false)]
		[TestCase("", 4, "09:00:00", "", 10, ExpectedResult = false)]
		[TestCase("", 4, "", "17:00:00", 10, ExpectedResult = false)]
		[TestCase("2025-09-03", 0, "09:00:00", "17:00:00", 10, ExpectedResult = true)]
		[TestCase("2025-09-03", 0, "09:00:00", "17:00:00", 0, ExpectedResult = false)]
		[TestCase("2025-09-03", 0, "09:00:00", "", 10, ExpectedResult = false)]
		[TestCase("2025-09-03", 0, "", "17:00:00", 10, ExpectedResult = false)]
		[TestCase("2025-09-03", 4, "09:00:00", "17:00:00", 30, ExpectedResult = true)]
		public bool CheckIsWork(string date, int dayOfWeak, string Start, string End, int interval) {
			return new DaySchedule {
				DayOfWeak = dayOfWeak,
				Date = DateTime.Parse(date),
				Interval = 0,
				StartString = Start,
				EndString = End
			}.IsWork;
		}
	}
}

