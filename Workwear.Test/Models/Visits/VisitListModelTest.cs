using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Workwear.Domain.Visits;
using Workwear.Models.Visits;

namespace Workwear.Test.Models.Visits
{
    [TestFixture]
    public class VisitListModelTest
    {
        #region Вспомогательные методы создания тестовых данных

        private DaySchedule CreateWorkDaySchedule(int dayOfWeek, string start = "09:00", string end = "18:00", int interval = 60)
        {
            return new DaySchedule
            {
                DayOfWeek = dayOfWeek,
                Date = null,
                StartString = start,
                EndString = end,
                Interval = interval
            };
        }

        private DaySchedule CreateNonWorkDaySchedule(int dayOfWeek)
        {
            return new DaySchedule
            {
                DayOfWeek = dayOfWeek,
                Date = null,
                StartString = null,
                EndString = null,
                Interval = 0
            };
        }

        private DaySchedule CreateExclusiveDaySchedule(DateTime date, string start = "10:00", string end = "16:00", int? interval = 30, bool? isWork = null)
        {
            return new DaySchedule
            {
                DayOfWeek = 0,
                Date = date,
                StartString = (isWork ?? true) ? start : null,
                EndString = (isWork ?? true) ? end : null,
                Interval = (isWork ?? true) ? interval : null
            };
        }

        private Visit CreateVisit(DateTime visitTime, string comment = "Test visit")
        {
            return new Visit
            {
                Id = 1,
                VisitTime = visitTime,
                Comment = comment,
                CreateDate = DateTime.Now
            };
        }

        #endregion

        #region Тесты конструктора

        [Test]
        public void Constructor_WithMixedSchedule_SeparatesRegularAndExclusiveDays()
        {
            // Arrange
            var regularSchedule = CreateWorkDaySchedule(1); // Понедельник
            var exclusiveSchedule = CreateExclusiveDaySchedule(new DateTime(2023, 12, 25)); // Конкретная дата
            var allSchedule = new List<DaySchedule> { regularSchedule, exclusiveSchedule };

            // Act
            var model = new VisitListModel(allSchedule);

            // Assert
            Assert.That(model.DaysSchedule.Count, Is.EqualTo(1));
            Assert.That(model.ExclusiveDays.Count, Is.EqualTo(1));
            Assert.That(model.DaysSchedule.First(), Is.EqualTo(regularSchedule));
            Assert.That(model.ExclusiveDays.First(), Is.EqualTo(exclusiveSchedule));
        }

        [Test]
        public void Constructor_WithEmptySchedule_InitializesEmptyCollections()
        {
            // Arrange
            var emptySchedule = new List<DaySchedule>();

            // Act
            var model = new VisitListModel(emptySchedule);

            // Assert
            Assert.That(model.DaysSchedule, Is.Empty);
            Assert.That(model.ExclusiveDays, Is.Empty);
            Assert.That(model.Items, Is.Empty);
        }

        #endregion

        #region Тесты IsWorkDay

