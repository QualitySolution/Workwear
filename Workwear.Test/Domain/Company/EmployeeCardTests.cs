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

		[Test(Description = "Проверяем что работает обновление потребности с учётом ограничения нормы по полу")]
		public void UpdateWorkwearItems_SexCondition() {

			var conditionF = new NormCondition() { SexNormCondition = SexNormCondition.OnlyWomen };
			var conditionM = new NormCondition() { SexNormCondition = SexNormCondition.OnlyMen };
			var conditionN = new NormCondition() { SexNormCondition = SexNormCondition.ForAll };

			var normF = new Norm();
			var normM = new Norm();
			var normN = new Norm();
			var protectionToolsF = Substitute.For<ProtectionTools>();
			var protectionToolsM = Substitute.For<ProtectionTools>();	
			var protectionToolsN = Substitute.For<ProtectionTools>();	
			normF.AddItem(protectionToolsF).NormCondition = conditionF;
			normM.AddItem(protectionToolsM).NormCondition = conditionM;
			normN.AddItem(protectionToolsN).NormCondition = conditionN;
			
			var employeeF = new EmployeeCard() { Sex = Sex.F };
			var employeeM = new EmployeeCard() { Sex = Sex.M };
			var employeeN = new EmployeeCard() { Sex = Sex.None };
			
			employeeF.AddUsedNorm(normF); 
			employeeM.AddUsedNorm(normM);
			employeeN.AddUsedNorm(normN);
			//UpdateWorkwearItems() вызывается при добавлении
			Assert.That(employeeF.WorkwearItems, Has.Count.EqualTo(1));
			Assert.That(employeeM.WorkwearItems, Has.Count.EqualTo(1));
			Assert.That(employeeN.WorkwearItems, Has.Count.EqualTo(1));
			
			employeeF.AddUsedNorm(normM); employeeF.AddUsedNorm(normN);
			employeeM.AddUsedNorm(normF); employeeM.AddUsedNorm(normN);
			employeeN.AddUsedNorm(normF); employeeN.AddUsedNorm(normM); 
			//UpdateWorkwearItems() вызывается при добавлении
			Assert.That(employeeF.WorkwearItems, Has.Count.EqualTo(2));
			Assert.That(employeeM.WorkwearItems, Has.Count.EqualTo(2));
			Assert.That(employeeN.WorkwearItems, Has.Count.EqualTo(1));
		}
	}
}
