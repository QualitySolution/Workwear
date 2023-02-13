using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Stock;
using Workwear.Models.Operations;
using Workwear.ViewModels.Import;

namespace Workwear.Models.Import.Issuance
{
	public class ImportModelWorkwearItems : ImportModelBase<DataTypeWorkwearItems, SheetRowWorkwearItems>, IImportModel
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly DataParserWorkwearItems dataParser;
		private readonly SettingsWorkwearItemsViewModel settingsWorkwearItemsViewModel;
		private readonly EmployeeIssueModel issueModel;

		public ImportModelWorkwearItems(DataParserWorkwearItems dataParser, SettingsWorkwearItemsViewModel settingsWorkwearItemsViewModel, EmployeeIssueModel issueModel) : base(dataParser, typeof(CountersWorkwearItems), settingsWorkwearItemsViewModel)
		{
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
			this.settingsWorkwearItemsViewModel = settingsWorkwearItemsViewModel ?? throw new ArgumentNullException(nameof(settingsWorkwearItemsViewModel));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
		}

		#region Параметры
		public string ImportName => "Загрузка выдачи";

		public string DataColumnsRecommendations => "Для каждой значимой колонки проставьте тип данных которые находится в таблице.\nОбязательными данными являются Табельный номер или ФИО, Номенклатура нормы и Дата выдачи. Если колонка с количеством отсутствует, количество будет взято из нормы.";

		#endregion
		protected override DataTypeWorkwearItems[] RequiredDataTypes => new []{DataTypeWorkwearItems.ProtectionTools, DataTypeWorkwearItems.IssueDate};

		protected override bool HasRequiredDataTypes(IEnumerable<DataTypeWorkwearItems> dataTypes) {
			return (dataTypes.Contains(DataTypeWorkwearItems.PersonnelNumber) || dataTypes.Contains(DataTypeWorkwearItems.Fio) || dataTypes.Contains(DataTypeWorkwearItems.NameWithInitials)) 
			       && base.HasRequiredDataTypes(dataTypes);
		}

		public bool CanSave { get; private set; }

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var grouped = UsedRows.Where(x => x.Operation != null)
				.GroupBy(x => x.Employee);
			logger.Debug($"В обработке {grouped.Count()} сотрудников.");
			progress.Start(maxValue: grouped.Count(), text: "Подготовка");
			issueModel.FillWearReceivedInfo(
				grouped.Select(x => x.Key).ToArray(),
				UsedRows.Where(x => x.Operation != null).Select(x => x.Operation).ToArray()
			);
			progress.Add();
			foreach(var employeeGroup in grouped) {
				progress.Add(text: $"Подготовка {employeeGroup.Key.ShortName}");
				employeeGroup.Key.UpdateNextIssueAll();
				dataParser.ChangedEmployees.Add(employeeGroup.Key);
			}

			List<object> toSave = new List<object>();
			toSave.AddRange(SavedNomenclatures);
			toSave.AddRange(dataParser.ChangedEmployees);
			toSave.AddRange(UsedRows.Where(x => x.Operation != null).Select(x => x.Operation));
			return toSave;
		}

		public void MatchAndChanged(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			dataParser.MatchChanges(this, progress, settingsWorkwearItemsViewModel, CountersViewModel, uow, UsedRows);
			OnPropertyChanged(nameof(DisplayRows));

			RecalculateCounters();
			CanSave = CountersViewModel.GetCount(CountersWorkwearItems.NewOperations) > 0;
		}

		public void CleanMatch()
		{
			foreach(var row in UsedRows) {
				row.ChangedColumns.Clear();
				//FIXME Возможно надо чистить сотрудника и операцию, но нужно проверять.
			}
		}

		protected override void RowOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.RowOnPropertyChanged(sender, e);
			RecalculateCounters();
		}

		private void RecalculateCounters()
		{
			CountersViewModel.SetCount(CountersWorkwearItems.SkipRows, UsedRows.Count(x => x.Skipped));
			CountersViewModel.SetCount(CountersWorkwearItems.UsedEmployees, UsedRows.Select(x => x.Employee).Distinct().Count(x => x!= null));
			CountersViewModel.SetCount(CountersWorkwearItems.NewOperations, UsedRows.Count(x => x.Operation != null && x.Operation.Id == 0));
			CountersViewModel.SetCount(CountersWorkwearItems.WorkwearItemNotFound, UsedRows.Count(x => x.Employee != null && x.WorkwearItem == null));
			CountersViewModel.SetCount(CountersWorkwearItems.NewNomenclatures, SavedNomenclatures.Count());
			logger.Debug(String.Join("\n", CountersViewModel.CountersText));
		}

		#region Справочники

		private IEnumerable<Nomenclature> SavedNomenclatures => UsedRows
			.Where(x => !x.Skipped && x.Operation?.Nomenclature?.Id == 0)
			.Select(x => x.Operation.Nomenclature)
			.Distinct();

		#endregion
	}
}
