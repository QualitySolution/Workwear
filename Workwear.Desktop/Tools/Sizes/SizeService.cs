using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;

namespace Workwear.Tools.Sizes
{
	/// <summary>
	/// Предоставляет доступ информации о размерах.
	/// Кеширует размеры при первом обращении.
	/// При необходимости нужно принудительно обновлять.  
	/// </summary>
	public class SizeService
	{
		private IList<Size> sizes;
		private IList<SizeType> types;

		public void RefreshSizes(IUnitOfWork uow) {
			var querySizes = uow.Session.QueryOver<Size>().Future();
			
			uow.Session.QueryOver<Size>()
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Fetch, s => s.SuitableSizes)
				.Future();
			
			uow.Session.QueryOver<Size>()
				.Fetch(SelectMode.ChildFetch, x => x)
				.Fetch(SelectMode.Fetch, s => s.SizesWhereIsThisSizeAsSuitable)
				.Future();
			
			var queryTypes = uow.Session.QueryOver<SizeType>()
				.OrderBy(x => x.Position).Asc
				.Future();

			sizes = querySizes.ToList();
			types = queryTypes.ToList();
		}

		public virtual IEnumerable<Size> GetSize(
			IUnitOfWork uow,
			SizeType sizeType = null, 
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false
		)
		{
			if(sizes is null)
				RefreshSizes(uow);
			IEnumerable<Size> filterSizes = sizes;
			if (sizeType != null)
				filterSizes = filterSizes.Where(x => x.SizeType.Id == sizeType.Id);
			if (onlyUseInEmployee)
				filterSizes = filterSizes.Where(x => x.ShowInEmployee);
			if (onlyUseInNomenclature)
				filterSizes = filterSizes.Where(x => x.ShowInNomenclature);
			return filterSizes;
		}
		
		public IEnumerable<SizeType> GetSizeType(
			IUnitOfWork uow, 
			bool onlyUseInEmployee = false)
		{
			if (types is null)
				RefreshSizes(uow);
			if (onlyUseInEmployee)
				return types.Where(x => x.UseInEmployee);
			return types;
		}

		public IEnumerable<SizeType> GetSizeTypeByCategory(
			IUnitOfWork uow,
			CategorySizeType categorySizeType,
			bool onlyUseInEmployee = false) =>
				GetSizeType(uow, onlyUseInEmployee)
				.Where(x => x.CategorySizeType == categorySizeType);

		public IEnumerable<Size> GetSizeByCategory(
			IUnitOfWork uow,
			CategorySizeType categorySizeType,
			bool onlyUseInEmployee = false, 
			bool onlyUseInNomenclature = false) =>
				GetSize(uow, null, onlyUseInEmployee, onlyUseInNomenclature)
				.Where(x => x.SizeType.CategorySizeType == categorySizeType);

		public const int MaxStandardSizeId = 1000, MaxStandardSizeTypeId = 100;

		#region Статика
		/// <summary>
		/// Возвращает строковое представление размера с ростом, если рост не null.
		/// </summary>
		public static string SizeTitle(Size size, Size height) => height != null ? $"{size?.Name}/{height?.Name}" : size?.Name;
		/// <summary>
		/// Сопоставляет размер в сотруднике с размером номенклатуры, с учетом подходящих.
		/// Так же учитывайте что если в размер в номенклатуре отсутствует, она считается подходящей для всех размеров.
		/// </summary>
		public static bool IsSuitable(Size sizeEmployee, Size sizeNomenclature) {
			if(sizeNomenclature == null)
				return true;
			if(sizeEmployee?.Id == sizeNomenclature.Id)
				return true;
			if(sizeEmployee != null)
				return sizeEmployee.SuitableSizes.Any(x => x.Id == sizeNomenclature.Id)
					|| sizeNomenclature.SuitableSizes.Any(x => x.Id == sizeEmployee.Id);
			return false;
		}

		#endregion
	}
}
