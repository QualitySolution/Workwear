using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.ViewModels;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Regulations;
using Workwear.ViewModels.Regulations;

namespace Workwear.ViewModels.Stock.NomenclatureChildren {
	public class NomenclatureProtectionToolsViewModel : ViewModelBase {
		private readonly NomenclatureViewModel parent;
		private readonly INavigationManager navigation;

		public NomenclatureProtectionToolsViewModel(NomenclatureViewModel parent, INavigationManager navigation) {
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}

		#region Helpers
		Nomenclature Entity => parent.Entity;
		IUnitOfWork UoW => parent.UoW;
		#endregion

		#region Свойства View
		public GenericObservableList<ProtectionTools> ObservableProtectionTools  => parent.Entity.ObservableProtectionTools;
		#endregion
		
		#region Действия View
		
		public void Add() {
			var selectJournal = navigation.OpenViewModel<ProtectionToolsJournalViewModel>(parent, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadProtectionTools;
		}

		private void LoadProtectionTools(object sender, JournalSelectedEventArgs e) {
			var list = UoW.GetById<ProtectionTools>(e.SelectedObjects.Select(x => x.GetId()));
			foreach(var protectionTools in list) {
				Entity.AddProtectionTools(protectionTools);
			}
		}
		
		public void Remove(ProtectionTools protectionTools) => Entity.RemoveProtectionTools(protectionTools);
		
		public void Open(ProtectionTools protectionTools) {
			navigation.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(parent, EntityUoWBuilder.ForOpen(protectionTools.Id));
		}
		#endregion
	}
}
