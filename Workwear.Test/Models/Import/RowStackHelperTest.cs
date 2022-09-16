using System.Collections.Generic;
using NPOI.SS.UserModel;
using NSubstitute;
using NUnit.Framework;
using Workwear.Models.Import;

namespace Workwear.Test.Models.Import {
	[TestFixture(TestOf = typeof(RowStackHelper))]
	public class RowStackHelperTest {
		[Test(Description = "Пустой стек")]
		public void NewPow_EmptyStack() {
			var stack = new Stack<IRow>();
			var row1 = Substitute.For<IRow>();
			row1.OutlineLevel.Returns(0);
			RowStackHelper.NewRow(stack, row1);
			Assert.That(stack, Has.Count.EqualTo(1));
		}
		
		[Test(Description = "Новая строка того же уровня")]
		public void NewPow_SameLevel() {
			var stack = new Stack<IRow>();
			var row1 = Substitute.For<IRow>();
			row1.OutlineLevel.Returns(0);
			stack.Push(row1);
			var newRow = Substitute.For<IRow>();
			newRow.OutlineLevel.Returns(0);
			RowStackHelper.NewRow(stack, newRow);
			Assert.That(stack, Has.Count.EqualTo(1));
			Assert.That(stack.Peek(), Is.EqualTo(newRow));
		}

		
		[Test(Description = "Новая строка уровня ниже")]
		public void NewPow_OneBelowLevel() {
			var stack = new Stack<IRow>();
			var row1 = Substitute.For<IRow>();
			row1.OutlineLevel.Returns(0);
			stack.Push(row1);
			var newRow = Substitute.For<IRow>();
			newRow.OutlineLevel.Returns(1);
			RowStackHelper.NewRow(stack, newRow);
			Assert.That(stack, Has.Count.EqualTo(2));
			Assert.That(stack.Peek(), Is.EqualTo(newRow));
		}
		
		[Test(Description = "Новая строка уровня ниже")]
		public void NewPow_ThreeBelowLevel() {
			var stack = new Stack<IRow>();
			var row1 = Substitute.For<IRow>();
			row1.OutlineLevel.Returns(0);
			stack.Push(row1);
			var newRow = Substitute.For<IRow>();
			newRow.OutlineLevel.Returns(3);
			RowStackHelper.NewRow(stack, newRow);
			Assert.That(stack, Has.Count.EqualTo(4));
			Assert.That(stack.Pop(), Is.EqualTo(newRow));
			Assert.That(stack.Pop(), Is.EqualTo(newRow));
			Assert.That(stack.Pop(), Is.EqualTo(newRow));
		}
		
		[Test(Description = "Новая строка уровня выше")]
		public void NewPow_OneUpLevel() {
			var stack = new Stack<IRow>();
			var row1 = Substitute.For<IRow>();
			row1.OutlineLevel.Returns(0);
			stack.Push(row1);
			var row2 = Substitute.For<IRow>();
			row2.OutlineLevel.Returns(1);
			stack.Push(row2);
			var row3 = Substitute.For<IRow>();
			row3.OutlineLevel.Returns(2);
			stack.Push(row3);
			var newRow = Substitute.For<IRow>();
			newRow.OutlineLevel.Returns(1);
			RowStackHelper.NewRow(stack, newRow);
			Assert.That(stack, Has.Count.EqualTo(2));
			Assert.That(stack.Peek(), Is.EqualTo(newRow));
		}
		
		[Test(Description = "Новая строка уровня выше")]
		public void NewPow_ThreeUpLevel() {
			var stack = new Stack<IRow>();
			var row1 = Substitute.For<IRow>();
			row1.OutlineLevel.Returns(0);
			stack.Push(row1);
			var row2 = Substitute.For<IRow>();
			row2.OutlineLevel.Returns(1);
			stack.Push(row2);
			var row3 = Substitute.For<IRow>();
			row3.OutlineLevel.Returns(2);
			stack.Push(row3);
			var newRow = Substitute.For<IRow>();
			newRow.OutlineLevel.Returns(0);
			RowStackHelper.NewRow(stack, newRow);
			Assert.That(stack, Has.Count.EqualTo(1));
			Assert.That(stack.Peek(), Is.EqualTo(newRow));
		}
	}
}
