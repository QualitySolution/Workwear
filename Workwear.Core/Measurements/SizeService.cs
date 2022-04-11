using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using workwear.Domain.Sizes;
using workwear.Repository.Sizes;

namespace Workwear.Measurements
{
	/// <summary>
	/// Предоставляет доступ к списку размеров
	/// </summary>
	public class SizeService
	{
		private static List<Size> sizes;
		private static List<SizeType> types;
		public static List<Size> GetSize(
			IUnitOfWork uow, 
			SizeType sizeType = null, 
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false)
		{
			if(sizes is null)
				sizes = SizeRepository.GetSize(uow);
			var filterSizes = sizes;
			if (sizeType != null)
				filterSizes = filterSizes.Where(x => x.SizeType == sizeType).ToList();
			if (onlyUseInEmployee)
				filterSizes = filterSizes.Where(x => x.UseInEmployee).ToList();
			if (onlyUseInNomenclature)
				filterSizes = filterSizes.Where(x => x.UseInNomenclature).ToList();
			return filterSizes.ToList();
		}
		
		public static List<SizeType> GetSizeType(
			IUnitOfWork uow, 
			bool onlyUseInEmployee = false)
		{
			if (types is null)
				types = SizeRepository.GetSizeType(uow);
			var filterTypes = types;
			if (onlyUseInEmployee)
				filterTypes = filterTypes.Where(x => x.UseInEmployee).ToList();
			return filterTypes;
		}

		public static List<SizeType> GetSizeTypeByCategory(
			IUnitOfWork uow,
			CategorySizeType categorySizeType,
			bool onlyUseInEmployee = false) =>
				GetSizeType(uow, onlyUseInEmployee)
				.Where(x => x.CategorySizeType == categorySizeType).ToList();

		public static List<Size> GetSizeByCategory(
			IUnitOfWork uow, 
			CategorySizeType categorySizeType, 
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false) =>
				GetSize(uow, null, onlyUseInEmployee, onlyUseInNomenclature)
				.Where(x => x.SizeType.CategorySizeType == categorySizeType).ToList();

		public void RefreshSizes(IUnitOfWork uow) => 
			sizes = SizeRepository.GetSize(uow);
		public void RefreshSizesType(IUnitOfWork uow) => 
			types = SizeRepository.GetSizeType(uow);
		public void ClearSizes() => sizes = null;
		public void ClearTypes() => types = null;
	}
}
