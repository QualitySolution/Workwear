using System;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Analytics;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;
using Workwear.ViewModels.Regulations;

namespace Workwear.ViewModels.Analytics 
{
	public class ProtectionToolsCategoryViewModel : EntityDialogViewModelBase<ProtectionToolsCategory> 
	{
		private readonly IInteractiveService interactive;
		private readonly IUnitOfWork uow;

		public ProtectionToolsCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IInteractiveService interactive,
			ILifetimeScope autofacScope,
			IValidator validator = null, 
			UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) 
		{
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
		}

		#region Commands
		public void AddProtectionTools() 
		{
			IPage<ProtectionToolsJournalViewModel> selectPage =
				NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += Nomenclature_OnSelectResult;
		}
		
		public void RemoveProtectionTools(ProtectionTools[] selected) 
		{
			Array.ForEach(selected, p => Entity.RemoveProtectionTools(p));
		}

		public void OpenProtectionTool(ProtectionTools tool) 
		{
			NavigationManager.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(tool.Id));
		}
		#endregion

		private void Nomenclature_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			ProtectionTools[] tools = 
				UoW.GetById<ProtectionTools>(e.SelectedObjects.Select(x => x.GetId())).ToArray();
			Array.ForEach(tools, p => Entity.AddProtectionTools(p));
		}
	}
}
