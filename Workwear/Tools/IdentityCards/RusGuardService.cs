using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using RglibInterop;

namespace workwear.Tools.IdentityCards
{
	public class RusGuardService : RglibInterface
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public RusGuardService() : base("rglib_x86.dll")
		{
			logger.Debug("Инициализация библиотеки работы со считывателями RusGuard");
			RG_InitializeLib(0);
			uint libVersion = RG_GetVersion();
			logger.Info(string.Format("Библиотека RusGuard версии {0}.{1}.{2}",
					libVersion >> 24, (libVersion >> 16) & 0xFF, libVersion & 0xFFFF));
		}

		#region Данные
		public readonly List<DeviceInfo> Devices = new List<DeviceInfo>();
		#endregion

		#region Методы
		public void RefreshDevices()
		{
			Devices.Clear();
			IntPtr endPointsListHandle = IntPtr.Zero;
			uint endpointsCount = 0;
			bool findUsbFlag = true;
			bool findHidFlag = true;
			byte endPointsMask = (byte)(0 | (findUsbFlag ? 1 : 0) | (findHidFlag ? 2 : 0));
			if(endPointsMask > 0) {
				uint errorCode =
					RG_FindEndPoints(ref endPointsListHandle, endPointsMask, ref endpointsCount);
				if(errorCode != 0)
					throw new ApplicationException($"Ошибка {errorCode} при вызове RG_FindEndPoints");

				if(endPointsListHandle != IntPtr.Zero) {
					RG_ENDPOINT_INFO portInfo = new RG_ENDPOINT_INFO();
					uint portIndex = 0;
					while(RG_GetFoundEndPointInfo(endPointsListHandle, portIndex, ref portInfo) == 0) {
						//Считываем все устройство на точке подключения
						RG_ENDPOINT endpoint = new RG_ENDPOINT();
						endpoint.Type = portInfo.PortType;
						endpoint.Address = portInfo.Address;
						byte currentDeviceAddress = 0;
						while(currentDeviceAddress < 4) {
							errorCode = RG_InitDevice(ref endpoint, currentDeviceAddress);
							if(errorCode == 0) {
								RG_DEVICE_INFO_SHORT deviceInfo = new RG_DEVICE_INFO_SHORT();
								if(RG_GetInfo(ref endpoint, currentDeviceAddress, ref deviceInfo) == 0) {
									Devices.Add(new DeviceInfo(portInfo, deviceInfo));
								}
							}
							currentDeviceAddress++;
						}
						portIndex++;
					}
					RG_CloseResource(endPointsListHandle);
				}
			}
		}

		public void SetCardMask(DeviceInfo device, IList<CardType> cardTypes)
		{
			logger.Debug("Устанавливаем маску используемых карт.");
			byte mask = 0;
			foreach(var type in cardTypes.Where(x => x.Active)) {
				mask |= (byte)type.CardTypeFamily;
			}

			RG_ENDPOINT portEndpoin = device.Endpoint;
			byte address = device.DeviceInfoShort.DeviceAddress;
			uint errorCode = RG_SetCardsMask(ref portEndpoin, address, mask);
			if(errorCode != 0) {
				throw new ApplicationException($"Ошибка {errorCode} при установке маски карт");
			}
		}

		public void StartDevice(DeviceInfo device)
		{
			//Записываю Кодограмму
			RG_ENDPOINT portEndpoin = device.Endpoint;
			RG_CODOGRAMM codogram = new RG_CODOGRAMM {
				Length = 32,
				Body = 0x00000000
			};
			byte codogramNumber = 1;
			uint errorCode = RG_WriteCodogramm(ref portEndpoin, device.DeviceInfoShort.DeviceAddress, codogramNumber, ref codogram);
			if(errorCode != 0) {
				throw new ApplicationException($"Ошибка при записи кодограммы = {errorCode}");
			}
		}

		#endregion
		#region Авто опрос

		public bool IsAutoPoll;
		public event EventHandler<CardStateEventArgs> СardStatusRead;

		private Timer AutoPollTimer;
		private DeviceInfo AutoPullDevice;

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

			uint errorCode = RG_GetStatus(ref portEndpoin, address,
				ref statusType, ref pinStates, ref cardInfo, ref cardMemory);
			if(errorCode != 0 && statusType == RG_DEVICE_STATUS_TYPE.DS_UNKNONWN) {
				throw new ApplicationException($"Ошибка {errorCode} при запросе статуса устройства");
			}

			if(statusType != RG_DEVICE_STATUS_TYPE.DS_NOCARD)
				logger.Debug($"Прочитана карта: {cardInfo.CardUid}");
			var result = new CardStateEventArgs(pinStates, statusType, cardInfo, cardMemory);
			СardStatusRead?.Invoke(this, result);
		}

		public void StopAutoPoll()
		{
			IsAutoPoll = false;
			AutoPollTimer.Enabled = false;
		}

		#endregion
	}
}
