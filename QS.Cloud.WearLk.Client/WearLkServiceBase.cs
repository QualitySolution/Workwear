using QS.Cloud.Client;

namespace QS.Cloud.WearLk.Client
{
	public class WearLkServiceBase : CloudClientServiceBase
	{
		public WearLkServiceBase(ISessionInfoProvider sessionInfoProvider) : base(sessionInfoProvider, "lk.wear.cloud.qsolution.ru", 4201) {}
	}
}
