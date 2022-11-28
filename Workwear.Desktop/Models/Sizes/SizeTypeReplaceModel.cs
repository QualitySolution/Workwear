using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Statements;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Sizes;

namespace Workwear.Models.Sizes {
	public class SizeTypeReplaceModel {
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private readonly SizeRepository sizeRepository;

		public SizeTypeReplaceModel(SizeRepository sizeRepository) {
			this.sizeRepository = sizeRepository;
		}

		public bool TryReplaceSizes(IUnitOfWork uow, IInteractiveService interactive, IProgressBarDisplayable progress, Nomenclature[] nomenclatures, SizeType oldSizeType, SizeType newSizeType, SizeType oldHeight, SizeType newHeight) {
			var usedSizesAll = sizeRepository.GetUsedSizes(uow, nomenclatures);
			if(!usedSizesAll.Any()) {
				logger.Debug("Смена типа размера, не потребовала замены размеров в базе.");
				return true;
			}

			var usedSizes = usedSizesAll.Where(x => x.SizeType.CategorySizeType == CategorySizeType.Size).ToList();
			var usedHeights = usedSizesAll.Where(x => x.SizeType.CategorySizeType == CategorySizeType.Height).ToList();
			
			if(newSizeType == null && usedSizes.Any()) {
				interactive.ShowMessage(ImportanceLevel.Error, "Размеры старого типа уже используются в операциях их нельзя удалить.");
				return false;
			}
			
			if(newHeight == null && usedHeights.Any()) {
				interactive.ShowMessage(ImportanceLevel.Error, "Рост старого типа уже используются в операциях его нельзя удалить.");
				return false;
			}

			if(newSizeType != null) {
				var sizeNoMatched = CheckCanReplace(usedSizes, newSizeType);
				if(sizeNoMatched.Any()) {
					var sizeList = String.Join(", ", sizeNoMatched.Select(x => x.Name));
					interactive.ShowMessage(ImportanceLevel.Error, $"Некоторые размеры уже используются в операциях. Не возможно произвести замену следующих размеров: " + sizeList);
					return false;
				}
			}
			
			if(newHeight != null) {
				var heightNoMatched = CheckCanReplace(usedHeights, newHeight);
				if(heightNoMatched.Any()) {
					var sizeList = String.Join(", ", heightNoMatched.Select(x => x.Name));
					interactive.ShowMessage(ImportanceLevel.Error, $"Некоторые роста уже используются в операциях. Не возможно произвести замену следующих ростов: " + sizeList);
					return false;
				}
			}

			var text = "Будет произведена замена ";
			if(usedSizes.Any())
				text += "размеров: " + String.Join(", ", usedSizes.Select(x => x.Name));
			if(usedHeights.Any())
				text += " и ростов: " + String.Join(", ", usedHeights.Select(x => x.Name));
			text += " складских операциях. Продолжить?";
			if(interactive.Question(text)) {
				progress.Start(usedSizesAll.Count * 7);
				ReplaceSize(uow, progress, usedSizes, nomenclatures, newSizeType);
				ReplaceHeight(uow, progress, usedHeights, nomenclatures, newHeight);
				progress.Close();
				return true;
			}
			return false;
		}

		private List<Size> CheckCanReplace(IEnumerable<Size> sizes, SizeType newSizeType) {
			var noMatched = new List<Size>();
			foreach(var size in sizes) {
				if(newSizeType.Sizes.Any(x => x.Name == size.Name))
					continue;
				
				noMatched.Add(size);
			}

			return noMatched;
		}
		
		private void ReplaceSize(IUnitOfWork uow, IProgressBarDisplayable progress, IEnumerable<Size> sizes, Nomenclature[] nomenclatures, SizeType newSizeType) {
			var nomenclaturesIds = nomenclatures.Select(x => x.Id).ToArray();
			foreach(var size in sizes) {
				Size newSize = newSizeType.Sizes.First(x => x.Name == size.Name);
				progress.Add(text: "Замена " + size.Title);
				
				uow.GetAll<WarehouseOperation>()
					.Where(x => x.WearSize.Id == size.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.WearSize, newSize)
					.Update();
				
				progress.Add();
				uow.GetAll<EmployeeIssueOperation>()
					.Where(x => x.WearSize.Id == size.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.WearSize, newSize)
					.Update();
				
				progress.Add();
				uow.GetAll<ExpenseItem>()
					.Where(x => x.WearSize.Id == size.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.WearSize, newSize)
					.Update();
				
				progress.Add();
				uow.GetAll<CollectiveExpenseItem>()
					.Where(x => x.WearSize.Id == size.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.WearSize, newSize)
					.Update();
				
				progress.Add();
				uow.GetAll<IncomeItem>()
					.Where(x => x.WearSize.Id == size.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.WearSize, newSize)
					.Update();
				
				progress.Add();
				uow.GetAll<IssuanceSheetItem>()
					.Where(x => x.WearSize.Id == size.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.WearSize, newSize)
					.Update();
				
				progress.Add();
				uow.GetAll<WriteoffItem>()
					.Where(x => x.WearSize.Id == size.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.WearSize, newSize)
					.Update();
				
				//Есть еще ссылки с SubdivisionIssueOperation но не понятно как это используется, возможно эти ссылки вообще не нужны.
				//Так же есть ссылка из Barcode, но пока не уверен, стоит ли менять размер после выпуска штрихкода.
			}
		}

		private void ReplaceHeight(IUnitOfWork uow, IProgressBarDisplayable progress, IEnumerable<Size> heights, Nomenclature[] nomenclatures, SizeType newHeightType) {
			var nomenclaturesIds = nomenclatures.Select(x => x.Id).ToArray();
			foreach(var height in heights) {
				Size newHeight = newHeightType.Sizes.First(x => x.Name == height.Name);
				progress.Add(text: "Замена " + height.Title);
				uow.GetAll<WarehouseOperation>()
					.Where(x => x.Height.Id == height.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.Height, newHeight)
					.Update();

				progress.Add();
				uow.GetAll<EmployeeIssueOperation>()
					.Where(x => x.Height.Id == height.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.Height, newHeight)
					.Update();

				progress.Add();
				uow.GetAll<ExpenseItem>()
					.Where(x => x.Height.Id == height.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.Height, newHeight)
					.Update();

				progress.Add();
				uow.GetAll<CollectiveExpenseItem>()
					.Where(x => x.Height.Id == height.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.Height, newHeight)
					.Update();

				progress.Add();
				uow.GetAll<IncomeItem>()
					.Where(x => x.Height.Id == height.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.Height, newHeight)
					.Update();

				progress.Add();
				uow.GetAll<IssuanceSheetItem>()
					.Where(x => x.Height.Id == height.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.Height, newHeight)
					.Update();

				progress.Add();
				uow.GetAll<WriteoffItem>()
					.Where(x => x.Height.Id == height.Id)
					.Where(x => nomenclaturesIds.Contains(x.Nomenclature.Id))
					.UpdateBuilder()
					.Set(x => x.Height, newHeight)
					.Update();

				//Есть еще ссылки с SubdivisionIssueOperation но не понятно как это используется, возможно эти ссылки вообще не нужны.
				//Так же есть ссылка из Barcode, но пока не уверен, стоит ли менять размер после выпуска штрихкода.
			}
		}
	}
}
