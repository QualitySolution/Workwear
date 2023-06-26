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
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Measurements;
using Workwear.Repository.Operations;
using Workwear.Repository.Sizes;
using Workwear.Repository.Stock;
using Workwear.Repository.User;
using Workwear.Tools;
using Workwear.Tools.Barcodes;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace WorkwearTest.ViewModels.Stock
{
	[TestFixture(TestOf = typeof(ExpenseEmployeeViewModel))]
	public class ExpenseEmployeeViewModelTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			NotifyConfiguration.Enable();
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
			var userRepository = Substitute.For<UserRepository>(userService);
			var interactive = Substitute.For<IInteractiveService>();
			var commonMessages = Substitute.For<CommonMessages>(interactive);
			var featuresService = Substitute.For<FeaturesService>();
			var baseParameters = Substitute.For<BaseParameters>();
			var sizeRepository = Substitute.For<SizeRepository>();
			var sizeService = Substitute.For<SizeService>(sizeRepository);
			var deleteService = Substitute.For<IDeleteEntityService>();
			var progress = Substitute.For<IProgressBarDisplayable>();
			
			var stockRepository = new StockRepository();

			var builder = new ContainerBuilder();
			builder.RegisterType<ExpenseDocItemsEmployeeViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<BarcodeService>().AsSelf();
			builder.Register(x => featuresService).As<FeaturesService>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => deleteService).As<IDeleteEntityService>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => interactive).As<IInteractiveQuestion>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var issueRepository = new EmployeeIssueRepository(uow);
				
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
					       progress,
					       issueRepository
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
				using (var vmCreate = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters, progress, new EmployeeIssueRepository(), employee))
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
				using (var vmResave = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForOpen(expenseIdForResave), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters, progress, new EmployeeIssueRepository()))
				{
					Assert.That(vmResave.Save(), Is.True);
				}
			}
		}
		
		[Test(Description = "Проверяем что не предлагаем к выдачи позиции которые выдавались частями.")]
		[Category("Integrated")]
		[Category("Real case")]
		public void FillDoc_DontIssuePartyIssued()
		{
			NewSessionWithSameDB();
			NotifyConfiguration.Enable();
			
			var navigation = Substitute.For<INavigationManager>();
			var validator = new ValidatorForTests();
			var userService = Substitute.For<IUserService>();
			var userRepository = Substitute.For<UserRepository>(userService);
			var interactive = Substitute.For<IInteractiveService>();
			//Отвечаем на вопрос об обновлении количества в документе "да"
			interactive.Question(Arg.Any<string>()).Returns(true); 
			var commonMessages = Substitute.For<CommonMessages>(interactive);
			var featuresService = Substitute.For<FeaturesService>();
			var baseParameters = Substitute.For<BaseParameters>();
			var sizeRepository = Substitute.For<SizeRepository>();
			var sizeService = Substitute.For<SizeService>(sizeRepository);
			var deleteService = Substitute.For<IDeleteEntityService>();
			var progress = Substitute.For<IProgressBarDisplayable>();
			
			var stockRepository = new StockRepository();

			var builder = new ContainerBuilder();
			builder.RegisterType<ExpenseDocItemsEmployeeViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<BarcodeService>().AsSelf();
			builder.Register(x => featuresService).As<FeaturesService>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => deleteService).As<IDeleteEntityService>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => interactive).As<IInteractiveQuestion>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var issueRepository = new EmployeeIssueRepository(uow);
				
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
				
				var nomenclature2 = new Nomenclature {
					Type = itemType
				};
				uow.Save(nomenclature2);

				var protectionTools = new ProtectionTools {
					Name = "Тестовый СИЗ(уже выдано частично)",
					Type = itemType
				};
				protectionTools.AddNomeclature(nomenclature);
				uow.Save(protectionTools);
				
				var protectionTools2 = new ProtectionTools {
					Name = "Тестовый СИЗ (надо выдать)",
					Type = itemType
				};
				protectionTools2.AddNomeclature(nomenclature2);
				uow.Save(protectionTools2);

				var norm = new Norm();
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 12;
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 12;
				var normItem2 = norm.AddItem(protectionTools2);
				normItem2.Amount = 12;
				normItem2.NormPeriod = NormPeriodType.Month;
				normItem2.PeriodCount = 12;
				uow.Save(norm);

				var employee = new EmployeeCard();
				employee.UoW = uow;
				employee.AddUsedNorm(norm);
				foreach (var item in employee.WorkwearItems) {
					item.Created = new DateTime(2020, 1, 1);
				}
				uow.Save(employee);

				var warehouseOperation = new WarehouseOperation {
					Amount = 20,
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
				};
				uow.Save(warehouseOperation);
				
				var warehouseOperation2 = new WarehouseOperation {
					Amount = 20,
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature2,
				};
				uow.Save(warehouseOperation2);
				
				//Частичные выдачи первой номенклатуры
				var issuedOperation = new EmployeeIssueOperation {
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					NormItem = normItem,
					OperationTime = new DateTime(2020, 1, 2),
					StartOfUse = new DateTime(2020, 1, 2),
					UseAutoWriteoff = true,
					AutoWriteoffDate = new DateTime(2021, 1, 2),
					ExpiryByNorm = new DateTime(2021, 1, 2),
					Issued = 9
				};
				uow.Save(issuedOperation);
				
				//Выдача второй части 3 через 10 дней. Итого у человека 9 + 3 = 12
				var issuedOperation2 = new EmployeeIssueOperation {
					Employee = employee,
					Nomenclature = nomenclature,
					ProtectionTools = protectionTools,
					NormItem = normItem,
					OperationTime = new DateTime(2020, 1, 12),
					StartOfUse = new DateTime(2020, 1, 12),
					UseAutoWriteoff = true,
					AutoWriteoffDate = new DateTime(2021, 1, 12),
					ExpiryByNorm = new DateTime(2021, 1, 12),
					Issued = 3
				};
				uow.Save(issuedOperation2);
				
				employee.UpdateNextIssueAll();
				uow.Commit();

				//Создаем выдачу чтобы выдать вторую номенклатуру программа не должна предлагать к выдаче первую.
				//Так как по первой сотрудник обеспечен полностью хоть выдача и происходила частями.
				using (var vmCreate = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters, progress, issueRepository, employee))
				{
					vmCreate.Entity.Date = new DateTime(2020, 04, 12);
					vmCreate.Entity.Warehouse = vmCreate.UoW.GetById<Warehouse>(warehouse.Id);

					Assert.That(vmCreate.Entity.Items.Count, Is.EqualTo(2));
					
					var item2 = vmCreate.Entity.Items.First(x => x.ProtectionTools.IsSame(protectionTools2));
					Assert.That(item2.Amount, Is.EqualTo(12));
					
					var item1 = vmCreate.Entity.Items.First(x => x.ProtectionTools.IsSame(protectionTools));
					Assert.That(item1.Amount, Is.EqualTo(0));
				}
			}
		}

		#region Ведомости
		[Test(Description = "Проверяем проверяем что можем сохранить документ выдачи с созданием ведомости.")]
		[Category("Integrated")]
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
			var sizeRepository = Substitute.For<SizeRepository>();
			var sizeService = Substitute.For<SizeService>(sizeRepository);
			var deleteService = Substitute.For<IDeleteEntityService>();
			var progress = Substitute.For<IProgressBarDisplayable>();
			
			var stockRepository = new StockRepository();

			var builder = new ContainerBuilder();
			builder.RegisterType<ExpenseDocItemsEmployeeViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<BarcodeService>().AsSelf();
			builder.Register(x => featuresService).As<FeaturesService>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => deleteService).As<IDeleteEntityService>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => interactive).As<IInteractiveQuestion>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var issueRepository = new EmployeeIssueRepository(uow);
				
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
					       progress,
					       issueRepository
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
					vmCreateIssue.CreateIssuanceSheet();

					Assert.That(vmCreateIssue.Save(), Is.True);
					expenseIdForResave = vmCreateIssue.Entity.Id;
				}

				//За одно проверяем возможность пере-сохранения с добавление и удалением строки.

				using (var vmResaveCreate = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForOpen(expenseIdForResave), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters, progress, new EmployeeIssueRepository())) {
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
			var sizeRepository = Substitute.For<SizeRepository>();
			var sizeService = Substitute.For<SizeService>(sizeRepository);
			var deleteService = Substitute.For<IDeleteEntityService>();
			var progress = Substitute.For<IProgressBarDisplayable>();
			
			var stockRepository = new StockRepository();

			var builder = new ContainerBuilder();
			builder.RegisterType<ExpenseDocItemsEmployeeViewModel>().AsSelf();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<BarcodeService>().AsSelf();
			builder.Register(x => featuresService).As<FeaturesService>();
			builder.Register(x => navigation).As<INavigationManager>();
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => deleteService).As<IDeleteEntityService>();
			builder.Register(x => baseParameters).As<BaseParameters>();
			builder.Register(x => interactive).As<IInteractiveQuestion>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			var container = builder.Build();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
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
					       progress,
					       new EmployeeIssueRepository()
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
					vmCreateIssue.CreateIssuanceSheet();
					itemLast.Amount = 0; //Тест падал из за этого, стока будет удалена, а в ведомости уже создана.
					
					Assert.That(vmCreateIssue.Save(), Is.True);
				}
			}
		}
		#endregion
	}
}
