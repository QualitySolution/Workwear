using NHibernate;
using QS.Project.DB;

namespace workwear.Tools.Nhibernate
{
	public class ProgresSessionProvider : ISessionProvider
	{
		private readonly ProgressInterceptor progressInterceptor;

		public ProgresSessionProvider(ProgressInterceptor progressInterceptor)
		{
			this.progressInterceptor = progressInterceptor;
		}

		public virtual ISession OpenSession()
		{
			ISession session = OrmConfig.OpenSession(progressInterceptor);
			session.FlushMode = FlushMode.Commit;
			return session;
		}
	}
}
