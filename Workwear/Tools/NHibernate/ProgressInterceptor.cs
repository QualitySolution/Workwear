using System;
using NHibernate;

namespace workwear.Tools.Nhibernate
{
	public class ProgressInterceptor : EmptyInterceptor, IInterceptor
	{
		public event EventHandler PrepareStatement;

		public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
		{
			PrepareStatement?.Invoke(this, EventArgs.Empty);
			return base.OnPrepareStatement(sql);
		}
	}
}
