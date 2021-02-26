using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Testing.Gui;
using RglibInterop;
using workwear.Tools.IdentityCards;
using workwear.ViewModels.Stock;

namespace WorkwearTest.ViewModels.Stock
{
	[TestFixture(TestOf = typeof(IssueByIdentifierViewModel))]
	public class IssueByIdentifierViewModelTest
	{
		[Test(Description = "Проверяем что отображаем номер карты в строке состояния считывателя")]
		public void CurrentState_CardUid()
		{
			var uowFactory = Substitute.For<IUnitOfWorkFactory>();
			var navigation = Substitute.For<INavigationManager>();
			var guiDispatcher = new GuiDispatcherForTests();
			var device = Substitute.For<DeviceInfo>(new RG_ENDPOINT_INFO(), new RG_DEVICE_INFO_SHORT());
			var cardReaderService = Substitute.For<ICardReaderService>();
			cardReaderService.Devices.Returns(new List<DeviceInfo> { device });

			bool UidCardWasСalled = false, CurentStateWasСalled =false;
			var viewModel = new IssueByIdentifierViewModel(uowFactory, navigation, guiDispatcher, cardReaderService);
			viewModel.SelectedDevice = viewModel.Devices.First();
			viewModel.CardFamilies.First(x => x.CardTypeFamily == RG_CARD_FAMILY_CODE.EF_MIFARE).Active = true;
			cardReaderService.IsAutoPoll.Returns(true);

			viewModel.PropertyChanged += delegate (object sender, System.ComponentModel.PropertyChangedEventArgs e) {
				if(e.PropertyName == nameof(viewModel.CardUid)) {
					UidCardWasСalled = true;
				}
				if(e.PropertyName == nameof(viewModel.CurrentState)) {
					CurentStateWasСalled = true;
				}
			};

			cardReaderService.СardStatusRead += Raise.EventWith(new CardStateEventArgs(
				new RG_PIN_SATETS_16 { Pin00 = true, Pin01 = true },
				RG_DEVICE_STATUS_TYPE.DS_CARD,
				new RG_CARD_INFO { CardType = RG_CARD_TYPE_CODE.CT_MF_CL1K_PL2K, CardUid = new byte[] {0x80, 0x31, 0x3E, 0x3A, 0x4A, 0x99, 0x04 } },
				new RG_CARD_MEMORY()
				));

			Assert.That(UidCardWasСalled, Is.True, $"Событие изменения { nameof(viewModel.CardUid)} не вызвано");
			Assert.That(CurentStateWasСalled, Is.True, $"Событие изменения { nameof(viewModel.CurrentState)} не вызвано");
			Assert.That(viewModel.CardUid, Is.EqualTo("80-31-3E-3A-4A-99-04"));
			Assert.That(viewModel.CurrentState.StartsWith("Карта: 80-31-3E-3A-4A-99-04"), Is.True);
		}
	}
}
