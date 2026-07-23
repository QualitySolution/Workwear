using System.Collections.Generic;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;
using QS.Configuration;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Testing.Gui;
using QS.Validation;
using QS.ViewModels.Resolve;
using RglibInterop;
using Workwear.Domain.Company;
using Workwear.Models.Operations;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;
using Workwear.Tools;
using workwear.Tools.IdentityCards;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock;

namespace WorkwearTest.ViewModels.Stock
{
	[TestFixture(TestOf = typeof(IssueByIdentifierViewModel))]
	public class IssueByIdentifierViewModelTest
	{
		[Test(Description = "Проверяем что отображаем номер карты в строке состояния считывателя")]
		public void CurrentState_CardUid()
		{
			var employeeRepository = Substitute.For<EmployeeRepository>((IUnitOfWork)null);
			employeeRepository.GetEmployeeByCardkey(Arg.Any<IUnitOfWork>(), Arg.Any<string>()).Returns((EmployeeCard)null);
			var device = Substitute.For<DeviceInfo>(new RG_ENDPOINT_INFO(), new RG_DEVICE_INFO_SHORT());
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var cardReaderService = Substitute.For<ICardReaderService>();
			cardReaderService.Devices.Returns(new List<DeviceInfo> { device });
			cardReaderService.CardFamilies.Returns(new VirtualCardReaderService(uowFactory).CardFamilies); //Тут тупо берем список из класса виртульного картридера.

			var builder = new ContainerBuilder();
			builder.RegisterType<IssueByIdentifierViewModel>().AsSelf();
			builder.RegisterType<UnitOfWorkProvider>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<EmployeeIssueModel>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<StockBalanceModel>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterType<EmployeeIssueRepository>().AsSelf().InstancePerLifetimeScope();
			builder.RegisterInstance(cardReaderService).As<ICardReaderService>();
			builder.RegisterInstance(employeeRepository).As<EmployeeRepository>();
			builder.Register(x => new GuiDispatcherForTests()).As<IGuiDispatcher>();
			builder.Register(x => Substitute.For<SizeService>()).As<SizeService>();
			builder.Register(x => Substitute.For<IValidator>()).As<IValidator>();
			builder.Register(x => Substitute.For<IChangeableConfiguration>()).As<IChangeableConfiguration>();
			builder.Register(x => Substitute.For<BaseParameters>()).As<BaseParameters>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			builder.Register(x => Substitute.For<FeaturesService>()).As<FeaturesService>();
			builder.Register(x => Substitute.For<StockRepository>()).As<StockRepository>();
			builder.Register(x => Substitute.For<IInteractiveQuestion>()).As<IInteractiveQuestion>();
			builder.Register(x => Substitute.For<IUnitOfWorkFactory>()).As<IUnitOfWorkFactory>();
			builder.Register(x => Substitute.For<INavigationManager>()).As<INavigationManager>();
			builder.Register(x => Substitute.For<IViewModelResolver>()).As<IViewModelResolver>();
			builder.Register(x => Substitute.For<IUserService>()).As<IUserService>();
			var container = builder.Build();

			using(var scope = container.BeginLifetimeScope()){
				bool uidCardWasCalled = false, currentStateWasCalled =false;
				var viewModel = scope.Resolve<IssueByIdentifierViewModel>();
				viewModel.SelectedDevice = viewModel.Devices.First();
				viewModel.CardFamilies.First(x => x.CardTypeFamily == RG_CARD_FAMILY_CODE.EF_MIFARE).Active = true;
				cardReaderService.IsAutoPoll.Returns(true);

				viewModel.PropertyChanged += delegate (object sender, System.ComponentModel.PropertyChangedEventArgs e) {
					if(e.PropertyName == nameof(viewModel.CardUid)) {
						uidCardWasCalled = true;
					}
					if(e.PropertyName == nameof(viewModel.CurrentState)) {
						currentStateWasCalled = true;
					}
				};

				cardReaderService.CardStatusRead += Raise.EventWith(new CardStateEventArgs(
					new RG_PIN_SATETS_16 { Pin00 = true, Pin01 = true },
					RG_DEVICE_STATUS_TYPE.DS_CARD,
					new RG_CARD_INFO { CardType = RG_CARD_TYPE_CODE.CT_MF_CL1K_PL2K, CardUid = new byte[] {0x80, 0x31, 0x3E, 0x3A, 0x4A, 0x99, 0x04 } },
					new RG_CARD_MEMORY()
					));

				Assert.That(uidCardWasCalled, Is.True, $"Событие изменения { nameof(viewModel.CardUid)} не вызвано");
				Assert.That(currentStateWasCalled, Is.True, $"Событие изменения { nameof(viewModel.CurrentState)} не вызвано");
				Assert.That(viewModel.CardUid, Is.EqualTo("80-31-3E-3A-4A-99-04"));
				Assert.That(viewModel.CurrentState.StartsWith("Карта: 80-31-3E-3A-4A-99-04"), Is.True);
			}
		}
	}
}
