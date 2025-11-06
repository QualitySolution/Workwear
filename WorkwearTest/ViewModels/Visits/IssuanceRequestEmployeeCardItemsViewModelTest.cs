using System;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Testing.DB;
using QS.Validation;
using QS.Validation.Testing;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Visits;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.Tools.User;
using Workwear.ViewModels.Visits;

namespace WorkwearTest.ViewModels.Visits
{
	[TestFixture(TestOf = typeof(IssuanceRequestEmployeeCardItemsViewModel))]
	public class IssuanceRequestEmployeeCardItemsViewModelTest : InMemoryDBGlobalConfigTestFixtureBase
	{
		[OneTimeSetUp]
		public void Init()
		{
			ConfigureOneTime.ConfigureNh();
			NotifyConfiguration.Enable();
			InitialiseUowFactory();
		}

		#region Helpers
		ContainerBuilder MakeContainer(IUserService userService, CurrentUserSettings currentUserSettings) {
			var baseParameters = Substitute.For<BaseParameters>();
			var deleteService = Substitute.For<IDeleteEntityService>();
			var featuresService = Substitute.For<FeaturesService>();
			var interactive = Substitute.For<IInteractiveService>();
			var navigation = Substitute.For<INavigationManager>();
			var sizeService = Substitute.For<SizeService>();
			var validator = new ValidatorForTests();
			var commonMessages = Substitute.For<CommonMessages>(interactive);
			var permissions = Substitute.For<ICurrentPermissionService>();
			permissions.ValidateEntityPermission(Arg.Any<Type>()).Returns(new SimplePermissionResult(true, true, true, true));

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
			builder.Register(x => sizeService).As<SizeService>();
			builder.Register(x => userService).As<IUserService>();
			builder.Register(x => validator).As<IValidator>();
			builder.RegisterType<EmployeeIssueModel>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf();
			builder.RegisterType<EmployeeRepository>().AsSelf();
			builder.RegisterType<StockBalanceModel>().AsSelf();
			builder.RegisterType<StockRepository>().AsSelf();
			builder.RegisterType<UnitOfWorkProvider>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<IssuanceRequestViewModel>().AsSelf();
			builder.RegisterType<IssuanceRequestEmployeeCardItemsViewModel>().AsSelf();
			
			return builder;
		}
		#endregion

		[Test(Description = "Проверяем, что операции документа выдачи с датой до даты заявки исключаются при подсчете потребности")]
		[Category("Integrated")]
		public void UpdateNodes_ExcludesExpenseOperationsBeforeRequestDate()
		{
			NewSessionWithSameDB();
			NotifyConfiguration.Enable();
			
			var userService = Substitute.For<IUserService>();
			var currentUserSettings = Substitute.For<CurrentUserSettings>();
			var container = MakeContainer(userService, currentUserSettings).Build();
			var baseParameters = container.Resolve<BaseParameters>();
			var interactive = container.Resolve<IInteractiveQuestion>();

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				// Создаем базовые сущности
				var warehouse = new Warehouse { Name = "Тестовый склад" };
				uow.Save(warehouse);

				var user = new UserBase();
				uow.Save(user);
				userService.GetCurrentUser().Returns(user);

				var itemType = new ItemsType {
					Name = "Тестовый тип СИЗ",
					IssueType = IssueType.Collective
				};
				uow.Save(itemType);
				
				var nomenclature = new Nomenclature {
					Type = itemType,
					Name = "Тестовая номенклатура"
				};
				uow.Save(nomenclature);

				var protectionTools = new ProtectionTools {
					Name = "Тестовый СИЗ",
					Type = itemType
				};
				protectionTools.AddNomenclature(nomenclature);
				uow.Save(protectionTools);

				// Создаем норму
				var norm = new Norm { Name = "Тестовая норма" };
				var normItem = norm.AddItem(protectionTools);
				normItem.Amount = 5; // Норма 5 единиц
				normItem.NormPeriod = NormPeriodType.Month;
				normItem.PeriodCount = 1;
				uow.Save(norm);

				// Создаем сотрудника
				var employee = new EmployeeCard { 
					PersonnelNumber = "123",
					LastName = "Тестов",
					FirstName = "Тест",
					HireDate = new DateTime(2020, 1, 1)
				};
				employee.AddUsedNorm(norm);
				uow.Save(employee);

				// Создаем складскую операцию (поступление товара на склад)
				var warehouseOperation = new WarehouseOperation {
					Amount = 20,
					ReceiptWarehouse = warehouse,
					Nomenclature = nomenclature,
					OperationTime = new DateTime(2023, 1, 1)
				};
				uow.Save(warehouseOperation);

				// Создаем заявку на выдачу с датой 15 января 2023
				var issuanceRequest = new IssuanceRequest {
					ReceiptDate = new DateTime(2023, 1, 15),
					Status = IssuanceRequestStatus.PartiallyIssued
				};
				issuanceRequest.Employees.Add(employee);
				uow.Save(issuanceRequest);

				// Создаем документ выдачи с датой ДО даты заявки (10 января 2023)
				var expense = new CollectiveExpense() {
					Date = new DateTime(2023, 1, 10), // До даты заявки
					Warehouse = warehouse,
					IssuanceRequest = issuanceRequest
				};
				
				var expenseItem = new CollectiveExpenseItem() {
					Document = expense,
					Employee = employee,
					Nomenclature = nomenclature,
					Amount = 3, // Выдали 3 единицы
					ProtectionTools = protectionTools
				};
				expenseItem.UpdateOperations(uow, baseParameters, interactive);
				expense.Items.Add(expenseItem);
				uow.Save(expense);
				uow.Commit();

				// Создаем ViewModel для тестирования
				using (var scope = container.BeginLifetimeScope()) {
					var employeeIssueModel = scope.Resolve<EmployeeIssueModel>();
					var stockBalanceModel = scope.Resolve<StockBalanceModel>();
					
					// Создаем полноценную IssuanceRequestViewModel
					var parentViewModel = scope.Resolve<IssuanceRequestViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(issuanceRequest.Id))
					);
					
					// Получаем дочернюю ViewModel из родительской
					var viewModel = parentViewModel.EmployeeCardItemsViewModel;

					// Выполняем тестируемый метод
					viewModel.UpdateNodes();

					// Проверяем результат
					Assert.That(viewModel.GroupedEmployeeCardItems, Is.Not.Null);
					Assert.That(viewModel.GroupedEmployeeCardItems.Count, Is.EqualTo(1));
					
					var node = viewModel.GroupedEmployeeCardItems.First();
					Assert.That(node.ProtectionToolsName, Is.EqualTo("Тестовый СИЗ"));
					
					// Главная проверка: потребность должна быть рассчитана без учета операций
					// документа выдачи с датой до даты заявки
					// Норма 5, но выдали 3 единицы 10 января (до заявки 15 января)
					// Эти 3 единицы НЕ должны учитываться при расчете потребности
					// Поэтому потребность должна быть 5 (полная норма)
					Assert.That(node.Need, Is.EqualTo(5), 
						"Операции документа выдачи с датой до даты заявки должны исключаться при подсчете потребности");
					
					// Уже выдано через коллективную выдачу
					Assert.That(node.Issued, Is.EqualTo(3));
					
					// Остается выдать
					Assert.That(node.NeedToBeIssued, Is.EqualTo(2));
				}
			}
		}
	}
}
