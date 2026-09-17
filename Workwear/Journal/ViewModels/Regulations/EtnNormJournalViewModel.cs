using System;
using System.Collections.Generic;
using System.Threading;
using QS.Cloud.WorkwearDictionary.Client;
using QS.Cloud.WorkwearDictionary.Grpc.Contracts;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;

namespace workwear.Journal.ViewModels.Regulations {
	public class EtnNormJournalViewModel : JournalViewModelBase {
		private readonly EtnDictionaryService etnDictionaryService;

		public EtnNormJournalViewModel(
			EtnDictionaryService etnDictionaryService,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			this.etnDictionaryService = etnDictionaryService ?? throw new ArgumentNullException(nameof(etnDictionaryService));
			Title = "Справочник ЕТН";
			DataLoader = new AnyDataLoader<Norm>(GetNodes, GetTotalCount);
		}

		private IList<Norm> GetNodes(int page, int pageSize, CancellationToken token) =>
			etnDictionaryService.GetNormsList(page, pageSize, SearchQuery).Norms;

		private int GetTotalCount(CancellationToken token) =>
			etnDictionaryService.GetNormsList(1, 1, SearchQuery).TotalCount;

		private string SearchQuery =>
			Search.SearchValues != null ? string.Join(" ", Search.SearchValues) : null;
	}
}
