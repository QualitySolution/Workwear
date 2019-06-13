using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using workwear.Domain.Operations;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Organization;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Repository.Operations;

namespace WorkwearTest.Organization
{
	[TestFixture(TestOf = typeof(EmployeeVacation))]
	public class EmployeeVacationTests
	{
		[Test(Description = "Проверяем что при изменении даты отпуска срок операции персчитывается")]
		public void UpdateRelatedOperations_ChangeVacationRecalculateOperations()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var employee = Substitute.For<EmployeeCard>();

			var vacationType = Substitute.For<VacationType>();
			vacationType.ExcludeFromWearing.Returns(true);

			var vacation = new EmployeeVacation();
			vacation.Employee = employee;
			vacation.BeginDate = new DateTime(2019, 3, 1);
			vacation.EndDate = new DateTime(2019, 3, 10);
			vacation.VacationType = vacationType;

			employee.Vacations.Returns(new List<EmployeeVacation> { vacation });

			var norm = new NormItem();
			norm.Amount = 1;
			norm.PeriodCount = 3;
			norm.NormPeriod = NormPeriodType.Month;

			var nomenclatureType = Substitute.For<ItemsType>();

			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.TypeName.Returns("fake");
			nomenclature.Type.Returns(nomenclatureType);

			var issue = new EmployeeIssueOperation();
			issue.UseAutoWriteoff = true;
			issue.Employee = employee;
			issue.Nomenclature = nomenclature;
			issue.NormItem = norm;
			issue.OperationTime = new DateTime(2019, 2, 10);
			issue.StartOfUse = new DateTime(2019, 2, 10);
			issue.ExpiryByNorm = new DateTime(2019, 5, 10);
			issue.Issued = 1;

			var operations = new List<EmployeeIssueOperation>() { issue };
			var graph = new IssueGraph(operations);

			IssueGraph.MakeIssueGraphTestGap = (e, t) => new IssueGraph(operations);

			EmployeeIssueRepository.GetOperationsTouchDatesTestGap = (arg1, arg2, arg3, arg4) => operations;

			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(false);
				
			vacation.UpdateRelatedOperations(uow, ask);

			Assert.That(issue.ExpiryByNorm, Is.EqualTo(new DateTime(2019, 5, 20)));
			Assert.That(issue.AutoWriteoffDate, Is.EqualTo(new DateTime(2019, 5, 20)));
		}

		[TearDown]
		public void RemoveStaticGaps()
		{
			IssueGraph.MakeIssueGraphTestGap = null;
		}
	}
}
