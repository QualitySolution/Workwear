using System.Collections.Generic;
using QS.DomainModel.UoW;
using workwear.Domain.Sizes;

namespace Workwear.Measurements
{
	/// <summary>
	/// Предоставляет доступ к списку размеров
	/// </summary>
	public class SizeService
	{
		public static IList<Size> GetSize(
			IUnitOfWork uow, 
			SizeType sizeType = null, 
			bool unusedInEmployee = false, 
			bool unusedInNomenclature = false)
		{
			var sizes = uow.Session.QueryOver<Size>();
			if (sizeType != null)
				sizes = sizes.Where(x => x.SizeType == sizeType);
			if (!unusedInEmployee)
				sizes = sizes.Where(x => x.UseInEmployee);
			if (!unusedInNomenclature)
				sizes = sizes.Where(x => x.UseInNomenclature);
			return sizes.List();
		}
		
		public static IList<SizeType> GetSizeType(
			IUnitOfWork uow, 
			bool unusedInEmployee = false) 
		{
			var sizeTypes = uow.Session.QueryOver<SizeType>();
			if (!unusedInEmployee)
				sizeTypes = sizeTypes.Where(x => x.UseInEmployee);
			return sizeTypes.List();
		}

		public static IEnumerable<SizeType> GetSizeTypeByCategory(
			IUnitOfWork uow,
			Category category,
			bool unusedInEmployee = false)
		{
			var sizeTypes = 
				uow.Session.QueryOver<SizeType>().Where(x => x.Category == category);
			if (!unusedInEmployee)
				sizeTypes = sizeTypes.Where(x => x.UseInEmployee);
			return sizeTypes.List();
		}

		public static IEnumerable<Size> GetSizeByCategory(
			IUnitOfWork uow, 
			Category category, 
			bool unusedInEmployee = false, 
			bool unusedInNomenclature = false) {
			SizeType sizeTypeAlias = null;
			var sizes = uow.Session.QueryOver<Size>()
				.JoinAlias(x => x.SizeType, () => sizeTypeAlias)
				.Where(x => sizeTypeAlias.Category == category);
			if (!unusedInEmployee)
				sizes = sizes.Where(x => x.UseInEmployee);
			if (!unusedInNomenclature)
				sizes = sizes.Where(x => x.UseInNomenclature);
			return sizes.List();
		}
	}
}
