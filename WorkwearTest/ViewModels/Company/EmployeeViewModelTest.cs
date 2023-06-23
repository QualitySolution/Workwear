using System;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Cloud.WearLk.Client;
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
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Measurements;
using Workwear.Models.Company;
using workwear.Models.Stock;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;
using Workwear.Repository.Regulations;
using Workwear.Repository.Sizes;
using Workwear.Tools;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Company.EmployeeChildren;
using Workwear.ViewModels.Operations;

namespace WorkwearTest.ViewModels.Company {
	[TestFixture(TestOf = typeof(EmployeeViewModel))]
	public class EmployeeViewModelTest  : InMemoryDBGlobalConfigTestFixtureBase{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			NotifyConfiguration.Enable();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем что редактируя ручные выдачи из карточки сотрудника пользователь сразу же увидит изменения в карточке.")]
		[Category("Integrated")]
		[Category("Real case")]
		public void EditManualIssueCase()
		{
			NewSessionWithSameDB();
			
			var baseParameters = Substitute.For<BaseParameters>();
			var interactive = Substitute.For<IInteractiveService>();
			var commonMessages = Substitute.For<CommonMessages>(interactive);
			var deleteEntityService = Substitute.For<IDeleteEntityService>();
			var featureService = Substitute.For<FeaturesService>();
			var lkUserManagerService = Substitute.For<LkUserManagerService>("");
			var tdiCompatibilityNavigation = Substitute.For<ITdiCompatibilityNavigation>();
			var openStockDocumentsModel = Substitute.For<OpenStockDocumentsModel>(tdiCompatibilityNavigation);
			var progress = Substitute.For<IProgressBarDisplayable>();
			var sizeRepository = Substitute.For<SizeRepository>();
			var sizeService = Substitute.For<SizeService>(sizeRepository);
			var userService = Substitute.For<IUserService>();
			var validator = new ValidatorForTests();

			var builder = new ContainerBuilder();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<EmployeeListedItemsViewModel>().AsSelf();
			builder.RegisterType<EmployeeMovementsViewModel>().AsSelf();
			builder.RegisterType<EmployeeNormsViewModel>().AsSelf();
			builder.RegisterType<EmployeePhotoViewModel>().AsSelf();
			builder.RegisterType<EmployeeRepository>().AsSelf();
			builder.RegisterType<EmployeeVacationsViewModel>().AsSelf();
			builder.RegisterType<EmployeeViewModel>().AsSelf();
			builder.RegisterType<EmployeeWearItemsViewModel>().AsSelf();
			builder.RegisterType<ManualEmployeeIssueOperationsViewModel>().AsSelf();
			builder.RegisterType<NavigationManagerForTests>().AsSelf().As<INavigationManager>().SingleInstance();
			builder.RegisterType<NormRepository>().AsSelf();
			builder.RegisterType<PersonNames>().AsSelf();
			builder.RegisterType<UnitOfWorkProvider>().AsSelf().InstancePerLifetimeScope();
			builder.Register(x => Substitute.For<BarcodeService>(baseParameters)).AsSelf();
			builder.Register(x => UnitOfWorkFactory).As<IUnitOfWorkFactory>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => commonMessages).As<CommonMessages>();
			builder.Register(x => deleteEntityService).As<IDeleteEntityService>();
			builder.Register(x => featureService).As<FeaturesService>();
			builder.Register(x => interactive).As<IInteractiveService>().As<IInteractiveMessage>().As<IInteractiveQuestion>();
			builder.Register(x => lkUserManagerService).As<LkUserManagerService>();
			builder.Register(x => openStockDocumentsModel).As<OpenStockDocumentsModel>();
			builder.Register(x => progress).As<IProgressBarDisplayable>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => tdiCompatibilityNavigation).As<ITdiCompatibilityNavigation>();
			builder.Register(x => userService).As<IUserService>();
			builder.Register(x => validator).As<IValidator>();
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
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 6;
				normItem.NormPeriod = NormPeriodType.Year;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				// Создаем ручную выдачу
				var issuedOperation1 = new EmployeeIssueOperation {
					Employee = employee,
					ProtectionTools = protectionTools,
					NormItem = normItem,
					ManualOperation = true,
					OperationTime = new DateTime(2022, 11, 11),
					StartOfUse = new DateTime(2022, 11, 11),
					UseAutoWriteoff = true,
					AutoWriteoffDate = new DateTime(2023, 11, 11),
					ExpiryByNorm = new DateTime(2023, 11, 11),
					Issued = 6
				};
				uow.Save(issuedOperation1);
				uow.Commit();
				
				employee.FillWearReceivedInfo(new EmployeeIssueRepository(uow));
				employee.UpdateNextIssueAll();

				var employeeItem = employee.WorkwearItems.First();
				Assert.That(employeeItem.NextIssue, Is.EqualTo(new DateTime(2023, 11, 11)));
				Assert.That(employeeItem.LastIssued(new DateTime(2023, 1, 1), baseParameters).First().date, Is.EqualTo(new DateTime(2022, 11, 11)));

				//Запускаем Модель карточки сотрудника
				var navigation = container.Resolve<NavigationManagerForTests>();
				var employeePage =
					navigation.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(employee.Id));
				var vmEmployeeCard = employeePage.ViewModel;

				//Переключаемся на вкладку спецодежда
				vmEmployeeCard.SwitchOn(3);
				//Редактирование ручной операции
				vmEmployeeCard.WearItemsViewModel.SetIssueDateManual(vmEmployeeCard.Entity.WorkwearItems.First());
				var vmManualIssue = (vmEmployeeCard.NavigationManager as NavigationManagerForTests).FindPage<ManualEmployeeIssueOperationsViewModel>();
				vmManualIssue.ViewModel.DateTime = new DateTime(2022, 11, 21);
				vmManualIssue.ViewModel.SaveAndClose();
				//Проверяем что данные внутри карточки изменились.
				var item = vmEmployeeCard.Entity.WorkwearItems.First();
				Assert.That(item.NextIssue, Is.EqualTo(new DateTime(2023, 11, 21)));
				Assert.That(item.LastIssued(new DateTime(2023, 1, 1), baseParameters).First().date, Is.EqualTo(new DateTime(2022, 11, 21)));
				vmEmployeeCard.Close(false, CloseSource.AppQuit);
			}
		}
	}
}
