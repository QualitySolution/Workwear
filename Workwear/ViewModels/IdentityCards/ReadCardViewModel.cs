using System;
using System.Linq;
using QS.Configuration;
using QS.Dialog;
using QS.Navigation;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;
using workwear.Tools.IdentityCards;

namespace Workwear.ViewModels.IdentityCards
{
	public class ReadCardViewModel : WindowDialogViewModelBase
	{
		private readonly ICardReaderService cardReaderService;
		private readonly IGuiDispatcher guiDispatcher;
		private readonly IChangeableConfiguration configuration;
		private TextSpinner textSpinner = new TextSpinner(new SpinnerTemplateAestheticScrolling());

		public ReadCardViewModel(
			INavigationManager navigation,
			ICardReaderService cardReaderService,
			IGuiDispatcher guiDispatcher,
			IChangeableConfiguration configuration) : base(navigation)
		{
			IsModal = true;
			Title = "Чтение карты";
			this.cardReaderService = cardReaderService ?? throw new ArgumentNullException(nameof(cardReaderService));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			StartReading();
		}

		#region Свойства

		private string status;
		public virtual string Status {
			get => status;
			set => SetField(ref status, value);
		}

		public string StatusColor { get; private set; }

		public bool SensetiveSaveButton => !String.IsNullOrEmpty(CardUid);

		public string CardUid { get; private set; }


		#endregion

		#region Действия

		public void Apply()
		{
			Close(false, CloseSource.Save);
		}

		#endregion

		#region Считыватель

		void StartReading()
		{
			DeviceInfo device = null;
			cardReaderService.RefreshDevices();

			var readerAddress = configuration["CardReader:Address"];
			if(!String.IsNullOrEmpty(readerAddress) && readerAddress.Contains(':')) {
				var parts = readerAddress.Split(':');
				device = cardReaderService.Devices.First(x => x.Endpoint.Address == parts[0] && x.DeviceInfoShort.DeviceAddress == int.Parse(parts[1]));
			}

			if(device == null)
				device = cardReaderService.Devices.FirstOrDefault();

			if(device == null) {
				UpdateState(null);
				return;
			}

			var cardTypes = configuration["CardReader:CardTypes"];
			if(!String.IsNullOrEmpty(cardTypes)) {
				var parts = cardTypes.Split(',');
				foreach(var part in parts) {
					var family = cardReaderService.CardFamilies.FirstOrDefault(x => x.CardTypeFamily.ToString() == part);
					if(family != null)
						family.Active = true;
				}
			}

			cardReaderService.СardStatusRead += RusGuardService_СardStatusRead;
			cardReaderService.StartDevice(device);
			cardReaderService.StartAutoPoll(device);
		}

		void RusGuardService_СardStatusRead(object sender, CardStateEventArgs e)
		{
			guiDispatcher.RunInGuiTread(delegate {
				UpdateState(e.CardUid);
			});
		}

		void UpdateState(string carduid)
		{
			if(cardReaderService == null) {
				StatusColor = "Dark Red";
				Status = "Библиотека RusGuard не загружена";
				return;
			}
			if(!cardReaderService.Devices.Any()) {
				StatusColor = "Dark Red";
				Status = "Считыватели не подключены";
				return;
			}
			if(!cardReaderService.CardFamilies.Any(x => x.Active)) {
				Status = "Не выбран тип карт";
				StatusColor = "Dark Cyan";
				return;
			}

			if(!String.IsNullOrEmpty(carduid)) {
				CardUid = carduid;
				OnPropertyChanged(nameof(SensetiveSaveButton));
			}

			if(!String.IsNullOrEmpty(CardUid)) {
				Status = $"Карта: {CardUid}\n" + textSpinner.GetFrame();
				StatusColor = String.IsNullOrEmpty(carduid) ? "Dark Green" : "Lime Green";
				return;
			}
			else {
				Status = "Приложите карту\n" + textSpinner.GetFrame();
				StatusColor = "Dark Salmon";
			}
		}

		#endregion
	}
}
