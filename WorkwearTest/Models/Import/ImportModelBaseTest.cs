using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NPOI.SS.UserModel;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.ViewModels;
using workwear.Models.Import;
using DataType = workwear.Models.Import.DataType;

namespace WorkwearTest.Models.Import {
	[TestFixture(TestOf = typeof(ImportModelBase<,>))]
	public class ImportModelBaseTest {
		[Test(Description = "Проверяем что правильно найдем максимальный уровень вложенности таблицы данных в случае если есть другие группы с более глубокими вложенностями")]
		public void AutoSetupColumns_CorrectFoundMaxDataLevel() {
			var progress = Substitute.For<IProgressBarDisplayable>();
			var dataTypeSubdivision = new DataType(DataTypeEnumTest.Subdivision);
			var dataParser = Substitute.For<IDataParser>();
			dataParser.SupportDataTypes.Returns(new List<DataType>{dataTypeSubdivision});
			dataParser.DetectDataType("шапка").Returns(dataTypeSubdivision);

			var model = new ImportModelTest(dataParser, typeof(CountersTest));
			model.LevelsCount = 4;
			model.ColumnsCount = 1;
			
			var row1 = Substitute.For<SheetRowTest>(); //Первая пустая строка
			var row2 = Substitute.For<SheetRowTest>(); //Сгруппирована шапка уровень 1
			row2.RowLevel.Returns(1);
			var row3 = Substitute.For<SheetRowTest>(); //Сгруппирована шапка уровень 2
			row3.RowLevel.Returns(2);
			var row4 = Substitute.For<SheetRowTest>(); //Сгруппирована шапка уровень 3
			row4.RowLevel.Returns(3);
			var row5 = Substitute.For<SheetRowTest>(); //Пустая строка
			var row6 = Substitute.For<SheetRowTest>(); //Заголовок таблицы
			row6.CellStringValue(0, null).Returns("Шапка");
			row6.CellValue(0).Returns("Шапка");
			var row7 = Substitute.For<SheetRowTest>(); //Группа в таблице
			row7.RowLevel.Returns(1);
			var row8 = Substitute.For<SheetRowTest>(); //Данные первой группы (Ожидаем что подобранный тип данных укажет на этот уровень!!!)
			row8.RowLevel.Returns(2);
			var row9 = Substitute.For<SheetRowTest>(); //Заголовок подвала
			var row10 = Substitute.For<SheetRowTest>(); // Группа в подвале уровень 1
			row10.RowLevel.Returns(1);
			var row11 = Substitute.For<SheetRowTest>(); // Группа в подвале уровень 2
			row11.RowLevel.Returns(2);
			var row12 = Substitute.For<SheetRowTest>(); // Группа в подвале уровень 3
			row12.RowLevel.Returns(3);
			model.XlsRows = new List<SheetRowTest>{row1, row2, row3, row4, row5, row6, row7, row8, row9, row10, row11, row12}; 
			
			model.AutoSetupColumns(progress);
			Assert.That(model.HeaderRow, Is.EqualTo(6));
			Assert.That(model.Columns[0].Title, Is.EqualTo("Шапка"));
			Assert.That(model.Columns[0].DataTypeByLevels[2].DataType, Is.EqualTo(dataTypeSubdivision));
		}
	}

	class ImportModelTest : ImportModelBase<DataTypeEnumTest, SheetRowTest> {
		public ImportModelTest(IDataParser dataParser, Type countersEnum, ViewModelBase matchSettingsViewModel = null) : base(dataParser, countersEnum, matchSettingsViewModel)
		{
		}

		protected override DataTypeEnumTest[] RequiredDataTypes { get; }
	}

	public class SheetRowTest : SheetRowBase<SheetRowTest> {
		public SheetRowTest() : base(new IRow[]{})
		{
		}
	}
	enum DataTypeEnumTest {
		[Display(Name = "Пропустить")]
		Unknown,
		[Display(Name = "Подразделение")]
		Subdivision,
		[Display(Name = "Должность")]
		Post,
	}

	enum CountersTest {
		[Display(Name = "Подразделение")]
		Subdivision
	}
}