        [Test]
        public void IsWorkDay_WithExclusiveWorkDay_ReturnsTrue()
        {
            // Arrange
            var testDate = new DateTime(2023, 12, 25);
            var exclusiveWorkDay = CreateExclusiveDaySchedule(testDate, isWork: true);
            var model = new VisitListModel(new List<DaySchedule> { exclusiveWorkDay });

            // Act
            var result = model.IsWorkDay(testDate);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsWorkDay_WithExclusiveNonWorkDay_ReturnsFalse()
        {
            // Arrange
            var testDate = new DateTime(2023, 12, 25);
            var exclusiveNonWorkDay = CreateExclusiveDaySchedule(testDate, isWork: false);
            var model = new VisitListModel(new List<DaySchedule> { exclusiveNonWorkDay });

            // Act
            var result = model.IsWorkDay(testDate);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsWorkDay_WithRegularWorkDay_ReturnsTrue()
        {
            // Arrange
            var monday = new DateTime(2023, 12, 18); // Понедельник
            var workDaySchedule = CreateWorkDaySchedule(1); // Понедельник = 1
            var model = new VisitListModel(new List<DaySchedule> { workDaySchedule });

            // Act
            var result = model.IsWorkDay(monday);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsWorkDay_WithRegularNonWorkDay_ReturnsFalse()
        {
            // Arrange
            var sunday = new DateTime(2023, 12, 17); // Воскресенье
            var nonWorkDaySchedule = CreateNonWorkDaySchedule(0); // Воскресенье = 0
            var model = new VisitListModel(new List<DaySchedule> { nonWorkDaySchedule });

            // Act
            var result = model.IsWorkDay(sunday);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsWorkDay_WithNoScheduleForDay_ReturnsFalse()
        {
            // Arrange
            var tuesday = new DateTime(2023, 12, 19); // Вторник
            var mondaySchedule = CreateWorkDaySchedule(1); // Только понедельник
            var model = new VisitListModel(new List<DaySchedule> { mondaySchedule });

            // Act
            var result = model.IsWorkDay(tuesday);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsWorkDay_ExclusiveDayOverridesRegularSchedule()
        {
            // Arrange
            var monday = new DateTime(2023, 12, 18); // Понедельник
            var regularWorkDay = CreateWorkDaySchedule(1); // Обычно рабочий понедельник
            var exclusiveNonWorkDay = CreateExclusiveDaySchedule(monday, isWork: false); // Но конкретный понедельник - выходной
            var model = new VisitListModel(new List<DaySchedule> { regularWorkDay, exclusiveNonWorkDay });

            // Act
            var result = model.IsWorkDay(monday);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region Тесты PutVisits

        [Test]
        public void PutVisits_WithUniqueVisitTimes_AddsAllVisits()
        {
            // Arrange
            var model = new VisitListModel(new List<DaySchedule>());
            var visit1 = CreateVisit(new DateTime(2023, 12, 18, 10, 0, 0));
            var visit2 = CreateVisit(new DateTime(2023, 12, 18, 11, 0, 0));
            var visits = new List<Visit> { visit1, visit2 };

            // Act
            model.PutVisits(visits);

            // Assert
            Assert.That(model.Items.Count, Is.EqualTo(2));
            Assert.That(model.Items.ContainsKey(visit1.VisitTime), Is.True);
            Assert.That(model.Items.ContainsKey(visit2.VisitTime), Is.True);
        }

        [Test]
        public void PutVisits_WithDuplicateVisitTimes_AdjustsTimesToAvoidCollisions()
        {
            // Arrange
            var model = new VisitListModel(new List<DaySchedule>());
            var visitTime = new DateTime(2023, 12, 18, 10, 0, 0);
            var visit1 = CreateVisit(visitTime);
            var visit2 = CreateVisit(visitTime); // Одинаковое время
            var visits = new List<Visit> { visit1, visit2 };

            // Act
            model.PutVisits(visits);

            // Assert
            Assert.That(model.Items.Count, Is.EqualTo(2));
            Assert.That(model.Items.ContainsKey(visitTime), Is.True);
            Assert.That(model.Items.ContainsKey(visitTime.AddSeconds(1)), Is.True);
        }

        [Test]
        public void PutVisits_WithEmptyList_DoesNothing()
        {
            // Arrange
            var model = new VisitListModel(new List<DaySchedule>());
            var emptyVisits = new List<Visit>();

            // Act
            model.PutVisits(emptyVisits);

            // Assert
            Assert.That(model.Items, Is.Empty);
        }

        #endregion

        #region Тесты FillScheduleOfDay

        [Test]
        public void FillScheduleOfDay_WithWorkDay_GeneratesScheduleSlots()
        {
            // Arrange
            var testDate = new DateTime(2023, 12, 18); // Понедельник
            var workDaySchedule = CreateWorkDaySchedule(1, "09:00", "11:00", 60); // 2 часа с интервалом 60 минут
            var model = new VisitListModel(new List<DaySchedule> { workDaySchedule });

            // Act
            model.FillScheduleOfDay(testDate);

            // Assert
            Assert.That(model.Items.Count, Is.EqualTo(2)); // 09:00 и 10:00
            Assert.That(model.Items.ContainsKey(testDate.Date.Add(TimeSpan.Parse("09:00"))), Is.True);
            Assert.That(model.Items.ContainsKey(testDate.Date.Add(TimeSpan.Parse("10:00"))), Is.True);
        }

        [Test]
        public void FillScheduleOfDay_WithExistingVisits_DoesNotOverwrite()
        {
            // Arrange
            var testDate = new DateTime(2023, 12, 18); // Понедельник
            var workDaySchedule = CreateWorkDaySchedule(1, "09:00", "11:00", 60);
            var model = new VisitListModel(new List<DaySchedule> { workDaySchedule });
            
            var existingVisitTime = testDate.Date.Add(TimeSpan.Parse("09:00"));
            var visit = CreateVisit(existingVisitTime);
            model.PutVisits(new List<Visit> { visit });

            // Act
            model.FillScheduleOfDay(testDate);

            // Assert
            Assert.That(model.Items.Count, Is.EqualTo(2)); // Существующий визит + новый слот на 10:00
            Assert.That(model.Items[existingVisitTime].Visit, Is.Not.Null); // Проверяем, что существующий визит не перезаписан
        }

        [Test]
        public void FillScheduleOfDay_WithExclusiveSchedule_UsesExclusiveSchedule()
        {
            // Arrange
            var testDate = new DateTime(2023, 12, 25);
            var regularSchedule = CreateWorkDaySchedule(1, "09:00", "18:00", 60); // Обычное расписание
            var exclusiveSchedule = CreateExclusiveDaySchedule(testDate, "10:00", "12:00", 30); // Особое расписание
            var model = new VisitListModel(new List<DaySchedule> { regularSchedule, exclusiveSchedule });

            // Act
            model.FillScheduleOfDay(testDate);

            // Assert
            Assert.That(model.Items.Count, Is.EqualTo(4)); // 10:00, 10:30, 11:00, 11:30
            Assert.That(model.Items.ContainsKey(testDate.Date.Add(TimeSpan.Parse("10:00"))), Is.True);
            Assert.That(model.Items.ContainsKey(testDate.Date.Add(TimeSpan.Parse("10:30"))), Is.True);
            Assert.That(model.Items.ContainsKey(testDate.Date.Add(TimeSpan.Parse("11:00"))), Is.True);
            Assert.That(model.Items.ContainsKey(testDate.Date.Add(TimeSpan.Parse("11:30"))), Is.True);
        }

        [Test]
        public void FillScheduleOfDay_WithNonWorkDay_GeneratesNoSlots()
        {
            // Arrange
            var testDate = new DateTime(2023, 12, 17); // Воскресенье
            var nonWorkDaySchedule = CreateNonWorkDaySchedule(0);
            var model = new VisitListModel(new List<DaySchedule> { nonWorkDaySchedule });

            // Act
            model.FillScheduleOfDay(testDate);

            // Assert
            Assert.That(model.Items, Is.Empty);
        }

        #endregion

        #region Интеграционные тесты

        [Test]
        public void IntegrationTest_CompleteWorkflow()
        {
            // Arrange
            var testDate = new DateTime(2023, 12, 18); // Понедельник
            var workDaySchedule = CreateWorkDaySchedule(1, "09:00", "12:00", 60); // 3 слота
            var model = new VisitListModel(new List<DaySchedule> { workDaySchedule });

            // Добавляем существующий визит
            var existingVisit = CreateVisit(testDate.Date.Add(TimeSpan.Parse("10:00")));
            model.PutVisits(new List<Visit> { existingVisit });

            // Act
            model.FillScheduleOfDay(testDate);

            // Assert
            Assert.That(model.Items.Count, Is.EqualTo(3)); // 09:00 (пустой), 10:00 (с визитом), 11:00 (пустой)
            
            // Проверяем, что слот в 10:00 содержит визит
            var tenAMSlot = model.Items[testDate.Date.Add(TimeSpan.Parse("10:00"))];
            Assert.That(tenAMSlot.Visit, Is.Not.Null);
            Assert.That(tenAMSlot.Visit, Is.EqualTo(existingVisit));

            // Проверяем, что другие слоты пустые
            var nineAMSlot = model.Items[testDate.Date.Add(TimeSpan.Parse("09:00"))];
            var elevenAMSlot = model.Items[testDate.Date.Add(TimeSpan.Parse("11:00"))];
            Assert.That(nineAMSlot.Visit, Is.Null);
            Assert.That(elevenAMSlot.Visit, Is.Null);
        }

        [Test]
        public void IntegrationTest_WeekdayModuloCalculation()
        {
            // Arrange
            var sunday = new DateTime(2023, 12, 17); // Воскресенье (DayOfWeek = 7)
            var mondaySchedule = CreateWorkDaySchedule(1);
            var sundaySchedule = CreateWorkDaySchedule(7);
            var model = new VisitListModel(new List<DaySchedule> { mondaySchedule, sundaySchedule });

            // Act & Assert
            Assert.That(model.IsWorkDay(sunday), Is.True); // Должно найти расписание для воскресенья
            Assert.That(model.IsWorkDay(sunday.AddDays(1)), Is.True); // Понедельник
            Assert.That(model.IsWorkDay(sunday.AddDays(2)), Is.False); // Вторник - нет расписания
        }
        
        [Test(Description = "Превращаем рабочий выходной день")]
        public void IntegrationTest_WorkToWeekend()
        {
	        // Arrange
	        var wednesday = new DateTime(2025, 9, 10); // Среда
	        var scheduleList = new List<DaySchedule>();
	        scheduleList.Add(CreateWorkDaySchedule(1));
	        scheduleList.Add(CreateWorkDaySchedule(2));
	        scheduleList.Add(CreateWorkDaySchedule(3));
	        scheduleList.Add(CreateWorkDaySchedule(4));
	        scheduleList.Add(CreateWorkDaySchedule(5));
	        scheduleList.Add(CreateExclusiveDaySchedule(new DateTime(2025, 9, 10), null, null, null)); // Делаем среду выходным днем
	        var model = new VisitListModel(scheduleList);

	        // Проверяем дни недели
	        Assert.That(model.IsWorkDay(wednesday), Is.False); // Должно найти уникальное расписание
	        Assert.That(model.IsWorkDay(wednesday.AddDays(7)), Is.True); // Следующая среда - по расписанию
	        
	        model.FillScheduleOfDay(wednesday.AddDays(-2));
	        model.FillScheduleOfDay(wednesday.AddDays(-1));
	        model.FillScheduleOfDay(wednesday);
	        model.FillScheduleOfDay(wednesday.AddDays(1));
	        model.FillScheduleOfDay(wednesday.AddDays(2));
	        model.FillScheduleOfDay(wednesday.AddDays(7));
	        
	        Assert.That(model.Items, Is.Not.Empty);
	        Assert.IsFalse(model.Items.Any(x => x.Key == new DateTime(2025, 9, 17, 8, 0, 0)));
	        Assert.IsTrue(model.Items.Any(x => x.Key == new DateTime(2025, 9, 17, 9, 0, 0)));
	        Assert.IsTrue(model.Items.Any(x => x.Key == new DateTime(2025, 9, 17, 17, 0, 0)));
	        Assert.IsFalse(model.Items.Any(x => x.Key == new DateTime(2025, 9, 17, 18, 0, 0)), "18:00 уже конец работы");
	        Assert.IsFalse(model.Items.Any(x => x.Key.Date == new DateTime(2025, 9, 10)), "10 сентября - среда, выходной");
        }
        
        [Test(Description = "Превращаем выходной день в рабочий")]
        public void IntegrationTest_WeekendToWork()
        {
	        // Arrange
	        var sunday = new DateTime(2025, 9, 13); // Суббота
	        var scheduleList = new List<DaySchedule>();
	        scheduleList.Add(CreateWorkDaySchedule(1));
	        scheduleList.Add(CreateWorkDaySchedule(2));
	        scheduleList.Add(CreateWorkDaySchedule(3));
	        scheduleList.Add(CreateWorkDaySchedule(4));
	        scheduleList.Add(CreateWorkDaySchedule(5));
	        scheduleList.Add(CreateExclusiveDaySchedule(new DateTime(2025, 9, 13), "09:00", "18:00", 60)); // Делаем субботу рабочим днем
	        var model = new VisitListModel(scheduleList);

	        // Проверяем дни недели
	        Assert.That(model.IsWorkDay(sunday), Is.True); // Должно найти уникальное расписание для субботы
	        Assert.That(model.IsWorkDay(sunday.AddDays(7)), Is.False); // Следующая суббота - по расписанию выходной
	        
	        model.FillScheduleOfDay(sunday.AddDays(-2));
	        model.FillScheduleOfDay(sunday.AddDays(-1));
	        model.FillScheduleOfDay(sunday);
	        model.FillScheduleOfDay(sunday.AddDays(1));
	        model.FillScheduleOfDay(sunday.AddDays(2));
	        model.FillScheduleOfDay(new DateTime(2025, 9, 20));
	        
	        Assert.That(model.Items, Is.Not.Empty);
	        Assert.IsTrue(model.Items.Any(x => x.Key == new DateTime(2025, 9, 13, 9, 0, 0)));
	        Assert.IsTrue(model.Items.Any(x => x.Key == new DateTime(2025, 9, 13, 17, 0, 0)));
	        Assert.IsFalse(model.Items.Any(x => x.Key == new DateTime(2025, 9, 13, 18, 0, 0)), "18:00 уже конец работы");
	        Assert.IsFalse(model.Items.Any(x => x.Key.Date == new DateTime(2025, 9, 14)), "14 сентября - воскресенье, выходной");
	        Assert.IsFalse(model.Items.Any(x => x.Key.Date == new DateTime(2025, 9, 20)), "200 сентября - следующая суббота, выходной");
        }

        
        [Test(Description = "В один день несколько интервалов работы")]
        public void IntegrationTest_MultiIntervals()
        {
	        var tuesday = new DateTime(2025, 9, 16);
	        // Arrange
	        var scheduleList = new List<DaySchedule>();
	        scheduleList.Add(CreateWorkDaySchedule(2, "09:00", "12:00", 60));
	        scheduleList.Add(CreateWorkDaySchedule(2, "13:00", "18:00", 30));
	        var model = new VisitListModel(scheduleList);
	        
	        model.FillScheduleOfDay(tuesday);
	        
	        Assert.That(model.Items, Is.Not.Empty);
	        Assert.IsFalse(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 8, 0, 0)));
	        Assert.IsTrue(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 9, 0, 0)));
	        Assert.IsFalse(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 9, 30, 0)), "9:30 не должно быть так как с утра интервал по часу");
	        Assert.IsFalse(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 12, 0, 0)), "обеденный перерыв");
	        Assert.IsFalse(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 12, 30, 0)), "обеденный перерыв");
	        Assert.IsTrue(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 13, 0, 0)));
	        Assert.IsTrue(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 13, 30, 0)), "Во второй половине дня интервалы по 30 минут");
	        Assert.IsTrue(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 17, 30, 0)), "Во второй половине дня интервалы по 30 минут");
	        Assert.IsFalse(model.Items.Any(x => x.Key == new DateTime(2025, 9, 16, 18, 0, 0)), "18:00 уже конец работы");
        }
        #endregion
    }
}
