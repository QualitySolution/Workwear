using System;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.Dialog.Testing;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.NotifyChange.Conditions;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Testing.DB;
using QS.Validation;
using QS.Validation.Testing;
using QS.ViewModels.Resolve;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Models.Operations;
using Workwear.Models.Print;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.Tools.User;
using Workwear.ViewModels.Stock;

namespace WorkwearTest.ViewModels.Stock
{
	[TestFixture(TestOf = typeof(CollectiveExpenseViewModel))]
	public class CollectiveExpenseViewModelTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			NotifyConfiguration.Enable();
			InitialiseUowFactory();
		}

		ContainerBuilder MakeContainer(IUserService userService, CurrentUserSettings currentUserSettings)
		{
			var baseParameters = Substitute.For<BaseParameters>();
			var deleteService = Substitute.For<IDeleteEntityService>();
			var featuresService = Substitute.For<FeaturesService>();
			var interactive = Substitute.For<IInteractiveService>();
			var navigation = Substitute.For<INavigationManager>();
			var progress = Substitute.For<IProgressBarDisplayable>();
			var sizeService = new SizeService();
			var validator = new ValidatorForTests();
			var commonMessages = Substitute.For<CommonMessages>(interactive);
			var changeWatcher = Substitute.For<IEntityChangeWatcher>();
			changeWatcher.BatchSubscribe(Arg.Any<BatchEntityChangeHandler>()).Returns(new SelectionConditions());
			var permissions = Substitute.For<ICurrentPermissionService>();
			permissions.ValidateEntityPermission(Arg.Any<Type>()).Returns(new SimplePermissionResult(true, true, true, true));
			permissions.ValidateEntityPermission(Arg.Any<Type>(), Arg.Any<DateTime?>()).Returns(new SimplePermissionResult(true, true, true, true));

			var builder = new ContainerBuilder();
			builder.Register(x => UnitOfWorkFactory).As<IUnitOfWorkFactory>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => commonMessages).AsSelf();
			builder.Register(x => currentUserSettings).AsSelf();
			builder.Register(x => deleteService).As<IDeleteEntityService>();
			builder.Register(x => featuresService).As<FeaturesService>();
			builder.Register(x => interactive).As<IInteractiveQuestion>().As<IInteractiveService>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => permissions).As<ICurrentPermissionService>();
			builder.Register(x => progress).As<IProgressBarDisplayable>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => userService).As<IUserService>();
			builder.Register(x => validator).As<IValidator>();
			builder.Register(x => Substitute.For<IViewModelResolver>()).As<IViewModelResolver>();
			builder.Register(x => changeWatcher).As<IEntityChangeWatcher>();
			builder.RegisterType<CollectiveExpenseItemsViewModel>().AsSelf();
			builder.RegisterType<CollectiveExpenseViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueModel>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<EmployeeRepository>().AsSelf();
			builder.RegisterType<IssuedSheetPrintModel>().AsSelf();
			builder.RegisterType<ModalProgressCreatorForTests>().As<ModalProgressCreator>();
			builder.RegisterType<StockBalanceModel>().AsSelf();
			builder.RegisterType<StockRepository>().AsSelf();
			builder.RegisterType<UnitOfWorkProvider>().AsSelf().InstancePerLifetimeScope();
			
			return builder;
		}

		[Test(Description = "Проверяем что сохраняется новая коллективная выдача с созданной ведомостью.")]
		[Category("Integrated")]
		public void CreateWithIssuanceSheet()
		{
			NewSessionWithSameDB();
			NotifyConfiguration.Enable();
			
			var userService = Substitute.For<IUserService>();
			var currentUserSettings = Substitute.For<CurrentUserSettings>();
			currentUserSettings.Settings.DefaultOrganization.Returns(null as Organization);
			currentUserSettings.Settings.DefaultLeader.Returns(null as Leader);
			currentUserSettings.Settings.DefaultResponsiblePerson.Returns(null as Leader);
			var container = MakeContainer(userService, currentUserSettings).Build();

			int collectiveExpenseId;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var warehouse = new Warehouse();
				uow.Save(warehouse);

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
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.DismissDate = null;
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				var warehouseOperation = new WarehouseOperation {
					Amount = 10,
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
				};
				uow.Save(warehouseOperation);
				
				uow.Commit();

				using(var scope = container.BeginLifetimeScope()) {
					var vmCreate = scope.Resolve<CollectiveExpenseViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate())
					);
					vmCreate.Entity.Date = new DateTime(2022, 04, 12);
					vmCreate.Entity.Warehouse = vmCreate.UoW.GetById<Warehouse>(warehouse.Id);

					var employeeInSession = vmCreate.UoW.GetById<EmployeeCard>(employee.Id);
					var nomenclatureInSession = vmCreate.UoW.GetById<Nomenclature>(nomenclature.Id);
					var item = vmCreate.Entity.AddItem(
						employeeInSession.WorkwearItems.First(),
						new StockPosition(nomenclatureInSession, 0, null, null, null),
						1
					);

					vmCreate.CreateIssuanceSheet();
					
					Assert.That(vmCreate.Save(), Is.True);
					Assert.That(item.Id, Is.GreaterThan(0));
					Assert.That(item.IssuanceSheetItem, Is.Not.Null);
					Assert.That(item.IssuanceSheetItem.Id, Is.GreaterThan(0));
					collectiveExpenseId = vmCreate.Entity.Id;
				}

				using(var checkUow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var savedExpense = checkUow.GetById<CollectiveExpense>(collectiveExpenseId);
					var savedIssuanceSheet = checkUow.Session.QueryOver<IssuanceSheet>()
						.Where(x => x.CollectiveExpense.Id == collectiveExpenseId)
						.SingleOrDefault();
				
					Assert.That(savedIssuanceSheet, Is.Not.Null);
					Assert.That(savedExpense.Items.Count, Is.EqualTo(1));
					Assert.That(savedIssuanceSheet.Items.Count, Is.EqualTo(1));
					Assert.That(savedIssuanceSheet.Items.First().CollectiveExpenseItem.Id,
						Is.EqualTo(savedExpense.Items.First().Id));
				}
			}
		}
	}
}
