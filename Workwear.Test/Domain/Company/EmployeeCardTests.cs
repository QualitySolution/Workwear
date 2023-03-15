using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace Workwear.Test.Domain.Company
{
	[TestFixture(TestOf = typeof(EmployeeCard))]
	public class EmployeeCardTests
	{
		[Test(Description = "Проверяем что корректно объединяем 2 нормы")]
		public void UpdateWorkwearItems_MergeTwoNorms()
		{
			var protectionTools = Substitute.For<ProtectionTools>();
			var protectionTools2 = Substitute.For<ProtectionTools>();
			var protectionTools3 = Substitute.For<ProtectionTools>();
			
			var norm1 = Substitute.For<Norm>();
			var norm1item1 = Substitute.For<NormItem>();
			norm1item1.ProtectionTools.Returns(protectionTools);
			norm1item1.NormCondition.ReturnsNull();
			var norm1item2 = Substitute.For<NormItem>();
			norm1item2.ProtectionTools.Returns(protectionTools2);
			norm1item2.AmountPerYear.Returns(2);
			norm1item2.NormCondition.ReturnsNull();
			norm1.Items.Returns(new List<NormItem> { norm1item1, norm1item2 });
			
			var norm2 = Substitute.For<Norm>();
			var norm2item1 = Substitute.For<NormItem>();
			norm2item1.ProtectionTools.Returns(protectionTools2);
			norm2item1.AmountPerYear.Returns(5);
			norm2item1.NormCondition.ReturnsNull();
			var norm2item2 = Substitute.For<NormItem>();
			norm2item2.ProtectionTools.Returns(protectionTools3);
			norm2item2.NormCondition.ReturnsNull();
			norm2.Items.Returns(new List<NormItem> { norm2item1, norm2item2 });
			
			var employee = new EmployeeCard();
			employee.AddUsedNorm(norm1);
			employee.AddUsedNorm(norm2);

			//Происходит автоматически
			//employee.UpdateWorkwearItems();
			Assert.That(employee.WorkwearItems, Has.Count.EqualTo(3));
			var item1 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools);
			Assert.That(item1.ActiveNormItem, Is.EqualTo(norm1item1));
			var item2 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools2);
			Assert.That(item2.ActiveNormItem, Is.EqualTo(norm2item1));
			var item3 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools3);
			Assert.That(item3.ActiveNormItem, Is.EqualTo(norm2item2));
		}
		
		[Test(Description = "Проверяем что корректно объединяем 2 нормы, при срабатывании доп условий у одной из норм")]
		public void UpdateWorkwearItems_MergeTwoNorms_ConditionCase()
		{
			var employee = new EmployeeCard();
			
			var protectionTools = Substitute.For<ProtectionTools>();
			var protectionTools2 = Substitute.For<ProtectionTools>();
			var protectionTools3 = Substitute.For<ProtectionTools>();
			
			var failCondition = Substitute.For<NormCondition>();
			failCondition.MatchesForEmployee(employee).Returns(false);
			
			var norm1 = Substitute.For<Norm>();
			var norm1item1 = Substitute.For<NormItem>();
			norm1item1.ProtectionTools.Returns(protectionTools);
			norm1item1.NormCondition.ReturnsNull();
			var norm1item2 = Substitute.For<NormItem>();
			norm1item2.ProtectionTools.Returns(protectionTools2);
			norm1item2.AmountPerYear.Returns(5);
			norm1item2.NormCondition.Returns(failCondition);
			norm1.Items.Returns(new List<NormItem> { norm1item1, norm1item2 });
			
			var norm2 = Substitute.For<Norm>();
			var norm2item1 = Substitute.For<NormItem>();
			norm2item1.ProtectionTools.Returns(protectionTools2);
			norm2item1.AmountPerYear.Returns(2);
			norm2item1.NormCondition.ReturnsNull();
			var norm2item2 = Substitute.For<NormItem>();
			norm2item2.ProtectionTools.Returns(protectionTools3);
			norm2item2.NormCondition.ReturnsNull();
			norm2.Items.Returns(new List<NormItem> { norm2item1, norm2item2 });
			
			employee.AddUsedNorm(norm1);
			employee.AddUsedNorm(norm2);

			//Происходит автоматически
			//employee.UpdateWorkwearItems();
			Assert.That(employee.WorkwearItems, Has.Count.EqualTo(3));
			var item1 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools);
			Assert.That(item1.ActiveNormItem, Is.EqualTo(norm1item1));
			var item2 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools2);
			Assert.That(item2.ActiveNormItem, Is.EqualTo(norm2item1));
			var item3 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools3);
			Assert.That(item3.ActiveNormItem, Is.EqualTo(norm2item2));
		}
		
		[Test(Description = "Проверяем что корректно объединяем 2 нормы, при срабатывании доп условий у одной из норм. (Как предыдущий но в обратной последовательности обработки норм)")]
		public void UpdateWorkwearItems_MergeTwoNorms_Condition_ReverseCase()
		{
			var employee = new EmployeeCard();
			
			var protectionTools = Substitute.For<ProtectionTools>();
			var protectionTools2 = Substitute.For<ProtectionTools>();
			var protectionTools3 = Substitute.For<ProtectionTools>();
			
			var failCondition = Substitute.For<NormCondition>();
			failCondition.MatchesForEmployee(employee).Returns(false);
			
			var norm1 = Substitute.For<Norm>();
			var norm1item1 = Substitute.For<NormItem>();
			norm1item1.ProtectionTools.Returns(protectionTools);
			norm1item1.NormCondition.ReturnsNull();
			var norm1item2 = Substitute.For<NormItem>();
			norm1item2.ProtectionTools.Returns(protectionTools2);
			norm1item2.AmountPerYear.Returns(5);
			norm1item2.NormCondition.Returns(failCondition);
			norm1.Items.Returns(new List<NormItem> { norm1item1, norm1item2 });
			
			var norm2 = Substitute.For<Norm>();
			var norm2item1 = Substitute.For<NormItem>();
			norm2item1.ProtectionTools.Returns(protectionTools2);
			norm2item1.AmountPerYear.Returns(2);
			norm2item1.NormCondition.ReturnsNull();
			var norm2item2 = Substitute.For<NormItem>();
			norm2item2.ProtectionTools.Returns(protectionTools3);
			norm2item2.NormCondition.ReturnsNull();
			norm2.Items.Returns(new List<NormItem> { norm2item1, norm2item2 });
			
			employee.AddUsedNorm(norm2);
			employee.AddUsedNorm(norm1);

			//Происходит автоматически
			//employee.UpdateWorkwearItems();
			Assert.That(employee.WorkwearItems, Has.Count.EqualTo(3));
			var item1 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools);
			Assert.That(item1.ActiveNormItem, Is.EqualTo(norm1item1));
			var item2 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools2);
			Assert.That(item2.ActiveNormItem, Is.EqualTo(norm2item1));
			var item3 = employee.WorkwearItems.First(x => x.ProtectionTools == protectionTools3);
			Assert.That(item3.ActiveNormItem, Is.EqualTo(norm2item2));
		}
		
		[Test(Description = "Проверяем что при добавлении нормы обновляется потребность с учётом ограничения по полу ")]
		[TestCase(Sex.None, SexNormCondition.ForAll, ExpectedResult = 1)]
		[TestCase(Sex.F, SexNormCondition.ForAll, ExpectedResult = 1)]
		[TestCase(Sex.M, SexNormCondition.ForAll, ExpectedResult = 1)]
		[TestCase(Sex.None, SexNormCondition.OnlyMen, ExpectedResult = 0)]
		[TestCase(Sex.F, SexNormCondition.OnlyMen, ExpectedResult = 0)]
		[TestCase(Sex.M, SexNormCondition.OnlyMen, ExpectedResult = 1)]
		[TestCase(Sex.None, SexNormCondition.OnlyWomen, ExpectedResult = 0)]
		[TestCase(Sex.F, SexNormCondition.OnlyWomen, ExpectedResult = 1)]
		[TestCase(Sex.M, SexNormCondition.OnlyWomen, ExpectedResult = 0)]
		public int UpdateWorkwearItems_SexCondition(Sex employeeSex, SexNormCondition sexNormCondition) {
			
			var protectionTools = Substitute.For<ProtectionTools>();
			var norm = new Norm();
			var normCondition = new NormCondition() { SexNormCondition = sexNormCondition };
			norm.AddItem(protectionTools).NormCondition = normCondition;
			var employee = new EmployeeCard() { Sex = employeeSex };
			employee.AddUsedNorm(norm);

			return employee.WorkwearItems.Count;
		}
		
		[Test(Description = "Проверяем что наличие дефолтного условия нормы не влияет на количество потребностей.")]
		[TestCase(Sex.None)]
		[TestCase(Sex.F)]
		[TestCase(Sex.M)]
		public void WorkwearItemsCount_WithCondition_equal_NotCondition(Sex employeeSex) {
			
			var protectionTools = Substitute.For<ProtectionTools>();
			var normWithoutCondition = new Norm();
			normWithoutCondition.AddItem(protectionTools);
			var employeeWithoutCondition = new EmployeeCard() { Sex = employeeSex };
			employeeWithoutCondition.AddUsedNorm(normWithoutCondition);
			
			var normWithCondition = new Norm();
			normWithCondition.AddItem(protectionTools).NormCondition = new NormCondition();
			var employeeWithCondition = new EmployeeCard() { Sex = employeeSex };
			employeeWithCondition.AddUsedNorm(normWithCondition);

			Assert.That(employeeWithCondition.WorkwearItems.Count, Is.EqualTo(employeeWithoutCondition.WorkwearItems.Count));
		}
		
		[Test(Description = "Проверяем что при добавлении в норму обновляется количество потребностей")]
		public void UpdateWorkwearItems_AddItemToNorm() {
			
			var norm = new Norm();
			var protectionTools1 = Substitute.For<ProtectionTools>();
			norm.AddItem(protectionTools1);
			var employee = new EmployeeCard();
			employee.AddUsedNorm(norm);
			var before = employee.WorkwearItems.Count;
			
			var protectionTools2 = Substitute.For<ProtectionTools>();
			norm.AddItem(protectionTools2);
			employee.UpdateWorkwearItems();
			var after = employee.WorkwearItems.Count;
			
			Assert.That(after - before, Is.EqualTo(1));
		}
		
		[Test(Description = "Проверяем что при удалении из нормы обновляется количество потребностей")]
		public void UpdateWorkwearItems_RemoveItemFromNorm() {
			
			var norm = new Norm();
			var protectionTools1 = Substitute.For<ProtectionTools>();
			norm.AddItem(protectionTools1);
			var protectionTools2 = Substitute.For<ProtectionTools>();
			var secondItem = norm.AddItem(protectionTools2);
			var employee = new EmployeeCard();
			employee.AddUsedNorm(norm);
			var before = employee.WorkwearItems.Count;
			
			norm.RemoveItem(secondItem);
			employee.UpdateWorkwearItems();
			var after = employee.WorkwearItems.Count;
			
			Assert.That(after - before, Is.EqualTo(-1));
		}
	}
}
