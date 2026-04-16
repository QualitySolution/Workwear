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

namespace Workwear.Test.Integration.Sizes {
	/// <summary>
	/// Интеграционный тест: создаёт по одной записи каждого типа сущности со «старым» размером/ростом,
	/// запускает <see cref="StockPositionSizeReplaceModel"/> и проверяет, что после замены все записи
	/// ссылаются на «новый» размер/рост.
	///
	/// Если добавлена новая сущность с WearSize/Height и она НЕ заменяется —
	/// этот тест НЕ поймает это; покрывающий тест <c>SizeTypeReplaceModelCoverageTest</c>
	/// (и его комментарий об обязательном обновлении <c>StockPositionSizeReplaceModel</c>) поймает.
	/// </summary>
	[TestFixture]
	[Category("Integrated")]
	public class StockPositionSizeReplaceModelIntegrationTest : InMemoryDBGlobalConfigTestFixtureBase {
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "ReplaceInStock обновляет WearSize во всех обрабатываемых типах сущностей")]
		public void ReplaceInStock_ReplacesWearSizeInAllHandledEntities() {
			var interactive = Substitute.For<IInteractiveService>();
			var progress    = Substitute.For<IProgressBarDisplayable>();

			using var uow = UnitOfWorkFactory.CreateWithoutRoot();

			// ── Общие объекты ────────────────────────────────────────────────
			var warehouse = new Warehouse();
			uow.Save(warehouse);

			var sizeType = new SizeType { CategorySizeType = CategorySizeType.Size };
			uow.Save(sizeType);

			var oldSize = new Size { Name = "L",  SizeType = sizeType };
			var newSize = new Size { Name = "XL", SizeType = sizeType };
			uow.Save(oldSize);
			uow.Save(newSize);

			var nomenclatureType = new ItemsType { Name = "Тестовый тип", SizeType = sizeType };
			uow.Save(nomenclatureType);

			var nomenclature = new Nomenclature { Type = nomenclatureType };
			uow.Save(nomenclature);

			// Другая номенклатура — её записи НЕ должны меняться
			var otherNomenclature = new Nomenclature { Type = nomenclatureType };
			uow.Save(otherNomenclature);

			var employee      = new EmployeeCard();
			uow.Save(employee);

			var issuanceSheet = new IssuanceSheet();
			uow.Save(issuanceSheet);

			// ── WarehouseOperation ───────────────────────────────────────────
			var wareOp = new WarehouseOperation { Nomenclature = nomenclature, WearSize = oldSize };
			uow.Save(wareOp);

			var otherWareOp = new WarehouseOperation { Nomenclature = otherNomenclature, WearSize = oldSize };
			uow.Save(otherWareOp);

			// ── EmployeeIssueOperation ───────────────────────────────────────
			var empOp = new EmployeeIssueOperation { Employee = employee, Nomenclature = nomenclature, WearSize = oldSize };
			uow.Save(empOp);

			// ── ExpenseItem ──────────────────────────────────────────────────
			var expense    = new Expense { Warehouse = warehouse };
			var expWareOp  = new WarehouseOperation { Nomenclature = nomenclature };
			uow.Save(expense);
			uow.Save(expWareOp);
			var expenseItem = new ExpenseItem {
				ExpenseDoc         = expense,
				Nomenclature       = nomenclature,
				WarehouseOperation = expWareOp,
				WearSize           = oldSize,
			};
			uow.Save(expenseItem);

			// ── CollectiveExpenseItem ────────────────────────────────────────
			var collectiveExpense    = new CollectiveExpense { Warehouse = warehouse };
			var colExpWareOp         = new WarehouseOperation { Nomenclature = nomenclature };
			uow.Save(collectiveExpense);
			uow.Save(colExpWareOp);
			var collectiveExpenseItem = new CollectiveExpenseItem {
				Document           = collectiveExpense,
				Nomenclature       = nomenclature,
				WarehouseOperation = colExpWareOp,
				WearSize           = oldSize,
			};
			uow.Save(collectiveExpenseItem);

			// ── IncomeItem (protected ctor) ──────────────────────────────────
			var incomeItem = CreateViaReflection<IncomeItem>();
			incomeItem.Nomenclature = nomenclature;
			incomeItem.WearSize     = oldSize;
			incomeItem.WarehouseOperation.Nomenclature = nomenclature;
			uow.Save(incomeItem.WarehouseOperation);
			uow.Save(incomeItem);

			// ── IssuanceSheetItem ────────────────────────────────────────────
			var issuanceSheetItem = new IssuanceSheetItem {
				IssuanceSheet = issuanceSheet,
				Employee      = employee,
				Nomenclature  = nomenclature,
				WearSize      = oldSize,
			};
			uow.Save(issuanceSheetItem);

			// ── WriteoffItem (protected ctor) ────────────────────────────────
			var writeoffItem = CreateViaReflection<WriteoffItem>();
			writeoffItem.Nomenclature = nomenclature;
			writeoffItem.WearSize     = oldSize;
			uow.Save(writeoffItem);

			// ── DutyNormIssueOperation ───────────────────────────────────────
			var dutyNormOp = new DutyNormIssueOperation { Nomenclature = nomenclature, WearSize = oldSize };
			uow.Save(dutyNormOp);

			// ── ReturnItem (protected ctor) ──────────────────────────────────
			var returnDoc  = new Return { Warehouse = warehouse };
			uow.Save(returnDoc);
			var returnItem = CreateViaReflection<ReturnItem>();
			returnItem.Document    = returnDoc;
			returnItem.Nomenclature = nomenclature;
			returnItem.WearSize    = oldSize;
			returnItem.WarehouseOperation.Nomenclature = nomenclature;
			uow.Save(returnItem.WarehouseOperation);
			uow.Save(returnItem);

			// ── ShipmentItem ─────────────────────────────────────────────────
			var shipment = new Shipment();
			uow.Save(shipment);
			var shipmentItem = new ShipmentItem {
				Shipment     = shipment,
				Nomenclature = nomenclature,
				WearSize     = oldSize,
			};
			uow.Save(shipmentItem);

			uow.Commit();
			uow.Session.Clear();

			// ── Замена ───────────────────────────────────────────────────────
			var model  = new StockPositionSizeReplaceModel();
			bool result = model.ReplaceInStock(uow, interactive, progress,
				nomenclature, oldSize, newSize, null, null);

			Assert.That(result, Is.True, "ReplaceInStock вернул false — проверьте настройку теста");

			uow.Session.Flush();
			uow.Session.Clear();

			// ── Проверки: целевая номенклатура обновлена ─────────────────────
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

			// ── Проверка: другая номенклатура НЕ тронута ─────────────────────
			Assert.That(uow.GetById<WarehouseOperation>(otherWareOp.Id).WearSize.Id,
				Is.EqualTo(oldSize.Id), "WarehouseOperation другой номенклатуры не должен был измениться");
		}

		[Test(Description = "ReplaceInStock обновляет Height во всех обрабатываемых типах сущностей")]
		public void ReplaceInStock_ReplacesHeightInAllHandledEntities() {
			var interactive = Substitute.For<IInteractiveService>();
			var progress    = Substitute.For<IProgressBarDisplayable>();

			using var uow = UnitOfWorkFactory.CreateWithoutRoot();

			// ── Общие объекты ────────────────────────────────────────────────
			var warehouse = new Warehouse();
			uow.Save(warehouse);

			var heightType = new SizeType { CategorySizeType = CategorySizeType.Height };
			uow.Save(heightType);

			var oldHeight = new Size { Name = "170", SizeType = heightType };
			var newHeight = new Size { Name = "176", SizeType = heightType };
			uow.Save(oldHeight);
			uow.Save(newHeight);

			var nomenclatureType = new ItemsType { Name = "Тестовый тип (рост)", HeightType = heightType };
			uow.Save(nomenclatureType);

			var nomenclature = new Nomenclature { Type = nomenclatureType };
			uow.Save(nomenclature);

			var employee      = new EmployeeCard();
			uow.Save(employee);

			var issuanceSheet = new IssuanceSheet();
			uow.Save(issuanceSheet);

			// ── WarehouseOperation ───────────────────────────────────────────
			var wareOp = new WarehouseOperation { Nomenclature = nomenclature, Height = oldHeight };
			uow.Save(wareOp);

			// ── EmployeeIssueOperation ───────────────────────────────────────
			var empOp = new EmployeeIssueOperation { Employee = employee, Nomenclature = nomenclature, Height = oldHeight };
			uow.Save(empOp);

			// ── ExpenseItem ──────────────────────────────────────────────────
			var expense   = new Expense { Warehouse = warehouse };
			var expWareOp = new WarehouseOperation { Nomenclature = nomenclature };
			uow.Save(expense);
			uow.Save(expWareOp);
			var expenseItem = new ExpenseItem {
				ExpenseDoc         = expense,
				Nomenclature       = nomenclature,
				WarehouseOperation = expWareOp,
				Height             = oldHeight,
			};
			uow.Save(expenseItem);

			// ── CollectiveExpenseItem ────────────────────────────────────────
			var collectiveExpense = new CollectiveExpense { Warehouse = warehouse };
			var colExpWareOp      = new WarehouseOperation { Nomenclature = nomenclature };
			uow.Save(collectiveExpense);
			uow.Save(colExpWareOp);
			var collectiveExpenseItem = new CollectiveExpenseItem {
				Document           = collectiveExpense,
				Nomenclature       = nomenclature,
				WarehouseOperation = colExpWareOp,
				Height             = oldHeight,
			};
			uow.Save(collectiveExpenseItem);

			// ── IncomeItem (protected ctor) ──────────────────────────────────
			var incomeItem = CreateViaReflection<IncomeItem>();
			incomeItem.Nomenclature = nomenclature;
			incomeItem.Height       = oldHeight;
			incomeItem.WarehouseOperation.Nomenclature = nomenclature;
			uow.Save(incomeItem.WarehouseOperation);
			uow.Save(incomeItem);

			// ── IssuanceSheetItem ────────────────────────────────────────────
			var issuanceSheetItem = new IssuanceSheetItem {
				IssuanceSheet = issuanceSheet,
				Employee      = employee,
				Nomenclature  = nomenclature,
				Height        = oldHeight,
			};
			uow.Save(issuanceSheetItem);

			// ── WriteoffItem (protected ctor) ────────────────────────────────
			var writeoffItem = CreateViaReflection<WriteoffItem>();
			writeoffItem.Nomenclature = nomenclature;
			writeoffItem.Height       = oldHeight;
			uow.Save(writeoffItem);

			// ── DutyNormIssueOperation ───────────────────────────────────────
			var dutyNormOp = new DutyNormIssueOperation { Nomenclature = nomenclature, Height = oldHeight };
			uow.Save(dutyNormOp);

			// ── ReturnItem (protected ctor) ──────────────────────────────────
			var returnDoc  = new Return { Warehouse = warehouse };
			uow.Save(returnDoc);
			var returnItem = CreateViaReflection<ReturnItem>();
			returnItem.Document     = returnDoc;
			returnItem.Nomenclature = nomenclature;
			returnItem.Height       = oldHeight;
			returnItem.WarehouseOperation.Nomenclature = nomenclature;
			uow.Save(returnItem.WarehouseOperation);
			uow.Save(returnItem);

			// ── ShipmentItem ─────────────────────────────────────────────────
			var shipment = new Shipment();
			uow.Save(shipment);
			var shipmentItem = new ShipmentItem {
				Shipment     = shipment,
				Nomenclature = nomenclature,
				Height       = oldHeight,
			};
			uow.Save(shipmentItem);

			uow.Commit();
			uow.Session.Clear();

			// ── Замена ───────────────────────────────────────────────────────
			var model  = new StockPositionSizeReplaceModel();
			bool result = model.ReplaceInStock(uow, interactive, progress,
				nomenclature, null, null, oldHeight, newHeight);

			Assert.That(result, Is.True, "ReplaceInStock вернул false — проверьте настройку теста");

			uow.Session.Flush();
			uow.Session.Clear();

			// ── Проверки ─────────────────────────────────────────────────────
			Assert.That(uow.GetById<WarehouseOperation>(wareOp.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "WarehouseOperation.Height не обновлён");

			Assert.That(uow.GetById<EmployeeIssueOperation>(empOp.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "EmployeeIssueOperation.Height не обновлён");

			Assert.That(uow.GetById<ExpenseItem>(expenseItem.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "ExpenseItem.Height не обновлён");

			Assert.That(uow.GetById<CollectiveExpenseItem>(collectiveExpenseItem.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "CollectiveExpenseItem.Height не обновлён");

			Assert.That(uow.GetById<IncomeItem>(incomeItem.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "IncomeItem.Height не обновлён");

			Assert.That(uow.GetById<IssuanceSheetItem>(issuanceSheetItem.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "IssuanceSheetItem.Height не обновлён");

			Assert.That(uow.GetById<WriteoffItem>(writeoffItem.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "WriteoffItem.Height не обновлён");

			Assert.That(uow.GetById<DutyNormIssueOperation>(dutyNormOp.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "DutyNormIssueOperation.Height не обновлён");

			Assert.That(uow.GetById<ReturnItem>(returnItem.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "ReturnItem.Height не обновлён");

			Assert.That(uow.GetById<ShipmentItem>(shipmentItem.Id).Height.Id,
				Is.EqualTo(newHeight.Id), "ShipmentItem.Height не обновлён");
		}

		// ── Вспомогательный метод ─────────────────────────────────────────────

		/// <summary>
		/// Создаёт экземпляр сущности с protected-конструктором (для NHibernate).
		/// </summary>
		private static T CreateViaReflection<T>() where T : class =>
			(T)Activator.CreateInstance(typeof(T), nonPublic: true);
	}
}

