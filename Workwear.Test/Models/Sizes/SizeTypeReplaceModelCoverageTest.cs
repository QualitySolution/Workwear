using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QS.Project.DB;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Supply;
using Workwear.Models.Sizes;

namespace Workwear.Test.Models.Sizes {
	/// <summary>
	/// Тест следит за тем, чтобы все сущности с полями WearSize и Height, ссылающимися на Size,
	/// были охвачены в <see cref="SizeTypeReplaceModel"/>.
	///
	/// Если вы добавили новую сущность с полем WearSize или Height (ссылка на Size)
	/// и этот тест упал — выполните все три шага:
	/// <list type="number">
	///   <item>
	///     Добавьте обработку новой сущности в
	///     <see cref="SizeTypeReplaceModel.ReplaceSize"/> и/или
	///     <c>SizeTypeReplaceModel.ReplaceHeight</c> (файл
	///     <c>Workwear.Desktop/Models/Sizes/SizeTypeReplaceModel.cs</c>).
	///   </item>
	///   <item>
	///     Добавьте тип в <see cref="HandledTypes"/> данного теста, чтобы тест снова стал зелёным.
	///     Либо — если замена намеренно не нужна — добавьте его в <see cref="ExcludedTypes"/>
	///     с кратким обоснованием.
	///   </item>
	///   <item>
	///     Добавьте новую сущность в интеграционный тест
	///     <c>Workwear.Test/Integration/Sizes/SizeTypeReplaceModelIntegrationTest.cs</c>:
	///     создайте экземпляр с <c>oldSize</c>, сохраните его, а после вызова
	///     <c>TryReplaceSizes()</c> проверьте, что <c>WearSize</c>/<c>Height</c>
	///     изменились на <c>newSize</c>.
	///   </item>
	///   <item>
	///     <b>Обязательно</b> выполните аналогичный шаг для модуля
	///     <see cref="Workwear.Models.Sizes.StockPositionSizeReplaceModel"/>
	///     (файл <c>Workwear.Desktop/Models/Sizes/StockPositionSizeReplaceModel.cs</c>)
	///     и обновите список <c>HandledTypes</c> в тесте
	///     <c>StockPositionSizeReplaceModelCoverageTest</c>.
	///   </item>
	/// </list>
	/// </summary>
	[TestFixture]
	[Category("Конфигурация")]
	public class SizeTypeReplaceModelCoverageTest {
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
		}

		/// <summary>
		/// Сущности, для которых замена размеров намеренно не производится.
		/// </summary>
		private static readonly Dictionary<Type, string> ExcludedTypes = new Dictionary<Type, string> {
			{ typeof(Barcode),           "Размер фиксируется в момент выпуска штрихкода и не должен меняться после." },
			{ typeof(NomenclatureSizes), "Справочная таблица допустимых размеров номенклатуры, не оперативные данные." },
		};

		/// <summary>
		/// Сущности, которые обрабатываются в <see cref="SizeTypeReplaceModel"/>.
		/// При добавлении новой сущности добавьте её сюда, в <c>SizeTypeReplaceModel</c>
		/// и в интеграционный тест <c>SizeTypeReplaceModelIntegrationTest</c>.
		/// </summary>
		private static readonly HashSet<Type> HandledTypes = new HashSet<Type> {
			typeof(WarehouseOperation),
			typeof(EmployeeIssueOperation),
			typeof(ExpenseItem),
			typeof(CollectiveExpenseItem),
			typeof(IncomeItem),
			typeof(IssuanceSheetItem),
			typeof(WriteoffItem),
			typeof(DutyNormIssueOperation),
			typeof(ReturnItem),
			typeof(ShipmentItem),
		};

		[Test(Description = "Все NH-сущности с полем WearSize (→ Size) охвачены в SizeTypeReplaceModel или явно исключены.")]
		public void AllEntitiesWithWearSizeAreCoveredTest() {
			var notCovered = GetMappedClassesWithSizeProperty("WearSize")
				.Where(t => !ExcludedTypes.ContainsKey(t) && !HandledTypes.Contains(t))
				.ToList();

			Assert.That(notCovered, Is.Empty,
				"Следующие сущности имеют NH-маппинг поля WearSize → Size, " +
				"но не обрабатываются в SizeTypeReplaceModel и не добавлены в список исключений: " +
				string.Join(", ", notCovered.Select(t => t.Name)) +
				"\nДобавьте их в SizeTypeReplaceModel.ReplaceSize() и в HandledTypes теста, " +
				"либо в ExcludedTypes с обоснованием.");
		}

		[Test(Description = "Все NH-сущности с полем Height (→ Size) охвачены в SizeTypeReplaceModel или явно исключены.")]
		public void AllEntitiesWithHeightAreCoveredTest() {
			var notCovered = GetMappedClassesWithSizeProperty("Height")
				.Where(t => !ExcludedTypes.ContainsKey(t) && !HandledTypes.Contains(t))
				.ToList();

			Assert.That(notCovered, Is.Empty,
				"Следующие сущности имеют NH-маппинг поля Height → Size, " +
				"но не обрабатываются в SizeTypeReplaceModel и не добавлены в список исключений: " +
				string.Join(", ", notCovered.Select(t => t.Name)) +
				"\nДобавьте их в SizeTypeReplaceModel.ReplaceHeight() и в HandledTypes теста, " +
				"либо в ExcludedTypes с обоснованием.");
		}

		[Test(Description = "HandledTypes не содержат типов, у которых нет NH-маппинга WearSize или Height → Size (защита от устаревших записей).")]
		public void HandledTypesAreActuallyMappedWithSizeTest() {
			var mappedWithWearSize = GetMappedClassesWithSizeProperty("WearSize");
			var mappedWithHeight   = GetMappedClassesWithSizeProperty("Height");
			var allMapped = mappedWithWearSize.Union(mappedWithHeight).ToHashSet();

			var orphaned = HandledTypes.Where(t => !allMapped.Contains(t)).ToList();

			Assert.That(orphaned, Is.Empty,
				"Следующие типы перечислены в HandledTypes, но не имеют NH-маппинга WearSize/Height → Size. " +
				"Возможно, маппинг был удалён или переименован: " +
				string.Join(", ", orphaned.Select(t => t.Name)));
		}

		// ─── helpers ────────────────────────────────────────────────────────────

		private static IEnumerable<Type> GetMappedClassesWithSizeProperty(string propertyName) {
			return OrmConfig.NhConfig.ClassMappings
				.Where(m => m.PropertyIterator.Any(p =>
					p.Name == propertyName &&
					p.IsEntityRelation &&
					p.Type.ReturnedClass == typeof(Size)))
				.Select(m => m.MappedClass);
		}
	}
}

