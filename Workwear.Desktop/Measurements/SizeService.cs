using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using Workwear.Repository.Sizes;

namespace Workwear.Measurements
{
	/// <summary>
	/// Предоставляет доступ к списку размеров
	/// </summary>
	public class SizeService
	{
		private IList<Size> sizes;
		private IList<SizeType> types;
		public virtual List<Size> GetSize(
			IUnitOfWork uow, 
			SizeType sizeType = null, 
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false,
			bool fetchSuitableSizes = false
			)
		{
			if(sizes is null)
				sizes = SizeRepository.GetSize(uow, fetchSuitableSizes);
			var filterSizes = (IEnumerable<Size>)sizes;
			if (sizeType != null)
				filterSizes = filterSizes.Where(x => x.SizeType.Id == sizeType.Id);
			if (onlyUseInEmployee)
				filterSizes = filterSizes.Where(x => x.ShowInEmployee);
			if (onlyUseInNomenclature)
				filterSizes = filterSizes.Where(x => x.ShowInNomenclature);
			return filterSizes.ToList();
		}
		
		public IList<SizeType> GetSizeType(
			IUnitOfWork uow, 
			bool onlyUseInEmployee = false)
		{
			if (types is null)
				types = SizeRepository.GetSizeType(uow);
			if (onlyUseInEmployee)
				return types.Where(x => x.UseInEmployee).ToList();
			return types.ToList();
		}

		public List<SizeType> GetSizeTypeByCategory(
			IUnitOfWork uow,
			CategorySizeType categorySizeType,
			bool onlyUseInEmployee = false) =>
				GetSizeType(uow, onlyUseInEmployee)
				.Where(x => x.CategorySizeType == categorySizeType).ToList();

		public List<Size> GetSizeByCategory(
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

		public const int MaxStandartSizeId = 1000, MaxStandartSizeTypeId = 100;
	}
}
