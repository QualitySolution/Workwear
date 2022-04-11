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
		private static IEnumerable<Size> sizes;
		private static IEnumerable<SizeType> types;
		public static IEnumerable<Size> GetSize(
			IUnitOfWork uow, 
			SizeType sizeType = null, 
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false)
		{
			if(sizes is null)
				sizes = SizeRepository.GetSize(uow);
			if (sizeType != null)
				sizes = sizes.Where(x => x.SizeType == sizeType);
			if (onlyUseInEmployee)
				sizes = sizes.Where(x => x.UseInEmployee);
			if (onlyUseInNomenclature)
				sizes = sizes.Where(x => x.UseInNomenclature);
			return sizes;
		}
		
		public static IEnumerable<SizeType> GetSizeType(
			IUnitOfWork uow, 
			bool onlyUseInEmployee = false)
		{
			if (types is null)
				types = SizeRepository.GetSizeType(uow);
			if (onlyUseInEmployee)
				types = types.Where(x => x.UseInEmployee);
			return types;
		}

		public static IEnumerable<SizeType> GetSizeTypeByCategory(
			IUnitOfWork uow,
			CategorySizeType categorySizeType,
			bool onlyUseInEmployee = false) =>
				GetSizeType(uow, onlyUseInEmployee)
				.Where(x => x.CategorySizeType == categorySizeType);

		public static IEnumerable<Size> GetSizeByCategory(
			IUnitOfWork uow, 
			CategorySizeType categorySizeType, 
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false) =>
				GetSize(uow, null, onlyUseInEmployee, onlyUseInNomenclature)
				.Where(x => x.SizeType.CategorySizeType == categorySizeType);

		public void RefreshSizes(IUnitOfWork uow) => 
			sizes = SizeRepository.GetSize(uow);
		public void RefreshSizesType(IUnitOfWork uow) => 
			types = SizeRepository.GetSizeType(uow);
		public void ClearSizes() => sizes = null;
		public void ClearTypes() => types = null;
	}
}
