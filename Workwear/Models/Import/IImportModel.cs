using System;
using System.Collections.Generic;
using System.ComponentModel;
using NPOI.SS.UserModel;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ViewModels;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public interface IImportModel : INotifyPropertyChanged
	{
		string ImportName { get; }
		string DataColumnsRecommendations { get; }
		Type DataTypeEnum { get; }
		
		CountersViewModel CountersViewModel { get; }

		#region Колонки
		IList<IDataColumn> DisplayColumns {get;}
		int MaxSourceColumns { get; set; }
		void AutoSetupColumns(IProgressBarDisplayable progress);
		#endregion

		#region Строки
		int HeaderRow { get; set; }
		int SheetRowCount { get; }
		void AddRow(IRow cells);
		List<ISheetRow> DisplayRows { get; }
		#endregion
		ICell[,] MergedCells { get; set; }

		#region Сопоставление
		ViewModelBase MatchSettingsViewModel { get; }
		bool CanMatch { get; }
		void MatchAndChanged(IProgressBarDisplayable progress, IUnitOfWork uow);
		/// <summary>
		/// Вызывается при шаге назад, для очистки заполненых данных
		/// </summary>
		void CleanMatch();
		#endregion

		#region Сохранение
		bool CanSave { get; }
		List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow);
		#endregion
	}
}
