using System;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Testing.DB;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Supply;
using Workwear.Models.Sizes;
using Workwear.Repository.Sizes;

namespace Workwear.Test.Integration.Sizes {
	/// <summary>
	/// Интеграционный тест: создаёт по одной записи каждого типа сущности с "старым" размером,
	/// запускает SizeTypeReplaceModel и проверяет, что после замены все записи ссылаются
	/// на "новый" размер. Если добавлена новая сущность с WearSize/Height и она НЕ заменяется —
	/// этот тест это не поймает, но покрывающий тест SizeTypeReplaceModelCoverageTest поймает.
	/// </summary>
	[TestFixture]
	[Category("Integrated")]
	public class SizeTypeReplaceModelIntegrationTest : InMemoryDBGlobalConfigTestFixtureBase {
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "TryReplaceSizes обновляет WearSize во всех зарегистрированных типах сущностей")]
		public void TryReplaceSizes_ReplacesWearSizeInAllHandledEntities() {
			var interactive = Substitute.For<IInteractiveService>();
			interactive.Question(Arg.Any<string>()).Returns(true);
			var progress = Substitute.For<IProgressBarDisplayable>();

			using var uow = UnitOfWorkFactory.CreateWithoutRoot();

			// ── Общие объекты ────────────────────────────────────────────────
			var warehouse = new Warehouse();
			uow.Save(warehouse);

			var oldSizeType = new SizeType { CategorySizeType = CategorySizeType.Size };
			var newSizeType = new SizeType { CategorySizeType = CategorySizeType.Size };
			uow.Save(oldSizeType);
			uow.Save(newSizeType);

			// Одинаковые имена — условие для успешной замены
			var oldSize = new Size { Name = "L", SizeType = oldSizeType };
			var newSize = new Size { Name = "L", SizeType = newSizeType };
			uow.Save(oldSize);
			uow.Save(newSize);

			// Коллекция Sizes — Inverse/LazyLoad, NH не заполняет её автоматически
			// в новых объектах; заполняем вручную, чтобы CheckCanReplace работал корректно
			oldSizeType.Sizes = new System.Collections.Generic.List<Size> { oldSize };
			newSizeType.Sizes = new System.Collections.Generic.List<Size> { newSize };

			var nomenclatureType = new ItemsType { Name = "Тестовый тип", SizeType = oldSizeType };
			uow.Save(nomenclatureType);

			var nomenclature = new Nomenclature { Type = nomenclatureType };
			uow.Save(nomenclature);

			var employee = new EmployeeCard();
			uow.Save(employee);

			var issuanceSheet = new IssuanceSheet();
			uow.Save(issuanceSheet);

			// ── WarehouseOperation ───────────────────────────────────────────
			// Обязательно нужна хотя бы одна с WearSize, чтобы GetUsedSizes вернул oldSize
			var wareOp = new WarehouseOperation { Nomenclature = nomenclature, WearSize = oldSize };
			uow.Save(wareOp);

			// ── EmployeeIssueOperation ───────────────────────────────────────
			var empOp = new EmployeeIssueOperation { Employee = employee, Nomenclature = nomenclature, WearSize = oldSize };
			uow.Save(empOp);

			// ── ExpenseItem (нужны Expense + WarehouseOperation) ─────────────
			var expense = new Expense { Warehouse = warehouse };
			var expWareOp = new WarehouseOperation { Nomenclature = nomenclature };
			uow.Save(expense);
			uow.Save(expWareOp);
			var expenseItem = new ExpenseItem {
				ExpenseDoc = expense,
				Nomenclature = nomenclature,
				WarehouseOperation = expWareOp,
				WearSize = oldSize,
			};
			uow.Save(expenseItem);

			// ── CollectiveExpenseItem (нужны CollectiveExpense + WarehouseOperation) ─
			var collectiveExpense = new CollectiveExpense { Warehouse = warehouse };
			var colExpWareOp = new WarehouseOperation { Nomenclature = nomenclature };
			uow.Save(collectiveExpense);
			uow.Save(colExpWareOp);
			var collectiveExpenseItem = new CollectiveExpenseItem {
				Document = collectiveExpense,
				Nomenclature = nomenclature,
				WarehouseOperation = colExpWareOp,
				WearSize = oldSize,
			};
			uow.Save(collectiveExpenseItem);

			// ── IncomeItem (protected ctor; поле warehouseOperation инициализировано автоматически) ─
			var incomeItem = CreateViaReflection<IncomeItem>();
			incomeItem.Nomenclature = nomenclature;
			incomeItem.WearSize = oldSize;
			incomeItem.WarehouseOperation.Nomenclature = nomenclature; // NOT NULL в маппинге
			uow.Save(incomeItem.WarehouseOperation);
			uow.Save(incomeItem);

			// ── IssuanceSheetItem (нужны IssuanceSheet + Employee) ───────────
			var issuanceSheetItem = new IssuanceSheetItem {
				IssuanceSheet = issuanceSheet,
				Employee = employee,
				Nomenclature = nomenclature,
				WearSize = oldSize,
			};
			uow.Save(issuanceSheetItem);

			// ── WriteoffItem (protected ctor; WarehouseOperation необязателен) ─
			var writeoffItem = CreateViaReflection<WriteoffItem>();
			writeoffItem.Nomenclature = nomenclature;
			writeoffItem.WearSize = oldSize;
			uow.Save(writeoffItem);

			// ── DutyNormIssueOperation ───────────────────────────────────────
			var dutyNormOp = new DutyNormIssueOperation { Nomenclature = nomenclature, WearSize = oldSize };
			uow.Save(dutyNormOp);

			// ── ReturnItem (protected ctor; поле warehouseOperation инициализировано автоматически) ─
			var returnDoc = new Return { Warehouse = warehouse };
			uow.Save(returnDoc);
			var returnItem = CreateViaReflection<ReturnItem>();
			returnItem.Document = returnDoc;
			returnItem.Nomenclature = nomenclature;
			returnItem.WearSize = oldSize;
			returnItem.WarehouseOperation.Nomenclature = nomenclature; // NOT NULL + Cascade
			uow.Save(returnItem.WarehouseOperation);
			uow.Save(returnItem);

			// ── ShipmentItem ─────────────────────────────────────────────────
			var shipment = new Shipment();
			uow.Save(shipment);
			var shipmentItem = new ShipmentItem {
				Shipment = shipment,
				Nomenclature = nomenclature,
				WearSize = oldSize,
			};
			uow.Save(shipmentItem);

			uow.Commit();

			// Сбрасываем сессионный кэш перед проверкой корректности данных в БД
			uow.Session.Clear();

			// ── Проверяем что все сущности корректно сохранены в БД ───────────
			Assert.That(uow.GetById<ShipmentItem>(shipmentItem.Id), Is.Not.Null,
				"ShipmentItem не найден в БД — проверьте сохранение");
			Assert.That(uow.GetById<ShipmentItem>(shipmentItem.Id).WearSize, Is.Not.Null,
				"ShipmentItem.WearSize не сохранён в БД");
			Assert.That(uow.GetById<ShipmentItem>(shipmentItem.Id).Nomenclature, Is.Not.Null,
				"ShipmentItem.Nomenclature не сохранён в БД — фильтр по номенклатуре не сработает");

			// Снова очищаем кэш перед заменой (newSizeType.Sizes — in-memory List, не NH-прокси, сохраняется)
			uow.Session.Clear();

			// ── Замена типа размера ───────────────────────────────────────────
			var model = new SizeTypeReplaceModel(new SizeRepository());
			bool result = model.TryReplaceSizes(uow, interactive, progress,
				new[] { nomenclature }, newSizeType, null);

			Assert.That(result, Is.True, "TryReplaceSizes вернул false — проверьте настройку теста");

			// Сбрасываем кэш NH, чтобы получить актуальные данные из БД
			uow.Session.Flush();
			uow.Session.Clear();

			// ── Проверки ─────────────────────────────────────────────────────
			Assert.That(uow.GetById<WarehouseOperation>(wareOp.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "WarehouseOperation.WearSize не обновлён");

			Assert.That(uow.GetById<EmployeeIssueOperation>(empOp.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "EmployeeIssueOperation.WearSize не обновлён");

			Assert.That(uow.GetById<ExpenseItem>(expenseItem.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "ExpenseItem.WearSize не обновлён");

			Assert.That(uow.GetById<CollectiveExpenseItem>(collectiveExpenseItem.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "CollectiveExpenseItem.WearSize не обновлён");

			Assert.That(uow.GetById<IncomeItem>(incomeItem.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "IncomeItem.WearSize не обновлён");

			Assert.That(uow.GetById<IssuanceSheetItem>(issuanceSheetItem.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "IssuanceSheetItem.WearSize не обновлён");

			Assert.That(uow.GetById<WriteoffItem>(writeoffItem.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "WriteoffItem.WearSize не обновлён");

			Assert.That(uow.GetById<DutyNormIssueOperation>(dutyNormOp.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "DutyNormIssueOperation.WearSize не обновлён");

			Assert.That(uow.GetById<ReturnItem>(returnItem.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "ReturnItem.WearSize не обновлён");

			Assert.That(uow.GetById<ShipmentItem>(shipmentItem.Id).WearSize.Id,
				Is.EqualTo(newSize.Id), "ShipmentItem.WearSize не обновлён");
		}

		// ── Вспомогательный метод ─────────────────────────────────────────────

		/// <summary>
		/// Создаёт экземпляр сущности с protected-конструктором (для NHibernate).
		/// </summary>
		private static T CreateViaReflection<T>() where T : class =>
			(T)Activator.CreateInstance(typeof(T), nonPublic: true);
	}
}



