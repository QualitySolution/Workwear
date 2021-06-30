using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace workwear.Tools.IdentityCards
{
	public interface ICardReaderService : IDisposable
	{
		event EventHandler<CardStateEventArgs> СardStatusRead;

		List<DeviceInfo> Devices { get; }
		BindingList<CardType> CardFamilies { get; }
		bool IsAutoPoll { get; }

		void RefreshDevices();
		void StartDevice(DeviceInfo device);
		void StartAutoPoll(DeviceInfo deviceInfo);
		void StopAutoPoll();
	}
}
