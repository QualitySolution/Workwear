using System;
using System.Collections.Generic;

namespace workwear.Tools.IdentityCards
{
	public interface ICardReaderService
	{
		event EventHandler<CardStateEventArgs> СardStatusRead;

		List<DeviceInfo> Devices { get; }
		bool IsAutoPoll { get; }

		void RefreshDevices();
		void SetCardMask(DeviceInfo device, IList<CardType> cardTypes);
		void StartDevice(DeviceInfo device);
		void StartAutoPoll(DeviceInfo deviceInfo);
		void StopAutoPoll();
	}
}
