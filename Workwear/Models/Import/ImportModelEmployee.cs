using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Measurements;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public class ImportModelEmployee : ImportModelBase<DataTypeEmployee, SheetRowEmployee>, IImportModel
	{
		private readonly DataParserEmployee dataParser;
		readonly SettingsMatchEmployeesViewModel matchSettingsViewModel;
		private readonly SizeService sizeService;
		private readonly IUnitOfWork unitOfWork;
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public ImportModelEmployee(
			DataParserEmployee dataParser, 
			SettingsMatchEmployeesViewModel matchSettingsViewModel,
			SizeService sizeService,
			IUnitOfWorkFactory unitOfWorkFactory
		) : base(dataParser, typeof(CountersEmployee), matchSettingsViewModel)
		{
			this.matchSettingsViewModel = matchSettingsViewModel ?? throw new ArgumentNullException(nameof(matchSettingsViewModel));
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
			this.sizeService = sizeService;
			unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
		}

		public override void Init(IUnitOfWork uow)
		{
			dataParser.CreateSizeDatatypes(uow);
		}

		#region Параметры
		public string ImportName => "Загрузка сотрудников";

		public string DataColumnsRecommendations => "Установите номер строки с заголовком данных, таким образом " +
		                                            "чтобы название колонок было корректно. Если в таблице заголовки " +
		                                            "отсутствуют укажите 0.\nДалее для каждой значимой колонки " +
		                                            "проставьте тип данных которых находится в таблице." +
		                                            "\nПри загрузки листа программа автоматически пытается найти " +
		                                            "заголовок таблицы и выбрать тип данных.\nОбязательными данными " +
		                                            "являются Фамилия и Имя или ФИО.";
		#endregion

		protected override bool HasRequiredDataTypes(IEnumerable<DataTypeEmployee> dataTypes) => dataTypes.Contains(DataTypeEmployee.Fio) 
			|| (dataTypes.Contains(DataTypeEmployee.FirstName) && dataTypes.Contains(DataTypeEmployee.LastName));

		protected override DataTypeEmployee[] RequiredDataTypes => new []{DataTypeEmployee.Fio, DataTypeEmployee.LastName, DataTypeEmployee.FirstName};
	
		public bool CanSave { get; private set; }

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var rows = UsedRows.Where(x => x.HasChanges).ToList();
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

		public void MatchAndChanged(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			if(Columns.Any(x => x.DataTypeEnum == DataTypeEmployee.PersonnelNumber))
				dataParser.MatchByNumber(uow, UsedRows, Columns, matchSettingsViewModel, progress);
			else
				dataParser.MatchByName(uow, UsedRows, Columns, progress);

			dataParser.FillExistEntities(uow, UsedRows, Columns, progress);
			dataParser.FindChanges(
				uow, 
				UsedRows, 
				Columns
					.Where(x => x.DataType != null || x.DataTypeEnum != DataTypeEmployee.Unknown)
					.OrderBy(x => x.DataTypeEnum)
					.ToArray(), 
				progress, 
				matchSettingsViewModel);
			OnPropertyChanged(nameof(DisplayRows));
			
			RecalculateCounters();
			CanSave = CountersViewModel.GetCount(CountersEmployee.ChangedEmployee) > 0
				|| CountersViewModel.GetCount(CountersEmployee.NewEmployee) > 0
				|| CountersViewModel.GetCount(CountersEmployee.NewPosts) > 0
				|| CountersViewModel.GetCount(CountersEmployee.NewDepartments) > 0
				|| CountersViewModel.GetCount(CountersEmployee.NewSubdivisions) > 0;
		}

		public void CleanMatch()
		{
			foreach(var row in UsedRows) {
				row.ChangedColumns.Clear();
				row.Employees.Clear();
			}
		}

		protected override void RowOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.RowOnPropertyChanged(sender, e);
			RecalculateCounters();
		}

		private void RecalculateCounters()
		{
			CountersViewModel.SetCount(CountersEmployee.SkipRows, UsedRows.Count(x => x.Skipped));
			CountersViewModel.SetCount(CountersEmployee.MultiMatch, UsedRows.Count(x => !x.Skipped && x.Employees.Count > 1));
			CountersViewModel.SetCount(CountersEmployee.NewEmployee, UsedRows.Count(x => !x.Skipped && x.EditingEmployee.Id == 0));
			CountersViewModel.SetCount(CountersEmployee.NotChangedEmployee, UsedRows.Count(x => !x.Skipped && !x.HasChanges));
			CountersViewModel.SetCount(CountersEmployee.ChangedEmployee, UsedRows.Count(x => !x.Skipped && x.HasChanges && x.EditingEmployee.Id > 0));

			CountersViewModel.SetCount(CountersEmployee.NewPosts, dataParser.UsedPosts.Count(x => x.Id == 0));
			CountersViewModel.SetCount(CountersEmployee.NewDepartments, dataParser.UsedDepartment.Count(x => x.Id == 0));
			CountersViewModel.SetCount(CountersEmployee.NewSubdivisions, dataParser.UsedSubdivisions.Count(x => x.Id == 0));
		}
	}
}
