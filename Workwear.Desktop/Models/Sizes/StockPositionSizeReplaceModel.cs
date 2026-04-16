using System;
using System.Linq;
using NHibernate.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Supply;

namespace Workwear.Models.Sizes {
	/// <summary>
	/// Замена конкретного значения размера (или роста) во всех операциях для заданной номенклатуры.
	/// В отличие от <see cref="SizeTypeReplaceModel"/>, здесь меняется не тип размера,
	/// а конкретное значение Size → другое значение Size (того же типа) для одной номенклатуры.
	/// </summary>
	public class StockPositionSizeReplaceModel {
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Выполняет замену размера и/или роста во всех операциях для указанной номенклатуры.
		/// </summary>
		/// <param name="uow">Единица работы.</param>
		/// <param name="interactive">Интерфейс для показа сообщений пользователю.</param>
		/// <param name="progress">Индикатор прогресса.</param>
		/// <param name="nomenclature">Номенклатура, для которой выполняется замена.</param>
		/// <param name="oldSize">Заменяемый размер (null — размер не меняется).</param>
		/// <param name="newSize">Новый размер (должен быть не null, если oldSize задан).</param>
		/// <param name="oldHeight">Заменяемый рост (null — рост не меняется).</param>
		/// <param name="newHeight">Новый рост (должен быть не null, если oldHeight задан).</param>
		/// <returns>true — операция выполнена; false — отменена или произошла ошибка.</returns>
		public bool ReplaceInStock(
			IUnitOfWork uow,
			IInteractiveService interactive,
			IProgressBarDisplayable progress,
			Nomenclature nomenclature,
			Size oldSize,
			Size newSize,
			Size oldHeight,
			Size newHeight)
		{
			if(nomenclature == null) throw new ArgumentNullException(nameof(nomenclature));

			bool replaceSize   = oldSize   != null && oldSize.Id   != newSize?.Id;
			bool replaceHeight = oldHeight != null && oldHeight.Id != newHeight?.Id;

			if(!replaceSize && !replaceHeight) {
				interactive.ShowMessage(ImportanceLevel.Warning,
					"Выбранные значения совпадают с текущими. Замена не требуется.");
				return false;
			}

			if(replaceSize && newSize == null) {
				interactive.ShowMessage(ImportanceLevel.Error, "Не выбран новый размер.");
				return false;
			}

			if(replaceHeight && newHeight == null) {
				interactive.ShowMessage(ImportanceLevel.Error, "Не выбран новый рост.");
				return false;
			}

			int steps = (replaceSize ? 10 : 0) + (replaceHeight ? 10 : 0);
			progress.Start(steps, text: "Замена размеров в операциях…");

			var nomenclatureId = nomenclature.Id;

			if(replaceSize) {
				logger.Info($"Замена размера {oldSize.Name} → {newSize.Name} для номенклатуры id={nomenclatureId}");
				ReplaceSize(uow, progress, nomenclatureId, oldSize, newSize);
			}

			if(replaceHeight) {
				logger.Info($"Замена роста {oldHeight.Name} → {newHeight.Name} для номенклатуры id={nomenclatureId}");
				ReplaceHeight(uow, progress, nomenclatureId, oldHeight, newHeight);
			}

			progress.Close();
			return true;
		}

		private void ReplaceSize(IUnitOfWork uow, IProgressBarDisplayable progress, int nomenclatureId, Size oldSize, Size newSize) {
			progress.Add(text: $"Складские операции…");
			uow.GetAll<WarehouseOperation>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Операции выдачи сотрудникам…");
			uow.GetAll<EmployeeIssueOperation>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Строки расхода…");
			uow.GetAll<ExpenseItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Строки коллективного расхода…");
			uow.GetAll<CollectiveExpenseItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Строки прихода…");
			uow.GetAll<IncomeItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Строки ведомостей выдачи…");
			uow.GetAll<IssuanceSheetItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Строки списания…");
			uow.GetAll<WriteoffItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Операции дежурных норм…");
			uow.GetAll<DutyNormIssueOperation>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Строки возврата…");
			uow.GetAll<ReturnItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();

			progress.Add(text: $"Строки поставок…");
			uow.GetAll<ShipmentItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.WearSize.Id == oldSize.Id)
				.UpdateBuilder().Set(x => x.WearSize, newSize).Update();
		}

		private void ReplaceHeight(IUnitOfWork uow, IProgressBarDisplayable progress, int nomenclatureId, Size oldHeight, Size newHeight) {
			progress.Add(text: $"Складские операции (рост)…");
			uow.GetAll<WarehouseOperation>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Операции выдачи сотрудникам (рост)…");
			uow.GetAll<EmployeeIssueOperation>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Строки расхода (рост)…");
			uow.GetAll<ExpenseItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Строки коллективного расхода (рост)…");
			uow.GetAll<CollectiveExpenseItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Строки прихода (рост)…");
			uow.GetAll<IncomeItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Строки ведомостей выдачи (рост)…");
			uow.GetAll<IssuanceSheetItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Строки списания (рост)…");
			uow.GetAll<WriteoffItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Операции дежурных норм (рост)…");
			uow.GetAll<DutyNormIssueOperation>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Строки возврата (рост)…");
			uow.GetAll<ReturnItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();

			progress.Add(text: $"Строки поставок (рост)…");
			uow.GetAll<ShipmentItem>()
				.Where(x => x.Nomenclature.Id == nomenclatureId && x.Height.Id == oldHeight.Id)
				.UpdateBuilder().Set(x => x.Height, newHeight).Update();
		}
	}
}

