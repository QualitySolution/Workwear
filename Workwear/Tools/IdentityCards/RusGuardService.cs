using System;
using System.Collections.Generic;
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
		#endregion
	}
}
