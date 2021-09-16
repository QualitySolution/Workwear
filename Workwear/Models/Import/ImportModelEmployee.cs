using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public class ImportModelEmployee : ImportModelBase<DataTypeEmployee, SheetRowEmployee>, IImportModel
	{
		private readonly DataParserEmployee dataParser;
		readonly SettingsMatchEmployeesViewModel matchSettingsViewModel;

		public ImportModelEmployee(DataParserEmployee dataParser, SettingsMatchEmployeesViewModel matchSettingsViewModel) : base(dataParser, matchSettingsViewModel)
		{
			this.matchSettingsViewModel = matchSettingsViewModel ?? throw new ArgumentNullException(nameof(matchSettingsViewModel));
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
		}

		#region Параметры
		public string ImportName => "Загрузка сотрудников";

		public string DataColunmsRecomendations => "Установите номер строки с заголовком данных, таким образом чтобы название колонок было корректно. Если в таблице заголовки отутствуют укажите 0.\nДалее для каждой значимой колонки проставьте тип данных которых находится в таблице.\nПри загрузки листа программа автоматически пытается найти залоговок таблицы и выбрать тип данных.\nОбязательными данными являются Фамилия и Имя или ФИО.";

		public Type CountersEnum => typeof(CountersEmployee);
		#endregion

		public override bool CanMatch => Columns.Any(x => x.DataType == DataTypeEmployee.Fio)
			|| (Columns.Any(x => x.DataType == DataTypeEmployee.LastName) && Columns.Any(x => x.DataType == DataTypeEmployee.FirstName));

		public bool CanSave { get; private set; }

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var rows = UsedRows.Where(x => x.ChangedColumns.Any()).ToList();
			progress.Start(maxValue: rows.Count, text: "Подготовка");

			List<object> toSave = new List<object>();
			toSave.AddRange(dataParser.UsedSubdivisions.Where(x => x.Id == 0));
			toSave.AddRange(dataParser.UsedDepartment.Where(x => x.Id == 0));
			toSave.AddRange(dataParser.UsedPosts.Where(x => x.Id == 0));
			foreach(var row in rows) {
				toSave.AddRange(dataParser.PrepareToSave(uow, matchSettingsViewModel, row));
			}
			return toSave;
		}

		public void MatchAndChanged(IProgressBarDisplayable progress, IUnitOfWork uow, CountersViewModel counters)
		{
			if(Columns.Any(x => x.DataType == DataTypeEmployee.PersonnelNumber))
				dataParser.MatchByNumber(uow, UsedRows, Columns, matchSettingsViewModel, progress);
			else
				dataParser.MatchByName(uow, UsedRows, Columns, progress);

			dataParser.FillExistEntities(uow, UsedRows, Columns, progress);
			dataParser.FindChanges(uow, UsedRows, Columns.Where(x => x.DataType != DataTypeEmployee.Unknown).OrderBy(x => x.DataType).ToArray(), progress, matchSettingsViewModel);
			OnPropertyChanged(nameof(DisplayRows));
			counters.SetCount(CountersEmployee.SkipRows, UsedRows.Count(x => x.Skiped));
			counters.SetCount(CountersEmployee.MultiMatch, UsedRows.Count(x => x.Employees.Count > 1));
			counters.SetCount(CountersEmployee.NewEmployee, UsedRows.Count(x => !x.Skiped && x.EditingEmployee.Id == 0));
			counters.SetCount(CountersEmployee.NotChangedEmployee, UsedRows.Count(x => !x.HasChanges));
			counters.SetCount(CountersEmployee.ChangedEmployee, UsedRows.Count(x => x.HasChanges && x.EditingEmployee.Id > 0));

			counters.SetCount(CountersEmployee.NewPosts, dataParser.UsedPosts.Count(x => x.Id == 0));
			counters.SetCount(CountersEmployee.NewDepartments, dataParser.UsedDepartment.Count(x => x.Id == 0));
			counters.SetCount(CountersEmployee.NewSubdivisions, dataParser.UsedSubdivisions.Count(x => x.Id == 0));

			CanSave = counters.GetCount(CountersEmployee.ChangedEmployee) > 0
				|| counters.GetCount(CountersEmployee.NewEmployee) > 0
				|| counters.GetCount(CountersEmployee.NewPosts) > 0
				|| counters.GetCount(CountersEmployee.NewDepartments) > 0
				|| counters.GetCount(CountersEmployee.NewSubdivisions) > 0;
		}
	}
}
