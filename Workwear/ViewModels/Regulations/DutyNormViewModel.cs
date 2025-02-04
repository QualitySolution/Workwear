using System;
using System.Collections.Generic;
using Gamma.Utilities;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Regulations {
	public class DutyNormViewModel : EntityDialogViewModelBase<DutyNorm>{
		
		private IInteractiveService interactive;
		private readonly IEntityChangeWatcher changeWatcher;
		public DutyNormViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IInteractiveService interactive,
			IEntityChangeWatcher changeWatcher,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null) 
			: base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider){
			this.interactive = interactive;
			currentTab = Entity.Id == 0 ? 0 : 1;
			
			if(changeWatcher == null) throw new ArgumentNullException(nameof(changeWatcher));
			changeWatcher.BatchSubscribe(DutyNormChangeEvent)
				.IfEntity<DutyNormIssueOperation>()
				.AndChangeType(TypeOfChangeEvent.Update)
				.AndWhere(op => op.DutyNorm.Id == Entity.Id);
			//Актуализация сроков при открытии			
			Entity.UpdateNextIssues(UoW);
		}
		
		#region Свойства

		public List<RegulationDoc> RegulationDocs { get; set; }
		
		private DutyNormItem selectedItem;
		public virtual DutyNormItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}

		private int currentTab;
		public virtual int CurrentTab {
			get => currentTab;
			set {
				SetField(ref currentTab, value);
			}
		}

		 public virtual IList<DutyNormIssueOperation> Operations => UoW.Session.QueryOver<DutyNormIssueOperation>()
			 .Where(o => o.DutyNorm.Id == Entity.Id).List();

		 #endregion
		
		#region Действия View
		public void AddItem() {
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Protection_OnSelectResult;
		}

		void Protection_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			foreach(var protectionNode in e.SelectedObjects) {
				var protectionTools = UoW.GetById<ProtectionTools>(protectionNode.GetId());
				var item = Entity.AddItem(protectionTools);
				item.UpdateNextIssue(UoW);
			}
		}

		public void RemoveItem(DutyNormItem item) {
			Entity.Items.Remove(item); 
		}

		public void AddExpense() {
			if(!Save())
				return;
			var vm = NavigationManager.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder, DutyNorm>(this, EntityUoWBuilder.ForCreate(), Entity);
		}
		
		//Для синхронизации с изменениями внесёнными в базу при открытом диалоге.
		private void DutyNormChangeEvent(EntityChangeEvent[] changeevents) {
			foreach(var changeEvent in changeevents) {
				var op = UoW.GetById<DutyNormIssueOperation>(changeEvent.Entity.GetId());
				UoW.Session.Refresh(op);
			}
			Entity.UpdateNextIssues(UoW);
		}

		public void OpenProtectionTools(DutyNormItem dutyNormItem) {
			NavigationManager.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(dutyNormItem.ProtectionTools.Id));
		}
		
		public void Print(DutyNormSheetPrint typeSheet) {
			if(UoW.HasChanges && !interactive.Question("Перед печатью изменения будут сохранены. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = (typeSheet == DutyNormSheetPrint.DutyNormPage1 ? $"Лицевая сторона карточки дежурной нормы" :
						typeSheet == DutyNormSheetPrint.DutyNormPage2 ? $"Оборотная сторона карточки дежурной нормы" :
						"Дежурная норма")
						+ $"  №{Entity.Id}",
				Identifier = typeSheet.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
		public void ShowLegend() {
			MessageDialogHelper.RunInfoDialog(
				"Количество:\n" +
				"<span color='black'>●</span> — потребность удовлетворена\n" +
				"<span color='blue'>●</span> — выдано больше нонрмы\n" +
				"<span color='orange'>●</span> — выданого количества не достаточно\n" +
				"<span color='red'>●</span> — не числится ничего\n" +
				"\nДата след. выдачи:\n" +
				"<span color='black'>●</span> — потребность удовлетворена\n" +
				"<span color='darkred'>●</span> — требуется выдача\n" +
				"<span color='orange'>●</span> — выдача потребуется в ближайшие 10 дней\n"
			);
		}
		#endregion
	}
}
