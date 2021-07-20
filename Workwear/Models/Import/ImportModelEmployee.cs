using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public class ImportModelEmployee : ImportModelBase<DataTypeEmployee, SheetRowEmployee>, IImportModel
	{
		private readonly DataParserEmployee dataParser;

		public ImportModelEmployee(DataParserEmployee dataParser) : base(dataParser)
		{
			this.dataParser = dataParser;
		}

		#region Параметры
		public string ImportName => "Загрузка сотрудников";

		public string DataColunmsRecomendations => "Установите номер строки с заголовком данных, таким образом чтобы название колонок было корретно. Если в таблице заголовки отутствуют укажите 0.\nДалее для каждой значимой колонки проставьте тип данных которых находится в таблице.\nПри загрузки листа программа автоматически пытается найти залоговок таблицы и выбрать тип данных.\nОбязательными данными являются Фамилия и Имя или ФИО.";

		public Type CountersEnum => typeof(CountersEmployee);
		#endregion

		public override bool CanMatch => Columns.Any(x => x.DataType == DataTypeEmployee.Fio)
			|| (Columns.Any(x => x.DataType == DataTypeEmployee.LastName) && Columns.Any(x => x.DataType == DataTypeEmployee.FirstName));

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var rows = UsedRows.Where(x => x.ChangedColumns.Any()).ToList();
			progress.Start(maxValue: rows.Count, text: "Подготовка");

			List<object> toSave = new List<object>();
			foreach(var row in rows) {
				toSave.AddRange(dataParser.PrepareToSave(uow, row));
			}
			return toSave;
		}

		public void MatchAndChanged(IUnitOfWork uow, CountersViewModel counters)
		{
			if(Columns.Any(x => x.DataType == DataTypeEmployee.PersonnelNumber))
				dataParser.MatchByNumber(uow, UsedRows, Columns);
			else
				dataParser.MatchByName(uow, UsedRows, Columns);

			dataParser.FindChanges(UsedRows, Columns.Where(x => x.DataType != DataTypeEmployee.Unknown).ToArray());
			OnPropertyChanged(nameof(DisplayRows));
		}
	}
}
