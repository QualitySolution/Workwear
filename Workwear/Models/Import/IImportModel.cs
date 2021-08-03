using System;
using System.Collections.Generic;
using System.ComponentModel;
using NPOI.SS.UserModel;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public interface IImportModel : INotifyPropertyChanged
	{
		string ImportName { get; }
		string DataColunmsRecomendations { get; }
		Type CountersEnum { get; }
		Type DataTypeEnum { get; }

		#region Колонки
		IList<IDataColumn> DisplayColumns {get;}
		int MaxSourceColumns { get; set; }
		void AutoSetupColumns();
		#endregion

		#region Строки
		int HeaderRow { get; set; }
		void AddRow(IRow cells);
		List<ISheetRow> DisplayRows { get; }
		#endregion

		#region Сопоставление
		bool CanMatch { get; }
		void MatchAndChanged(IUnitOfWork uow, CountersViewModel counters);
		#endregion

		#region Сохранение
		bool CanSave { get; }
		List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow);
		#endregion
	}
}
