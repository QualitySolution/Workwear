using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NPOI.SS.UserModel;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.Models.Import;

namespace workwear.ViewModels.Tools
{
	public class NormsLoadViewModel : ExcelImportViewModel<DataTypeNorm>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public NormsLoadViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IInteractiveMessage interactiveMessage, DataParserNorm dataParser) : base(unitOfWorkFactory, navigation, interactiveMessage, dataParser)
		{
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
		}

		private readonly DataParserNorm dataParser;

		#region Шаг 1


		#endregion

		#region Шаг 2



		#endregion

		#region Шаг 3

		public new List<SheetRowNorm> DisplayRows => base.DisplayRows.Cast<SheetRowNorm>().ToList();

		public void ThirdStep()
		{
			CurrentStep = 2;

		}

		public new void Save()
		{
			int i = 0;
			var toSave = DisplayRows.Where(x => (SaveNewEmployees && !x.Employees.Any()) 
					|| (SaveChangedEmployees && x.Employees.Any() && x.ChangedColumns.Any()))
				.ToList();
			logger.Info($"Новых: {toSave.Count(x => !x.Employees.Any())} Измененых: {toSave.Count(x => x.Employees.Any())} Всего: {toSave.Count}");
			ProgressStep3.Start(toSave.Count);
			foreach(var row in toSave) {
				var employee = dataParser.PrepareToSave(UoW, row);
				UoW.Save(employee);
				i++;
				if(i % 50 == 0) {
					UoW.Commit();
					Console.Write($"\r\tСохранили {i} [{(float)i / toSave.Count:P}]... ");
				}
				ProgressStep3.Add();
			}
			UoW.Commit();
			ProgressStep3.Close();
			Close(false, CloseSource.Save);
		}

		#endregion

		#region private Methods

		#endregion
	}
}
