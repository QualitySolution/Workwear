using System;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Testing.DB;
using QS.Validation.Testing;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.Domain.Regulations;
using Workwear.Domain.Regulations;
using workwear.Domain.Stock;
using Workwear.Measurements;
using workwear.Repository;
using workwear.Repository.Operations;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;

namespace WorkwearTest.ViewModels.Stock
{
	[TestFixture(TestOf = typeof(ExpenseEmployeeViewModel))]
	public class ExpenseEmployeeViewModelTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "Проверяем проверяем что можем пере-сохранить документ с одновременным списанием.")]
		[Category("Integrated")]
		public void CreateAndResaveDocWithWriteoff()
		{
			NewSessionWithSameDB();
			NotifyConfiguration.Enable();
			
			var navigation = Substitute.For<INavigationManager>();
			var validator = new ValidatorForTests();
			var userService = Substitute.For<IUserService>();
			var userRepository = Substitute.For<UserRepository>();
			var interactive = Substitute.For<IInteractiveService>();
			var commonMessages = Substitute.For<CommonMessages>(interactive);
			var featuresService = Substitute.For<FeaturesService>();
			var baseParameters = Substitute.For<BaseParameters>();
			var sizeService = Substitute.For<SizeService>(new BaseSizeSettings(baseParameters));
			var deleteService = Substitute.For<IDeleteEntityService>();
			var progress = Substitute.For<IProgressBarDisplayable>();
			
			var stockRepository = new StockRepository();

			var builder = new ContainerBuilder();
			builder.RegisterType<ExpenseDocItemsEmployeeViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.Register(x => featuresService).As<FeaturesService>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => deleteService).As<IDeleteEntityService>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var user = new UserBase();
				uow.Save(user);
				userService.GetCurrentUser(Arg.Any<IUnitOfWork>()).Returns(user);

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
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				var warehouseOperation = new WarehouseOperation {
					Amount = 10,
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
				};
				uow.Save(warehouseOperation);
				
				uow.Commit();
				
				//Создаем предыдущую выдачу которая должна быть списана.
				using (var vmCreateLastIssue = new ExpenseEmployeeViewModel(
					       EntityUoWBuilder.ForCreate(),
					       UnitOfWorkFactory,
					       navigation,
					       container.BeginLifetimeScope(),
					       validator,
					       userService,
					       userRepository,
					       interactive,
					       stockRepository,
					       commonMessages,
					       featuresService,
					       baseParameters,
					       progress
				))
				{
					vmCreateLastIssue.Entity.Date = new DateTime(2022, 04, 1);
					vmCreateLastIssue.Entity.Warehouse = vmCreateLastIssue.UoW.GetById<Warehouse>(warehouse.Id);
					vmCreateLastIssue.Entity.Employee = vmCreateLastIssue.UoW.GetById<EmployeeCard>(employee.Id);
				
					Assert.That(vmCreateLastIssue.Entity.Items.Count, Is.EqualTo(1));
					var itemLast = vmCreateLastIssue.Entity.Items.First();
					itemLast.Amount = 1;
					Assert.That(itemLast.IsEnableWriteOff, Is.False);

					Assert.That(vmCreateLastIssue.Save(), Is.True);
				}

				//Создаем выдачу в место выданного ранее.
				int expenseIdForResave;
				using (var vmCreate = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters, progress, employee))
				{
					vmCreate.Entity.Date = new DateTime(2022, 04, 12);
					vmCreate.Entity.Warehouse = vmCreate.UoW.GetById<Warehouse>(warehouse.Id);

					Assert.That(vmCreate.Entity.Items.Count, Is.EqualTo(1));
					var item = vmCreate.Entity.Items.First();
					item.Amount = 1;
					Assert.That(item.IsEnableWriteOff, Is.True);
					//Главное в этом тесте, устанавливаем галочку что выдача идет вместе со списанием.
					item.IsWriteOff = true;

					Assert.That(vmCreate.Save(), Is.True);
					expenseIdForResave = vmCreate.Entity.Id;
				}
				
				//Пересохраняем для проверки что это работает
				using (var vmResave = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForOpen(expenseIdForResave), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters, progress))
				{
					Assert.That(vmResave.Save(), Is.True);
				}
			}
		}
		
		[Test(Description = "Проверяем проверяем что можем сохранить документ выдачи с созданием ведомости.")]
		[Category("Integrated")]
		[Ignore("Отключен до версии 2.8, чтобы не переделывать зависимости")]
		public void CreateWithIssuanceSheetAndResave()
		{
			NewSessionWithSameDB();
			NotifyConfiguration.Enable();
			
			var navigation = Substitute.For<INavigationManager>();
			var validator = new ValidatorForTests();
			var userService = Substitute.For<IUserService>();
			var userRepository = Substitute.For<UserRepository>(userService);
			var interactive = Substitute.For<IInteractiveService>();
			var commonMessages = Substitute.For<CommonMessages>(interactive);
			var featuresService = Substitute.For<FeaturesService>();
			var baseParameters = Substitute.For<BaseParameters>();
			var sizeService = Substitute.For<SizeService>();
			var deleteService = Substitute.For<IDeleteEntityService>();
			var progress = Substitute.For<IProgressBarDisplayable>();
			
			var stockRepository = new StockRepository();

			var builder = new ContainerBuilder();
			builder.RegisterType<ExpenseDocItemsEmployeeViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.Register(x => featuresService).As<FeaturesService>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => deleteService).As<IDeleteEntityService>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var user = new UserBase();
				uow.Save(user);
				userService.GetCurrentUser(Arg.Any<IUnitOfWork>()).Returns(user);

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
				
				var protectionTools2 = new ProtectionTools {
					Name = "Тестовый СИЗ 2",
					Type = itemType
				};
				protectionTools2.AddNomeclature(nomenclature);
				uow.Save(protectionTools2);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 1;
				var normItem2 = norm.AddItem(protectionTools2);
				normItem2.Amount = 12;
				normItem2.NormPeriod = NormPeriodType.Year;
				normItem2.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				var warehouseOperation = new WarehouseOperation {
					Amount = 100,
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
				};
				uow.Save(warehouseOperation);
				
				uow.Commit();
				
				int expenseIdForResave;
				//Диалог создания документа выдачи.
				using (var vmCreateIssue = new ExpenseEmployeeViewModel(
					       EntityUoWBuilder.ForCreate(),
					       UnitOfWorkFactory,
					       navigation,
					       container.BeginLifetimeScope(),
					       validator,
					       userService,
					       userRepository,
					       interactive,
					       stockRepository,
					       commonMessages,
					       featuresService,
					       baseParameters,
					       progress
					       ))
				{
					vmCreateIssue.Entity.Date = new DateTime(2022, 04, 1);
					vmCreateIssue.Entity.Warehouse = vmCreateIssue.UoW.GetById<Warehouse>(warehouse.Id);
					vmCreateIssue.Entity.Employee = vmCreateIssue.UoW.GetById<EmployeeCard>(employee.Id);
				
					Assert.That(vmCreateIssue.Entity.Items.Count, Is.EqualTo(2));
					var itemLast = vmCreateIssue.Entity.Items.First(x => x.ProtectionTools.Id == protectionTools.Id);
					itemLast.Amount = 1;
					
					var item2 = vmCreateIssue.Entity.Items.First(x => x.ProtectionTools.Id == protectionTools2.Id);
					item2.Amount = 10;
					
					//Создаем ведомость
					vmCreateIssue.CreateIssuenceSheet();

					Assert.That(vmCreateIssue.Save(), Is.True);
					expenseIdForResave = vmCreateIssue.Entity.Id;
				}

				//За одно проверяем возможность пере-сохранения с добавление и удалением строки.

				using (var vmResaveCreate = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForOpen(expenseIdForResave), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters, progress)) {
					var expense = vmResaveCreate.Entity;
					Assert.That(expense.IssuanceSheet, Is.Not.Null);
					var itemsheet1 = expense.IssuanceSheet.Items.First(x => x.ExpenseItem.ProtectionTools.IsSame(protectionTools));
					Assert.That(itemsheet1.Amount, Is.EqualTo(1));
					var itemsheet2 = expense.IssuanceSheet.Items.First(x => x.ExpenseItem.ProtectionTools.IsSame(protectionTools2));
					Assert.That(itemsheet2.Amount, Is.EqualTo(10));
					
					vmResaveCreate.Entity.RemoveItem(itemsheet1.ExpenseItem);

					var changed = itemsheet2.ExpenseItem;
					changed.Amount = 12;
					Assert.That(vmResaveCreate.Save(), Is.True);
					
					Assert.That(itemsheet2.Amount, Is.EqualTo(12));
				}
			}
		}
		
		[Test(Description = "Проверяем проверяем что можем сохранить документ выдачи с созданием ведомости. В случае если ведомость создали а потом убрали количество у одной из строк.")]
		[Category("Integrated")]
		[Category("Real case")]
		[Ignore("Отключен до версии 2.8, чтобы не переделывать зависимости")]
		public void CreateWithIssuanceSheet_SetAmountZeroAfterCreateSheet()
		{
			NewSessionWithSameDB();
			NotifyConfiguration.Enable();
			
			var navigation = Substitute.For<INavigationManager>();
			var validator = new ValidatorForTests();
			var userService = Substitute.For<IUserService>();
			var userRepository = Substitute.For<UserRepository>(userService);
			var interactive = Substitute.For<IInteractiveService>();
			var commonMessages = Substitute.For<CommonMessages>(interactive);
			var featuresService = Substitute.For<FeaturesService>();
			var baseParameters = Substitute.For<BaseParameters>();
			var sizeService = Substitute.For<SizeService>();
			var deleteService = Substitute.For<IDeleteEntityService>();
			var progress = Substitute.For<IProgressBarDisplayable>();
			
			var stockRepository = new StockRepository();

			var builder = new ContainerBuilder();
			builder.RegisterType<ExpenseDocItemsEmployeeViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.Register(x => featuresService).As<FeaturesService>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => deleteService).As<IDeleteEntityService>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var warehouse = new Warehouse();
				uow.Save(warehouse);

				var user = new UserBase();
				uow.Save(user);
				userService.GetCurrentUser(Arg.Any<IUnitOfWork>()).Returns(user);

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
				
				var protectionTools2 = new ProtectionTools {
					Name = "Тестовый СИЗ 2",
					Type = itemType
				};
				protectionTools2.AddNomeclature(nomenclature);
				uow.Save(protectionTools2);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 1;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 1;
				var normItem2 = norm.AddItem(protectionTools2);
				normItem2.Amount = 12;
				normItem2.NormPeriod = NormPeriodType.Year;
				normItem2.PeriodCount = 1;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				var warehouseOperation = new WarehouseOperation {
					Amount = 100,
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
				};
				uow.Save(warehouseOperation);
				
				uow.Commit();
				
				//Диалог создания документа выдачи.
				using (var vmCreateIssue = new ExpenseEmployeeViewModel(
					       EntityUoWBuilder.ForCreate(),
					       UnitOfWorkFactory,
					       navigation,
					       container.BeginLifetimeScope(),
					       validator,
					       userService,
					       userRepository,
					       interactive,
					       stockRepository,
					       commonMessages,
					       featuresService,
					       baseParameters,
					       progress
					       ))
				{
					vmCreateIssue.Entity.Date = new DateTime(2022, 04, 1);
					vmCreateIssue.Entity.Warehouse = vmCreateIssue.UoW.GetById<Warehouse>(warehouse.Id);
					vmCreateIssue.Entity.Employee = vmCreateIssue.UoW.GetById<EmployeeCard>(employee.Id);
				
					Assert.That(vmCreateIssue.Entity.Items.Count, Is.EqualTo(2));
					var itemLast = vmCreateIssue.Entity.Items.First(x => x.ProtectionTools.Id == protectionTools.Id);
					itemLast.Amount = 1;
					
					var item2 = vmCreateIssue.Entity.Items.First(x => x.ProtectionTools.Id == protectionTools2.Id);
					item2.Amount = 10;
					
					//Создаем ведомость
					vmCreateIssue.CreateIssuenceSheet();
					itemLast.Amount = 0; //Тест падал из за этого, стока будет удалена, а в ведомости уже создана.
					
					Assert.That(vmCreateIssue.Save(), Is.True);
				}
			}
		}
	}
}
