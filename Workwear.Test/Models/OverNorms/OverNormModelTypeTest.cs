using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Models;

namespace Workwear.Test.Models.OverNorms {
	[TestFixture]
	public class OverNormModelTypeTest {
		public static IEnumerable<TestCaseData> ModelCases {
			get {
				yield return new TestCaseData(
					OverNormType.Simple,
					new Func<IUnitOfWork, OverNormModelBase>(uow => new SimpleModel(uow) { UseBarcodes = true }))
					.SetName("SimpleModel");
				yield return new TestCaseData(
					OverNormType.Guest,
					new Func<IUnitOfWork, OverNormModelBase>(uow => new GuestModel(uow)))
					.SetName("GuestModel");
				yield return new TestCaseData(
					OverNormType.Substitute,
					new Func<IUnitOfWork, OverNormModelBase>(uow => new SubstituteFundModel(uow)))
					.SetName("SubstituteFundModel");
			}
		}

		[TestCaseSource(nameof(ModelCases))]
		public void CreateDocument_SetsOperationType(OverNormType expectedType, Func<IUnitOfWork, OverNormModelBase> createModel) {
			var model = createModel(Substitute.For<IUnitOfWork>());
			var param = CreateParam(expectedType);

			var document = model.CreateDocument(new[] { param }, new Warehouse());

			Assert.That(document.Type, Is.EqualTo(expectedType));
			Assert.That(document.Items.Select(x => x.OverNormOperation.Type), Is.All.EqualTo(expectedType));
		}

		[TestCaseSource(nameof(ModelCases))]
		public void CreateDocument_FillsWarehouseAndOverNormOperationStockPosition(
			OverNormType expectedType,
			Func<IUnitOfWork, OverNormModelBase> createModel)
		{
			var model = createModel(Substitute.For<IUnitOfWork>());
			var param = CreateParam(expectedType);

			var document = model.CreateDocument(new[] { param }, new Warehouse());
			var operation = document.Items.Single().OverNormOperation;

			AssertOperationStockPosition(operation, param);
		}

		[TestCaseSource(nameof(ModelCases))]
		public void AddOperation_SetsOperationType(OverNormType expectedType, Func<IUnitOfWork, OverNormModelBase> createModel) {
			var model = createModel(Substitute.For<IUnitOfWork>());
			var document = new OverNorm { Warehouse = new Warehouse() };

			model.AddOperation(document, CreateParam(expectedType), document.Warehouse);

			Assert.That(document.Items.Select(x => x.OverNormOperation.Type), Is.All.EqualTo(expectedType));
		}

		[TestCaseSource(nameof(ModelCases))]
		public void UpdateOperation_SetsOperationType(OverNormType expectedType, Func<IUnitOfWork, OverNormModelBase> createModel) {
			var model = createModel(Substitute.For<IUnitOfWork>());
			var document = new OverNorm { Warehouse = new Warehouse() };
			var operation = new OverNormOperation {
				Type = expectedType == OverNormType.Simple ? OverNormType.Guest : OverNormType.Simple,
				Employee = new EmployeeCard()
			};
			var item = document.AddItem(operation);

			model.UpdateOperation(item, CreateParam(expectedType));

			Assert.That(operation.Type, Is.EqualTo(expectedType));
		}

		[TestCaseSource(nameof(ModelCases))]
		public void UpdateOperation_FillsWarehouseAndOverNormOperationStockPosition(
			OverNormType expectedType,
			Func<IUnitOfWork, OverNormModelBase> createModel)
		{
			var model = createModel(Substitute.For<IUnitOfWork>());
			var document = new OverNorm { Warehouse = new Warehouse() };
			var operation = new OverNormOperation {
				Type = expectedType,
				Employee = new EmployeeCard()
			};
			var item = document.AddItem(operation);
			var param = CreateParam(expectedType);

			model.UpdateOperation(item, param);

			AssertOperationStockPosition(operation, param);
		}

		private static OverNormParam CreateParam(OverNormType type) {
			var employee = new EmployeeCard();
			var itemType = new ItemsType();
			var nomenclature = new Nomenclature { Type = itemType };
			var size = new Size { Id = 10, Name = "52" };
			var height = new Size { Id = 11, Name = "182" };
			var barcode = new Barcode {
				Nomenclature = nomenclature,
				Size = size,
				Height = height
			};
			var employeeIssueOperation = type == OverNormType.Substitute
				? new EmployeeIssueOperation {
					Employee = employee,
					Nomenclature = nomenclature
				}
				: null;

			return new OverNormParam(
				employee,
				nomenclature,
				1,
				size,
				height,
				employeeIssueOperation: employeeIssueOperation,
				barcodes: new List<Barcode> { barcode },
				wearPercent: 0.3m);
		}

		private static void AssertOperationStockPosition(OverNormOperation operation, OverNormParam param)
		{
			Assert.That(operation.Nomenclature, Is.SameAs(param.Nomenclature));
			Assert.That(operation.WearSize, Is.SameAs(param.Size));
			Assert.That(operation.Height, Is.SameAs(param.Height));
			Assert.That(operation.WearPercent, Is.EqualTo(param.WearPercent));
			Assert.That(operation.WarehouseOperation.Nomenclature, Is.SameAs(param.Nomenclature));
			Assert.That(operation.WarehouseOperation.WearSize, Is.SameAs(param.Size));
			Assert.That(operation.WarehouseOperation.Height, Is.SameAs(param.Height));
			Assert.That(operation.WarehouseOperation.WearPercent, Is.EqualTo(param.WearPercent));
		}
	}
}
