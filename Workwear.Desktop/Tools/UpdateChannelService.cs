using System.Collections.Generic;
using QS.Configuration;
using QS.Project.Versioning;
using QS.Updater.App;

namespace Workwear.Tools {
	public class UpdateChannelService : IUpdateChannelService {
		public UpdateChannelService(IChangeableConfiguration configuration, IApplicationInfo applicationInfo) {
		}

		public UpdateChannel CurrentChannel {
			get {
				// В сборке для реестра проверка обновлений полностью отключена.
				return UpdateChannel.Off;
			}
		}

		public IEnumerable<UpdateChannel> AvailableChannels {
			get {
				yield return UpdateChannel.Off;
			}
		}
	}
}
