using System;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;

namespace workwear.Journal.ViewModels.Postomats {
	public class FullnessJournalViewModel : JournalViewModelBase {
		private readonly PostomatManagerService postomatService;

		public FullnessJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			PostomatManagerService postomatService
			) : base(unitOfWorkFactory, interactiveService, navigation) {
			this.postomatService = postomatService ?? throw new ArgumentNullException(nameof(postomatService));
			Title = "Заполненность постоматов";
			SearchEnabled = false;
			
			DataLoader = new AnyDataLoader<FullnessInfo>(postomatService.GetFullness);
		}
	}
}
