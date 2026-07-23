using System;
using NUnit.Framework;
using Workwear.Domain.Visits;

namespace Workwear.Test.Domain.Visits {
	[TestFixture(TestOf = typeof(DaySchedule))]
	public class DayScheduleTest {

		[Test(Description = "Проверка расписания на определение будней")]
		[TestCase(null, 4, "09:00:00", "17:00:00", 10, ExpectedResult = true)]
		[TestCase(null, 4, "09:00:00", "17:00:00", 0, ExpectedResult = false)]
		[TestCase(null, 4, "09:00:00", "", 10, ExpectedResult = false)]
		[TestCase(null, 4, "", "17:00:00", 10, ExpectedResult = false)]
		[TestCase("2025-09-03", 0, "09:00:00", "17:00:00", 10, ExpectedResult = true)]
		[TestCase("2025-09-03", 0, "09:00:00", "17:00:00", 0, ExpectedResult = false)]
		[TestCase("2025-09-03", 0, "09:00:00", "", 10, ExpectedResult = false)]
		[TestCase("2025-09-03", 0, "", "17:00:00", 10, ExpectedResult = false)]
		[TestCase("2025-09-03", 4, "09:00:00", "17:00:00", 30, ExpectedResult = true)]
		public bool CheckIsWork(DateTime? date, int dayOfWeak, string start, string end, int interval) {
			return new DaySchedule {
				DayOfWeek = dayOfWeak,
				Date = date,
				Interval = interval,
				StartString = start,
				EndString = end
			}.IsWork;
		}
	}
}

