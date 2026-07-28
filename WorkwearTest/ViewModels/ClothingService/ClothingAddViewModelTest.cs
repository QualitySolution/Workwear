using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Testing.DB;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Company;
using Workwear.Domain.Postomats;
using Workwear.Domain.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.ViewModels.ClothingService;
using Workwear.ViewModels.Postomats;

namespace WorkwearTest.ViewModels.ClothingService {
	[TestFixture(TestOf = typeof(ClothingAddViewModel))]
	public class ClothingAddViewModelTest : InMemoryDBGlobalConfigTestFixtureBase {
		[OneTimeSetUp]
		public void Init() {
			ConfigureOneTime.ConfigureNh();
			InitialiseUowFactory();
		}

		[Test(Description = "При сканировании штрихкода в документ постамата должна добавляться активная заявка на обслуживание.")]
		[Category("Integrated")]
		public void ScanBarcode_ForPostomatDocument_AddsActiveServiceClaim()
		{
			NewSessionWithSameDB();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var barcode = new Barcode {
					Title = "000000000001",
					Nomenclature = new Nomenclature {
						Name = "Куртка",
						Type = new ItemsType { Name = "Одежда" }
					}
				};
				var employee = new EmployeeCard {
					FirstName = "Иван",
					LastName = "Иванов"
				};
				var claim = new ServiceClaim {
					Employee = employee,
					Barcode = barcode,
					IsClosed = false
				};
				uow.Save(barcode.Nomenclature.Type);
				uow.Save(barcode.Nomenclature);
				uow.Save(barcode);
				uow.Save(employee);
				uow.Save(claim);
				uow.Commit();

				var unitOfWorkProvider = new UnitOfWorkProvider();
				var barcodeRepository = new BarcodeRepository(unitOfWorkProvider);
				var barcodeInfoViewModel = new BarcodeInfoViewModel(
					barcodeRepository,
					Substitute.For<BaseParameters>());
				var postomatDocumentViewModel = CreatePostomatDocumentViewModelStub();
				var viewModel = new ClothingAddViewModel(
					barcodeRepository,
					barcodeInfoViewModel,
					UnitOfWorkFactory,
					Substitute.For<INavigationManager>(),
					postomatDocVm: postomatDocumentViewModel,
					null,
					null,
					unitOfWorkProvider: unitOfWorkProvider
					);

				barcodeInfoViewModel.Barcode = viewModel.UoW.GetById<Barcode>(barcode.Id);

				Assert.That(viewModel.ActiveClaim?.Id, Is.EqualTo(claim.Id));
				Assert.That(viewModel.Claims.Select(x => x.Id), Is.EqualTo(new[] { claim.Id }));
				Assert.That(viewModel.CanAdd, Is.False, "Заявка уже автодобавлена в список.");
			}
		}

		private static PostomatDocumentViewModel CreatePostomatDocumentViewModelStub()
		{
			var viewModel = (PostomatDocumentViewModel)FormatterServices.GetUninitializedObject(typeof(PostomatDocumentViewModel));
			var uow = Substitute.For<IUnitOfWorkGeneric<PostomatDocument>>();
			uow.Root.Returns(new PostomatDocument());
			typeof(PostomatDocumentViewModel)
				.BaseType
				.GetProperty(nameof(PostomatDocumentViewModel.UoWGeneric), BindingFlags.Instance | BindingFlags.Public)
				.GetSetMethod(true)
				.Invoke(viewModel, new object[] { uow });
			return viewModel;
		}
	}
}
