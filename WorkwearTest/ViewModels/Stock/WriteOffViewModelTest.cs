using System;
using System.Linq;
using System.Threading;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Testing.DB;
using QS.Testing.Testing.Navigation;
using QS.Validation;
using QS.Validation.Testing;
using QS.ViewModels.Resolve;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.Journal.ViewModels.Company;
using Workwear.Models.Operations;
using Workwear.Repository.Operations;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock;

namespace WorkwearTest.ViewModels.Stock
{
	[TestFixture(TestOf = typeof(WriteOffViewModel))]
	public class WriteOffViewModelTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			NotifyConfiguration.Enable();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что при списании обновляем дату следующей выдачи в карточке сотрудника.")]
		[Timeout(2000)] //Так как ожидаем работу в потоке, и может что-то поломаться.
		[Category("Integrated")]
		public void Employee_UpdateNextIssueDate()
		{
			NewSessionWithSameDB();

			var validator = new ValidatorForTests();
			var userService = Substitute.For<IUserService>();

			var builder = new ContainerBuilder();
			builder.RegisterType<WriteOffViewModel>().AsSelf();
			builder.RegisterType<EmployeeBalanceJournalViewModel>().AsSelf();
			builder.RegisterType<EmployeeBalanceFilterViewModel>().AsSelf();
			builder.RegisterType<UnitOfWorkProvider>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<EmployeeIssueModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<NavigationManagerForTests>().AsSelf().As<INavigationManager>().SingleInstance();
			builder.Register(x => Substitute.For<FeaturesService>()).As<FeaturesService>();
			builder.Register(x => validator).As<IValidator>();
			builder.Register(x => Substitute.For<IViewModelResolver>()).As<IViewModelResolver>();
			builder.Register(x => Substitute.For<SizeService>()).As<SizeService>();
			builder.Register(x => Substitute.For<IDeleteEntityService>()).As<IDeleteEntityService>();
			builder.Register(x => Substitute.For<IInteractiveService>()).As<IInteractiveService>().As<IInteractiveQuestion>().As<IInteractiveMessage>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			builder.Register(x => UnitOfWorkFactory).As<IUnitOfWorkFactory>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				
				var user = new UserBase();
				uow.Save(user);
				userService.GetCurrentUser().Returns(user);

				var itemType = new ItemsType {
					Name = "Тип"
				};
				uow.Save(itemType);
				
				var nomenclature = new Nomenclature {
					Type = itemType
				};
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools {
					Name = "Тестовый СИЗ",
					Type = itemType
				};
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.FirstName = "Вася";
				employee.AddUsedNorm(norm);
				Assert.That(employee.WorkwearItems, Has.Count.EqualTo(1));
				employee.WorkwearItems.First().Created = new DateTime(2023, 1, 1); //Чтобы дата следующей выдачи не привязывалась с дате запуска теста.
				employee.WorkwearItems.First().NextIssue = new DateTime(2023, 1, 1); //Чтобы дата следующей выдачи не привязывалась с дате запуска теста.
				uow.Save(employee);

				var issueOperation = new EmployeeIssueOperation {
					Employee = employee,
					OperationTime = new DateTime(2023, 1, 1),
					StartOfUse = new DateTime(2023, 1, 1),
					UseAutoWriteoff = true,
					AutoWriteoffDate = null, // new DateTime(2023, 2, 1),
					ExpiryByNorm = new DateTime(2023, 2, 1),
					Issued = 1,
					ProtectionTools = protectionTools,
					NormItem = normItem,
					Nomenclature = nomenclature,
				};
				uow.Save(issueOperation);
				
				uow.Commit();
				
				var navigation = container.Resolve<NavigationManagerForTests>();
				var page =
					navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder, EmployeeCard>(null, EntityUoWBuilder.ForCreate(), employee);
				var vmWriteoff = page.ViewModel;

				vmWriteoff.Entity.Date = new DateTime(2023, 1, 15);
				vmWriteoff.AddFromEmployee();

				var selectJournal = (EmployeeBalanceJournalViewModel)navigation.CurrentPage.ViewModel;
				Console.WriteLine("Ждем поток журнала");
				while(selectJournal.DataLoader.LoadInProgress) {
					Console.Write('⧖');
					Thread.Sleep(10); //Ожидаем загрузки данных так как она происходит в отдельном потоке.
				}
				Assert.That(selectJournal.Items, Has.Count.EqualTo(1));
				
				selectJournal.RowActivatedAction.ExecuteAction(selectJournal.Items.Cast<object>().ToArray());
				Assert.That(vmWriteoff.Entity.Items, Has.Count.EqualTo(1));

				vmWriteoff.Entity.Items.First().Amount = 1;
				vmWriteoff.Save();

				var item = vmWriteoff.Entity.Items.First();
				Assert.That(item.EmployeeWriteoffOperation.Employee.WorkwearItems.First().NextIssue, Is.EqualTo(new DateTime(2023, 1, 15)));
			}
		}
	}
}
