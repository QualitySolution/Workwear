using System;
using System.Collections.Generic;
using NHibernate;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Repository.Operations;
using Workwear.Tools;

namespace WorkwearTest.Organization
{
	[TestFixture(TestOf = typeof(EmployeeVacation))]
	public class EmployeeVacationTests
	{
		[Test(Description = "Проверяем что при изменении даты отпуска срок операции пересчитывается")]
		public void UpdateRelatedOperations_ChangeVacationRecalculateOperations()
		{
			var uow = Substitute.For<IUnitOfWork>();

			var employee = new EmployeeCard();

			var vacationType = Substitute.For<VacationType>();
			vacationType.ExcludeFromWearing.Returns(true);

			var vacation = new EmployeeVacation();
			vacation.Employee = employee;
			vacation.BeginDate = new DateTime(2019, 3, 1);
			vacation.EndDate = new DateTime(2019, 3, 10);
			vacation.VacationType = vacationType;

			employee.Vacations.Add(vacation);

			var norm = new NormItem();
			norm.Amount = 1;
			norm.PeriodCount = 3;
			norm.NormPeriod = NormPeriodType.Month;

			var nomenclatureType = Substitute.For<ItemsType>();
			var protectionTools = Substitute.For<ProtectionTools>();

			var nomenclature = Substitute.For<Nomenclature>();
			nomenclature.TypeName.Returns("fake");
			nomenclature.Type.Returns(nomenclatureType);

			var issue = new EmployeeIssueOperation();
			issue.UseAutoWriteoff = true;
			issue.Employee = employee;
			issue.Nomenclature = nomenclature;
			issue.ProtectionTools = protectionTools;
			issue.NormItem = norm;
			issue.OperationTime = new DateTime(2019, 2, 10);
			issue.StartOfUse = new DateTime(2019, 2, 10);
			issue.ExpiryByNorm = new DateTime(2019, 5, 10);
			issue.Issued = 1;

			var operations = new List<EmployeeIssueOperation>() { issue };
			IssueGraph.MakeIssueGraphTestGap = (e, t) => new IssueGraph(operations);

			var employeeIssueRepository = Substitute.For<EmployeeIssueRepository>(uow);
			employeeIssueRepository.GetOperationsTouchDates(Arg.Any<IUnitOfWork>(), Arg.Any<EmployeeCard[]>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>>>())
				.Returns(operations);

			var ask = Substitute.For<IInteractiveQuestion>();
			ask.Question(string.Empty).ReturnsForAnyArgs(false);

			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ColDayAheadOfShedule.Returns(0);

			vacation.UpdateRelatedOperations(uow, employeeIssueRepository, baseParameters, ask);

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
