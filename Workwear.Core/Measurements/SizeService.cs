using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using workwear.Domain.Sizes;

namespace Workwear.Measurements
{
	/// <summary>
	/// Предоставляет различную информацию о работе с размерами.
	/// </summary>
	public class SizeService
	{
		public SizeService() { }
		#region Новые размеры
		public static IList<Size> GetSize(IUnitOfWork UoW, SizeType sizeType = null) {
			var sizes = UoW.Session.QueryOver<Size>();
			return sizeType is null ? 
				sizes.List() : sizes.Where(x => x.SizeType == sizeType).List();
		}
		public static IList<SizeType> GetSizeType(IUnitOfWork UoW) 
			=> UoW.Session.QueryOver<SizeType>().List();
		public static IList<SizeType> GetSizeTypeByCategory(IUnitOfWork UoW, Category category) 
			=> UoW.Session.QueryOver<SizeType>()
				.Where(x => x.Category == category).List();

		public static IEnumerable<Size> GetSizeByCategory(IUnitOfWork UoW, Category category) {
			SizeType sizeTypeAlias = null;
			var query = UoW.Session.QueryOver<Size>()
				.JoinAlias(x => x.SizeType, () => sizeTypeAlias)
				.Where(x => sizeTypeAlias.Category == category).List();
			return query;
		}
		#endregion
	}
}
