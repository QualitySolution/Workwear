using System.Collections.Generic;
using NHibernate;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace Workwear.Tools.OverNorms 
{
	public interface IOverNormService 
	{
		/// <summary>
		/// Получить список выданных операций выдачи вне нормы для конкретного сотрудника 
		/// </summary>
		/// <param name="param">Условия для поиска операций выдачи вне нормы</param>
		/// <param name="type">Тип операции выдачи вне нормы</param>
		/// <param name="warehouse">Склад для поиска</param>
		/// <returns>Список выданных операций выдачи вне нормы для конкретного сотурдника, удовлетворяющих условиям</returns>
		IList<OverNormOperation> GetActualOverNormIssued(OverNormParam param = null, OverNormType? type = null, Warehouse warehouse = null);
	}
}
