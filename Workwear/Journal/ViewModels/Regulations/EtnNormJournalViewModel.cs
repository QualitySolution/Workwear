using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QS.Cloud.WorkwearDictionary.Client;
using QS.Cloud.WorkwearDictionary.Grpc.Contracts;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using Workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations {
	public class EtnNormJournalViewModel : JournalViewModelBase {
		private const int PageSize = 200;

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
			DataLoader = new AnyDataLoader<Norm>(GetNodes);

			var createNormAction = new JournalAction("Создать норму",
				nodes => nodes.Length == 1,
				nodes => true,
				nodes => CreateNormFromEtn(nodes.Cast<Norm>().First()));
			NodeActionsList.Add(createNormAction);
			RowActivatedAction = createNormAction;
		}

		private IList<Norm> GetNodes(CancellationToken token) {
			var searchQuery = Search.SearchValues != null ? string.Join(" ", Search.SearchValues) : null;
			return etnDictionaryService.GetNormsList(1, PageSize, searchQuery).Norms;
		}

		private void CreateNormFromEtn(Norm etnNode) {
			var etnNorm = etnDictionaryService.GetNormItems(etnNode.NormId);
			var page = NavigationManager.OpenViewModel<NormViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			page.ViewModel.FillFromEtn(etnNorm);
		}
	}
}
