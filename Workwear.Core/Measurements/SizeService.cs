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
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false)
		{
			var sizes = uow.Session.QueryOver<Size>();
			if (sizeType != null)
				sizes = sizes.Where(x => x.SizeType == sizeType);
			if (onlyUseInEmployee)
				sizes = sizes.Where(x => x.UseInEmployee);
			if (onlyUseInNomenclature)
				sizes = sizes.Where(x => x.UseInNomenclature);
			return sizes.List();
		}
		
		public static IList<SizeType> GetSizeType(
			IUnitOfWork uow, 
			bool onlyUseInEmployee = false) 
		{
			var sizeTypes = uow.Session.QueryOver<SizeType>();
			if (onlyUseInEmployee)
				sizeTypes = sizeTypes.Where(x => x.UseInEmployee);
			return sizeTypes.List();
		}

		public static IEnumerable<SizeType> GetSizeTypeByCategory(
			IUnitOfWork uow,
			Category category,
			bool onlyUseInEmployee = false)
		{
			var sizeTypes = 
				uow.Session.QueryOver<SizeType>().Where(x => x.Category == category);
			if (onlyUseInEmployee)
				sizeTypes = sizeTypes.Where(x => x.UseInEmployee);
			return sizeTypes.List();
		}

		public static IEnumerable<Size> GetSizeByCategory(
			IUnitOfWork uow, 
			Category category, 
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false) {
			SizeType sizeTypeAlias = null;
			var sizes = uow.Session.QueryOver<Size>()
				.JoinAlias(x => x.SizeType, () => sizeTypeAlias)
				.Where(x => sizeTypeAlias.Category == category);
			if (onlyUseInEmployee)
				sizes = sizes.Where(x => x.UseInEmployee);
			if (onlyUseInNomenclature)
				sizes = sizes.Where(x => x.UseInNomenclature);
			return sizes.List();
		}
	}
}
