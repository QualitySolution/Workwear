using System;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
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
			
			var stockRepository = new StockRepository();

			var builder = new ContainerBuilder();
			builder.RegisterType<ExpenseDocItemsEmployeeViewModel>().AsSelf();
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
					       baseParameters))
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
				using (var vmCreate = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters, employee))
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
				using (var vmResave = new ExpenseEmployeeViewModel(EntityUoWBuilder.ForOpen(expenseIdForResave), UnitOfWorkFactory, navigation, container.BeginLifetimeScope(), validator, userService, userRepository, interactive, stockRepository, commonMessages, featuresService, baseParameters))
				{
					Assert.That(vmResave.Save(), Is.True);
				}
			}
		}
	}
}