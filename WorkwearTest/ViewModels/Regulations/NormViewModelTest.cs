using System;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Testing.DB;
using QS.Tools;
using QS.Validation;
using QS.Validation.Testing;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;
using Workwear.Tools;
using Workwear.ViewModels.Regulations;

namespace WorkwearTest.ViewModels.Regulations {
	
	[TestFixture(TestOf = typeof(NormViewModel))]
	public class NormViewModelTest : InMemoryDBGlobalConfigTestFixtureBase {
		
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			NotifyConfiguration.Enable();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что можем произвести пересчет выданного с последовательным перемещением даты начала выдачи." +
		                    "То есть по ошибке выдавали несколько месяцев подряд по 6 перчаток в месяц, но оказалось что по норме надо 6 на год." +
		                    "По факту выдали на несколько лет вперед. Надо чтобы программа умела пересчитать такую ситуацию.")]
		[Category("Integrated")]
		[Category("Real case")]
		public void ReSaveLastIssue_SequentialRecalculationCase()
		{
			NewSessionWithSameDB();

			var navigation = Substitute.For<INavigationManager>();
			var progressCreator = Substitute.For<ModalProgressCreator>(navigation);
			var progressPage = Substitute.For<IPage<ProgressWindowViewModel>>();
			var progressViewModel = new ProgressWindowViewModel(navigation) {
				Progress = Substitute.For<IProgressBarDisplayable>()
			};
			progressPage.ViewModel.Returns(progressViewModel);
			navigation.OpenViewModel<ProgressWindowViewModel>(null).Returns(progressPage);
			
			var validator = new ValidatorForTests();
			var monitor = Substitute.For<IChangeMonitor<NormItem>>();
			var interactive = Substitute.For<IInteractiveService>();
			var baseParameters = Substitute.For<BaseParameters>();
			baseParameters.ExtendPeriod.Returns(AnswerOptions.Yes);
			baseParameters.ShiftExpluatacion.Returns(AnswerOptions.Yes);

			var builder = new ContainerBuilder();
			builder.RegisterType<NormViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<EmployeeRepository>().AsSelf();
			builder.RegisterType<EmployeeIssueModel>().AsSelf();
			builder.Register(x => monitor).As<IChangeMonitor<NormItem>>();
			builder.Register(x => UnitOfWorkFactory).As<IUnitOfWorkFactory>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => interactive).As<IInteractiveService>();
			builder.Register(x => validator).As<IValidator>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => progressCreator).As<ModalProgressCreator>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var itemType = new ItemsType {
					Name = "Тип перчатки"
				};
				uow.Save(itemType);
				
				var nomenclature = new Nomenclature {
					Type = itemType
				};
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools {
					Name = "Перчатки",
					Type = itemType
				};
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				norm.DateFrom = new DateTime(2022, 1, 11);
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 6;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				// Создаем последовательность неправильных выдач
				var issuedOperation1 = new EmployeeIssueOperation {
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					NormItem = normItem,
					OperationTime = new DateTime(2022, 11, 11),
					StartOfUse = new DateTime(2022, 11, 11),
					UseAutoWriteoff = true,
					AutoWriteoffDate = new DateTime(2022, 12, 11),
					ExpiryByNorm = new DateTime(2022, 12, 11),
					Issued = 6
				};
				uow.Save(issuedOperation1);
				
				var issuedOperation2 = new EmployeeIssueOperation {
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					NormItem = normItem,
					OperationTime = new DateTime(2022, 12, 13),
					StartOfUse = new DateTime(2022, 12, 13),
					UseAutoWriteoff = true,
					AutoWriteoffDate = new DateTime(2023, 01, 13),
					ExpiryByNorm = new DateTime(2023, 01, 13),
					Issued = 6
				};
				uow.Save(issuedOperation2);
				
				var issuedOperation3 = new EmployeeIssueOperation {
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					NormItem = normItem,
					OperationTime = new DateTime(2023, 1, 17),
					StartOfUse = new DateTime(2023, 1, 17),
					UseAutoWriteoff = true,
					AutoWriteoffDate = new DateTime(2023, 02, 17),
					ExpiryByNorm = new DateTime(2023, 02, 17),
					Issued = 6
				};
				uow.Save(issuedOperation3);
				uow.Commit();
				
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
				employee.UpdateNextIssueAll();
				
				Assert.That(employee.WorkwearItems.First().NextIssue, Is.EqualTo(new DateTime(2023, 2, 17)));
				
				//Меняем норму
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 12;
				uow.Save(normItem);
				uow.Commit();
				
				//Открываем норму и запускаем пересчет.
				using (var vmNorm = container.Resolve<NormViewModel>(new [] {
					       new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(norm.Id))
					       }))
				{
					vmNorm.ReSaveLastIssue(vmNorm.Entity.Items.First());
				}

				using(var uow2 = UnitOfWorkFactory.CreateWithoutRoot()) {
					var employee2 = uow2.GetById<EmployeeCard>(employee.Id);
					Assert.That(employee2.WorkwearItems.First().NextIssue, Is.EqualTo(new DateTime(2025, 11, 11)));
				}
			}
		}
	}
}
