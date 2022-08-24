using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;

namespace workwear.ViewModels.Import 
{
	public class IncomeImportViewModel : UowDialogViewModelBase {
		private string file;
		private readonly IInteractiveService interactiveService;
		public IncomeImportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IInteractiveService interactiveService) : base(unitOfWorkFactory, navigation) {
			this.interactiveService = interactiveService;
		}

		public void LoadDocument(string filePatch) {
			if(filePatch.ToLower().EndsWith(".xml"))
				file = filePatch;
			else
				interactiveService.ShowMessage(ImportanceLevel.Error, "Формат файла не поддерживается");
		}
	}
}
