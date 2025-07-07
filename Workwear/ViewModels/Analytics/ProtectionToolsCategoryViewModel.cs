using System;
using System.Collections.Generic;
using System.Linq;
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
			selectPage.ViewModel.OnSelectResult += ProtectionTools_OnSelectResult;
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

		private void ProtectionTools_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			List<ProtectionTools> tools = 
				UoW.GetById<ProtectionTools>(e.SelectedObjects.Select(x => x.GetId())).ToList();
			
			if (tools.Any(IsCategoryAlreadyUse)) 
			{
				string message = tools.Where(IsCategoryAlreadyUse).Take(2).Count() > 1 ? 
					"У нескольких номенклатур нормы уже есть категории для аналитики. Заменить их?" : 
					"У одной номенклатуры нормы уже есть категория для аналитики. Заменить её?";

				if(!interactive.Question(message)) 
				{
					tools = tools.Where(p => !IsCategoryAlreadyUse(p)).ToList();
				}
			}
			
			tools.ForEach(p => Entity.AddProtectionTools(p));
		}

		private bool IsCategoryAlreadyUse(ProtectionTools tool) 
		{
			return tool.CategoryForAnalytic != null && (tool.CategoryForAnalytic.Id != Entity.Id || Entity.Id == 0);
		}
	}
}
