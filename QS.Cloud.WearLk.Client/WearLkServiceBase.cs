using System;
using Grpc.Core;

namespace QS.Cloud.WearLk.Client
{
	public class WearLkServiceBase : IDisposable
	{
		private static readonly string ServiceAddress = "lk.wear.cloud.qsolution.ru";
		private static readonly int ServicePort = 4201;
		
		private readonly string sessionId;

		protected readonly Metadata Headers;
        
		public WearLkServiceBase(string sessionId)
		{
			this.sessionId = sessionId;
			Headers = new Metadata {{"Authorization", $"Bearer {sessionId}"}};
		}

		private Channel channel;
		protected Channel Channel {
			get {
				if(channel == null || channel.State == ChannelState.Shutdown)
					channel = new Channel(ServiceAddress, ServicePort, ChannelCredentials.Insecure);
				if (channel.State == ChannelState.TransientFailure)
					channel.ConnectAsync();
				return channel;
			}
		}
		
		public virtual void Dispose()
		{
			channel?.ShutdownAsync().Wait();
		}
	}
}
