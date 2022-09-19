using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using QS.DomainModel.UoW;
using RglibInterop;
using Workwear.Domain.Company;

namespace workwear.Tools.IdentityCards
{
	public class VirtualCardReaderService : ICardReaderService
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public VirtualCardReaderService(IUnitOfWorkFactory unitOfWorkFactory)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public List<DeviceInfo> Devices { get; private set; }

		public void RefreshDevices()
		{
			var device1 = new DeviceInfo(
				new RglibInterop.RG_ENDPOINT_INFO {
					Address = "0000000",
					FriendlyName = "Случайные карты",
					PortType = RglibInterop.RG_ENDPOINT_TYPE.PT_USBHID
				},
				new RglibInterop.RG_DEVICE_INFO_SHORT {
					DeviceAddress = 0,
				}
			);

			var device2 = new DeviceInfo(
				new RglibInterop.RG_ENDPOINT_INFO {
					Address = "0000001",
					FriendlyName = "Сотрудники из базы",
					PortType = RglibInterop.RG_ENDPOINT_TYPE.PT_USBHID
				},
				new RglibInterop.RG_DEVICE_INFO_SHORT {
					DeviceAddress = 1,
				}
			);

			Devices = new List<DeviceInfo> { device1, device2 };
		}

		public BindingList<CardType> CardFamilies { get; } = new BindingList<CardType>() {
				new CardType(RG_CARD_FAMILY_CODE.CF_COTAG),
				new CardType(RG_CARD_FAMILY_CODE.CF_EMMARINE),
				new CardType(RG_CARD_FAMILY_CODE.CF_HID),
				new CardType(RG_CARD_FAMILY_CODE.CF_INDALA),
				new CardType(RG_CARD_FAMILY_CODE.CF_PINCODE),
				new CardType(RG_CARD_FAMILY_CODE.CF_TEMIC),
				new CardType(RG_CARD_FAMILY_CODE.EF_MIFARE)
		};

		public void StartDevice(DeviceInfo device)
		{
			if(device.DeviceInfoShort.DeviceAddress == 1) {
				using(var uow = unitOfWorkFactory.CreateWithoutRoot()) {
					Uids = uow.Session.QueryOver<EmployeeCard>()
						.Where(x => x.CardKey != null)
						.Select(x => x.CardKey)
						.List<string>();
				}
			}
		}

		private IList<string> Uids;

		#region Авто опрос
		public event EventHandler<CardStateEventArgs> СardStatusRead;

		public bool IsAutoPoll { get; private set; }

		private Timer AutoPollTimer;
		private DeviceInfo AutoPullDevice;
		private Random random = new Random();
		private byte[] lastUid = new byte[8];
		private bool stateNoCard;
		private uint ticksLeft;
		private uint step = 0;

		public void StartAutoPoll(DeviceInfo deviceInfo)
		{
			if(IsAutoPoll)
				return;

			IsAutoPoll = true;
			AutoPullDevice = deviceInfo;
			AutoPollTimer = new Timer(200);
			AutoPollTimer.Elapsed += AutoPollTimer_Elapsed;
			AutoPollTimer.Enabled = true;
		}

		void AutoPollTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			RG_ENDPOINT portEndpoin = AutoPullDevice.Endpoint;
			byte address = AutoPullDevice.DeviceInfoShort.DeviceAddress;

			RG_PIN_SATETS_16 pinStates = new RG_PIN_SATETS_16();
			RG_DEVICE_STATUS_TYPE statusType = RG_DEVICE_STATUS_TYPE.DS_UNKNONWN;
			RG_CARD_INFO cardInfo = new RG_CARD_INFO() { CardType = RG_CARD_TYPE_CODE.CT_UNKNOWN };
			RG_CARD_MEMORY cardMemory = new RG_CARD_MEMORY() {
				ProfileBlock = 0,
				MemBlock = new byte[16]
			};

			if(ticksLeft <= 0) {
				if(stateNoCard) {
					stateNoCard = false;
					if(AutoPullDevice.DeviceInfoShort.DeviceAddress == 0)
						random.NextBytes(lastUid);
					else if(AutoPullDevice.DeviceInfoShort.DeviceAddress == 1) {
						NextStepReader1();
					}
					ticksLeft = 10;
				}
				else {
					stateNoCard = true;
					ticksLeft = 25;
				}
			}
			else
				ticksLeft--;

			if(stateNoCard) {
				statusType = RG_DEVICE_STATUS_TYPE.DS_NOCARD;
			}
			else {
				statusType = RG_DEVICE_STATUS_TYPE.DS_CARD;
				cardInfo.CardUid = lastUid;
			}

			var result = new CardStateEventArgs(pinStates, statusType, cardInfo, cardMemory);
			СardStatusRead?.Invoke(this, result);
		}

		private string step1Uid;

		private void NextStepReader1()
		{
			step++;
			logger.Debug($"Сценарий виртуального картридера: Шаг {step}");
			//Карточка нового сотрудника
			if(step == 1) {
				step1Uid = Uids[random.Next(Uids.Count)];
				lastUid = RusGuardService.UidToBytes(step1Uid);
			}
			//Неправильная попытка подтвердить другой карточкой.
			if(step == 2) {
				var strUid = Uids[random.Next(Uids.Count)];
				lastUid = RusGuardService.UidToBytes(strUid);
			}
			//Подтверждение правильной карточкой.
			if(step == 3) {
				lastUid = RusGuardService.UidToBytes(step1Uid);
				step = 0;
			}
		}

		public void StopAutoPoll()
		{
			IsAutoPoll = false;
			AutoPollTimer.Enabled = false;
		}

		public void Dispose()
		{
			if(IsAutoPoll)
				StopAutoPoll();
		}

		#endregion
	}
}
