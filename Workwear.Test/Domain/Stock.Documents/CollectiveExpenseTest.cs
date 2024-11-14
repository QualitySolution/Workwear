using System;
using NSubstitute;
using NUnit.Framework;
using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Stock;
using Workwear.Models.Operations;
using Workwear.Tools;

namespace Workwear.Test.Domain.Stock.Documents {
	[TestFixture(TestOf = typeof(CollectiveExpense))]
	public class CollectiveExpenseTest {
		[Test(Description = "Проверяем что при добавлении строки в документ, если первая из лучших позиций не подходит по количеству, а далее есть подходящие, но менее приоритетные мы хоть что нибудь добавим.")]
		public void AddItem_NotBest() {
			var parameters = Substitute.For<BaseParameters>();
			var itemType = Substitute.For<ItemsType>();
			var nomenclatureOne = new Nomenclature {
				Id = 25,
				Type = itemType
			};
			var nomenclatureNegativeBalance = new Nomenclature {
				Id = 26,
				Type = itemType
			};
			var nomenclaturePositiveBalance = new Nomenclature {
				Id = 250,
				Type = itemType
			};
			var protectionTools = Substitute.For<ProtectionTools>();
			protectionTools.Nomenclatures.Returns(new ObservableList<Nomenclature> { nomenclatureOne, nomenclatureNegativeBalance, nomenclaturePositiveBalance });
			
			var employeeItem = Substitute.For<EmployeeCardItem>();
			employeeItem.CalculateRequiredIssue(parameters, Arg.Any<DateTime>()).Returns(4);
			
			var stockList = new List<StockBalance> {
				new StockBalance(new StockPosition(nomenclatureNegativeBalance, 0, null, null, null), -10),
				new StockBalance(new StockPosition(nomenclatureOne, 0, null, null, null), 1),
				new StockBalance(new StockPosition(nomenclaturePositiveBalance, 0, null, null, null), 10)
			};
			employeeItem.BestChoiceInStock.Returns(stockList);
			
			var expense = new CollectiveExpense();
			
			//Проверка
			var item = expense.AddItem(employeeItem, parameters);
			Assert.That(item.Nomenclature, Is.Not.Null);
			Assert.That(item.Nomenclature, Is.EqualTo(nomenclaturePositiveBalance));
		}
		
		[Test(Description = "Проверяем что при создании операции используем настройку базы автосписания по умолчанию.")]
		[TestCase(true)]
		[TestCase(false)]
		public void UpdateOperstions_DefaultAutoWriteOff(bool autoWriteOff) {
			var parameters = Substitute.For<BaseParameters>();
			parameters.DefaultAutoWriteoff.Returns(autoWriteOff);
			var uow = Substitute.For<IUnitOfWork>();
			
			var nomenclature = Substitute.For<Nomenclature>();
			
			var employeeItem = Substitute.For<EmployeeCardItem>();
			var employee = Substitute.For<EmployeeCard>();
			employeeItem.EmployeeCard.Returns(employee);
			
			var expense = new CollectiveExpense();
			expense.Date = new DateTime(2023, 1, 1);
			var item = new CollectiveExpenseItem {
				Document = expense,
				Employee = employee,
				ProtectionTools = Substitute.For<ProtectionTools>(),
				Nomenclature = nomenclature,
				Amount = 1
			};
			expense.Items.Add(item);
			
			//Проверка
			expense.UpdateOperations(uow, parameters, Substitute.For<IInteractiveQuestion>());
			Assert.That(item.EmployeeIssueOperation.UseAutoWriteoff, Is.EqualTo(autoWriteOff));
		}
	}
}
