using System.Collections.Generic;
using Workwear.Domain.Regulations;
using EtnNormItem = QS.Cloud.WorkwearDictionary.Grpc.Contracts.NormItem;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.Grpc.Contracts.ItemSIZ;

namespace Workwear.Models.Regulations {
	/// <summary>
	/// Спрашивает пользователя, какие позиции комплекта ЕТН добавить в норму.
	/// Нужно, чтобы <see cref="EtnNormImportModel"/> мог тестироваться без гграфики.
	/// </summary>
	public interface IEtnComplectResolver {
		/// <summary>
		/// Комплект "или": пользователь выбирает вариант для добавления. Возвращает выбор пользователя, null - если отказался.
		/// </summary>
		IList<EtnComplectResolvedItem> ResolveOR(EtnNormItem complect);

		/// <summary>
		/// Позиции с неопределённым сроком выдачи (period_type - "разовое использование"/"по необходимости" или есть period_special)
		/// собранные со всей нормы - просим пользователя проставить им сроки/количество разом.
		/// Возвращает выбор пользователя, либо null если пользователь отказался добавлять эти позиции.
		/// </summary>
		IList<EtnComplectResolvedItem> ResolveNeedPeriodItems(IList<EtnItemSIZ> items);
	}

	/// <summary>
	/// Позиция комплекта ЕТН, выбранная и при необходимости отредактированная пользователем.
	/// </summary>
	public class EtnComplectResolvedItem {
		public string Name { get; set; }
		public string TypeName { get; set; }
		public string Condition { get; set; }
		public int? Amount { get; set; }
		public int? PeriodCount { get; set; }
		public NormPeriodType? PeriodType { get; set; }

		/// <summary>
		/// Комментарий к строке нормы (не к номенклатуре)
		/// </summary>
		public string NormItemComment { get; set; }

		/// <summary>
		/// Комментарий для создаваемой номенклатуры (не для строки нормы).
		/// </summary>
		public string ProtectionToolsComment { get; set; }

		/// <summary>
		/// У позиции в ЕТН period_type - "разовое использование"/"по необходимости" или есть (period_special).
		/// </summary>
		public bool HasUndefinedPeriod { get; set; }
	}
}
