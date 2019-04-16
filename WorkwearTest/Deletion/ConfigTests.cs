using System.Collections;
using NHibernate.Mapping;
using NUnit.Framework;
using QS.Deletion;
using QS.Deletion.Testing;
using workwear.Domain.Operations;
using workwear.Domain.Organization;

namespace WorkwearTest.Deletion
{
	[TestFixture]
	[Category("Конфигурация удаления")]
	public class ConfigTests : DeleteConfigTestBase
	{
		static ConfigTests()
		{
			ConfigureOneTime.ConfigureNh();
			ConfigureOneTime.ConfogureDeletion();

			//Так как этот класс в общей библиотеке и пока никак не используется для удаления.
			//IgnoreMissingClass.Add(typeof(QS.Project.Domain.UserBase));
			AddIgnoredProperty<EmployeeIssueOperation>(x => x.IssuedOperation, "Потому что если мы удаляем операцию списания, мы не должны при этом удалять операцию выдачи.");
			AddIgnoredProperty<EmployeeCardItem>(x => x.ActiveNormItem, "Должно удалятся более сложным способом, а именно через обновление потребностей.");
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
		public override void DeleteRuleExisitForNHMappedClasssTest(NHibernate.Mapping.PersistentClass mapping)
		{
			base.DeleteRuleExisitForNHMappedClasssTest(mapping);
		}

		public new static IEnumerable NhibernateMappedEntityRelation => DeleteConfigTestBase.NhibernateMappedEntityRelation;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelation))]
		public override void DeleteRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property property)
		{
			base.DeleteRuleExisitForNHMappedEntityRelationTest(mapping, property);
		}

		public new static IEnumerable NhibernateMappedEntityRelationWithExistRule => DeleteConfigTestBase.NhibernateMappedEntityRelationWithExistRule;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelationWithExistRule))]
		public override void DependenceRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property property, IDeleteRule related)
		{
			base.DependenceRuleExisitForNHMappedEntityRelationTest(mapping, property, related);
		}

		public new static IEnumerable NhibernateMappedEntityRelationWithExistRuleCascadeRelated => DeleteConfigTestBase.NhibernateMappedEntityRelationWithExistRuleCascadeRelated;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelationWithExistRuleCascadeRelated))]
		public override void CascadeDependenceRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property property, IDeleteRule related)
		{
			base.CascadeDependenceRuleExisitForNHMappedEntityRelationTest(mapping, property, related);
		}

		public new static IEnumerable NhibernateMappedCollection => DeleteConfigTestBase.NhibernateMappedCollection;

		[Test, TestCaseSource(nameof(NhibernateMappedCollection))]
		public override void NHMappedCollectionsAllInOneTest(PersistentClass mapping, Property property)
		{
			base.NHMappedCollectionsAllInOneTest(mapping, property);
		}

		public new static IEnumerable AllDeleteRules => DeleteConfigTestBase.AllDeleteRules;

		[Test, TestCaseSource(nameof(AllDeleteRules))]
		public override void DeleteRules_ExistTitle_Test(IDeleteRule info)
		{
			base.DeleteRules_ExistTitle_Test(info);
		}
	}
}
