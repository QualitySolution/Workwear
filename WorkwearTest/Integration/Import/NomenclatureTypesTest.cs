using NUnit.Framework;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Testing.DB;
using Workwear.Measurements;
using Workwear.Models.Import;

namespace WorkwearTest.Integration.Import {
	[TestFixture(TestOf = typeof(NomenclatureTypes))]
	[Category("Integrated")]
	public class NomenclatureTypesTest : InMemoryDBGlobalConfigTestFixtureBase {
		
		
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
			NewSessionWithSameDB();
			Uow = UnitOfWorkFactory.CreateWithoutRoot();
			MakeDefaultData(Uow);
		}
		
		[OneTimeTearDown]
		public void Shutdown()
		{
			Uow.Dispose();
			NewSessionWithNewDB();
		}

		private IUnitOfWork Uow;
		
		[Test(Description = "Проверяем подбор типа номенклатуры")]
		[TestCase("Рукавицы, утепленные", "Рукавицы")] //Реальный кейс, тут важным является наличие запятой.
		[TestCase("Изделия трикотажные перчаточные ", "Перчатки")]
		[TestCase("Фильтра для полумасок/для зачисток/Фильтра ФГ-5М марки А2", "Имущество")] //Тут возможно надо будет сменить категорию на СИЗОД, данных кейс просто фиксирует что номенклатура попадает в какой то тип.
		public void ParseNomenclatureName_Cases(string nomenclatureName, string typeName) {
			var nomenclatureTypes = new NomenclatureTypes(Uow, new SizeService(), false);
			var type = nomenclatureTypes.ParseNomenclatureName(nomenclatureName);
			Assert.That(type, Is.Not.Null);
			Assert.That(type.Name, Is.EqualTo(typeName));
		}

		void MakeDefaultData(IUnitOfWork uow) {
			uow.Save(new MeasurementUnits {
				Name = "шт.",
				OKEI = "796"
			});
			uow.Save(new MeasurementUnits {
				Name = "пара",
				OKEI = "715"
			});
			uow.Commit();
		}
	}
}
