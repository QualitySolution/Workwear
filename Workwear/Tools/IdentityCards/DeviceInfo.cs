using System;
using RglibInterop;

namespace workwear.Tools.IdentityCards
{
	public class DeviceInfo
	{
		private readonly RG_ENDPOINT_INFO endpointInfo;
		private readonly RG_DEVICE_INFO_SHORT deviceInfoShort;

		public DeviceInfo(RG_ENDPOINT_INFO endpointInfo, RG_DEVICE_INFO_SHORT deviceInfoShort)
		{
			this.endpointInfo = endpointInfo;
			this.deviceInfoShort = deviceInfoShort;
		}

		public string Title => string.Format("{0}:{2} [{1}]", endpointInfo.Address, endpointInfo.FriendlyName, deviceInfoShort.DeviceAddress); 
	}
}
