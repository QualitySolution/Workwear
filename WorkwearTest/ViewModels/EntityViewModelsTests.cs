using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using QS.ViewModels;
using Workwear.ViewModels.Company;

namespace WorkwearTest.ViewModels
{
	[TestFixture]
	[Category("Проверка ViewModel-ей")]
	public class EntityViewModelsTests : EntityViewModelsTestsBase
	{
		static EntityViewModelsTests()
		{
			ScanAssemblies = new[] { Assembly.GetAssembly(typeof(OrganizationViewModel))};
		}

		public new static IEnumerable AllEntityViewModels => EntityViewModelsTestsBase.AllEntityViewModels;

		[Test, TestCaseSource(nameof(AllEntityViewModels))]
		public override void ViewModelForValidatableEntityHasValidatorDependenceTest(Type type)
		{
			base.ViewModelForValidatableEntityHasValidatorDependenceTest(type);
		}
		
		[Test, TestCaseSource(nameof(AllEntityViewModels))]
		public override void ViewModelForHistoryLogEntityHasNamesTest(Type type)
		{
			base.ViewModelForHistoryLogEntityHasNamesTest(type);
		}
	}
}
