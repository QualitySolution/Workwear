using System.Collections.Generic;
using QS.Configuration;
using QS.Project.Versioning;
using QS.Updater.App;

namespace Workwear.Tools {
	public class UpdateChannelService : IUpdateChannelService {
		private readonly IChangeableConfiguration configuration;
		private readonly IApplicationInfo applicationInfo;

		public UpdateChannelService(IChangeableConfiguration configuration, IApplicationInfo applicationInfo) {
			this.configuration = configuration;
			this.applicationInfo = applicationInfo;
		}

		public UpdateChannel CurrentChannel {
			get {
				if(applicationInfo.Modification != null)
					return UpdateChannel.Current; //Для редакций всегда текущий.
				
				var channel = configuration["AppUpdater:Channel"]; 
				if(UpdateChannel.TryParse(channel, out UpdateChannel updateChannel))
					return updateChannel;
				return UpdateChannel.Current;
			}
		}

		public IEnumerable<UpdateChannel> AvailableChannels {
			get {
				yield return UpdateChannel.Current;
				if(applicationInfo.Modification == null)
					yield return UpdateChannel.Stable;
			}
		}
	}
}
