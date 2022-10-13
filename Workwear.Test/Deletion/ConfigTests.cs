using System.Collections;
using NHibernate.Mapping;
using NUnit.Framework;
using QS.Deletion.Configuration;
using QS.Deletion.Testing;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;

namespace Workwear.Test.Deletion
{
	[TestFixture]
	[Category("Конфигурация удаления")]
	public class ConfigTests : DeleteConfigTestBase
	{
		static ConfigTests()
		{
			ConfigureOneTime.ConfigureNh();
			ConfigureOneTime.ConfigureDeletion();

			AddIgnoredProperty<EmployeeIssueOperation>(x => x.IssuedOperation, "Потому что если мы удаляем операцию списания, мы не должны при этом удалять операцию выдачи.");
			AddIgnoredProperty<EmployeeIssueOperation>(x => x.WarehouseOperation, "Является лиш дополнительной ссылкой на операцию. И скорей всего и так вместе будет удалятся за счет других ссылок.");
			AddIgnoredProperty<SubdivisionIssueOperation>(x => x.IssuedOperation, "Потому что если мы удаляем операцию списания, мы не должны при этом удалять операцию выдачи.");
			AddIgnoredProperty<SubdivisionIssueOperation>(x => x.WarehouseOperation, "Является лиш дополнительной ссылкой на операцию. И скорей всего и так вместе будет удалятся за счет других ссылок.");
			AddIgnoredProperty<BarcodeOperation>(x => x.EmployeeIssueOperation, "Является дочерней частью операции при удалении не должна тянуть за собой операцию.");
			AddIgnoredProperty<BarcodeOperation>(x => x.WarehouseOperation, "Является дочерней частью операции при удалении не должна тянуть за собой операцию.");
			AddIgnoredProperty<EmployeeCardItem>(x => x.ActiveNormItem, "Должно удалятся более сложным способом, а именно через обновление потребностей.");
			AddIgnoredProperty<IssuanceSheetItem>(x => x.IssueOperation, "Является дополнительной ссылкой на операцию, а не основной, поэтому не должно удалять операцию.");
			
			AddIgnoredCollection<ProtectionTools>(x => x.Nomenclatures, "Коллекция многие к многим, связи удаляются на уровне БД. Не много смысла их показывать пользователю.");
			AddIgnoredCollection<ProtectionTools>(x => x.Analogs, "Коллекция многие к многим, связи удаляются на уровне БД. Не много смысла их показывать пользователю.");
			AddIgnoredCollection<Nomenclature>(x => x.ProtectionTools, "Коллекция многие к многим, связи удаляются на уровне БД. Не много смысла их показывать пользователю.");
			AddIgnoredCollection<Size>(x => x.SuitableSizes, "Коллекция многие к многим, связи удаляются на уровне БД. Не много смысла их показывать пользователю.");
			AddIgnoredCollection<Size>(x => x.SizesWhereIsThisSizeAsSuitable, "Коллекция многие к многим, связи удаляются на уровне БД. Не много смысла их показывать пользователю.");
		}

		public new static IEnumerable AllDeleteItems => DeleteConfigTestBase.AllDeleteItems;

		[Test, TestCaseSource(nameof(AllDeleteItems))]
		public override void DeleteItemsTypesTest(IDeleteRule info, DeleteDependenceInfo dependence)
		{
			base.DeleteItemsTypesTest(info, dependence);
		}

		public new static IEnumerable AllClearItems => DeleteConfigTestBase.AllClearItems;

		[Test, TestCaseSource(nameof(AllClearItems))]
		public override void ClearItemsTypesTest(IDeleteRule info, ClearDependenceInfo dependence)
		{
			base.ClearItemsTypesTest(info, dependence);
		}

		public new static IEnumerable NhibernateMappedClasses => DeleteConfigTestBase.NhibernateMappedClasses;

		[Test, TestCaseSource(nameof(NhibernateMappedClasses))]
		public override void DeleteRuleExistForNhMappedClassTest(NHibernate.Mapping.PersistentClass mapping)
		{
			base.DeleteRuleExistForNhMappedClassTest(mapping);
		}

		public new static IEnumerable NhibernateMappedEntityRelation => DeleteConfigTestBase.NhibernateMappedEntityRelation;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelation))]
		public override void DeleteRuleExistForNhMappedEntityRelationTest(PersistentClass mapping, Property property)
		{
			base.DeleteRuleExistForNhMappedEntityRelationTest(mapping, property);
		}

		public new static IEnumerable NhibernateMappedEntityRelationWithExistRule => DeleteConfigTestBase.NhibernateMappedEntityRelationWithExistRule;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelationWithExistRule))]
		public override void DependenceRuleExistForNhMappedEntityRelationTest(PersistentClass mapping, Property property, IDeleteRule related)
		{
			base.DependenceRuleExistForNhMappedEntityRelationTest(mapping, property, related);
		}

		public new static IEnumerable NhibernateMappedEntityRelationWithExistRuleCascadeRelated => DeleteConfigTestBase.NhibernateMappedEntityRelationWithExistRuleCascadeRelated;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelationWithExistRuleCascadeRelated))]
		public override void CascadeDependenceRuleExistForNhMappedEntityRelationTest(PersistentClass mapping, Property property, IDeleteRule related)
		{
			base.CascadeDependenceRuleExistForNhMappedEntityRelationTest(mapping, property, related);
		}

		public new static IEnumerable NhibernateMappedCollection => DeleteConfigTestBase.NhibernateMappedCollection;

		[Test, TestCaseSource(nameof(NhibernateMappedCollection))]
		public override void NHMappedCollectionsAllInOneTest(PersistentClass mapping, Property property)
		{
			base.NHMappedCollectionsAllInOneTest(mapping, property);
		}

		#region Оформление заголовков

		public new static IEnumerable AllDeleteRules => DeleteConfigTestBase.AllDeleteRules;

		[Test, TestCaseSource(nameof(AllDeleteRules))]
		public override void DeleteRules_ExistTitle_Test(IDeleteRule info)
		{
			base.DeleteRules_ExistTitle_Test(info);
		}

		[Test, TestCaseSource(nameof(AllDeleteRules))]
		public override void DeleteRules_ExistAppellativeAttribute_Test(IDeleteRule info)
		{
			base.DeleteRules_ExistAppellativeAttribute_Test(info);
		}

		#endregion
	}
}
