using System;
using RglibInterop;

namespace workwear.Tools.IdentityCards
{
	public class DeviceInfo
	{
		public readonly RG_ENDPOINT_INFO EndpointInfo;
		public readonly RG_DEVICE_INFO_SHORT DeviceInfoShort;

		public DeviceInfo(RG_ENDPOINT_INFO endpointInfo, RG_DEVICE_INFO_SHORT deviceInfoShort)
		{
			this.EndpointInfo = endpointInfo;
			this.DeviceInfoShort = deviceInfoShort;
		}

		public string Title => string.Format("{0}:{2} [{1}]", EndpointInfo.Address, EndpointInfo.FriendlyName, DeviceInfoShort.DeviceAddress);

		public RG_ENDPOINT Endpoint => new RG_ENDPOINT {
				Type = EndpointInfo.PortType,
				Address = EndpointInfo.Address 
		};
	}
}
