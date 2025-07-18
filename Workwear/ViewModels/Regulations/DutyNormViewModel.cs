using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using NHibernate.Criterion;
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
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using Workwear.Models.Operations;
using Workwear.Repository.Regulations;
using Workwear.Tools;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Regulations {
	public class DutyNormViewModel : EntityDialogViewModelBase<DutyNorm>, IDialogDocumentation{
		
		private IInteractiveService interactive;
		private readonly IEntityChangeWatcher changeWatcher;
		public readonly DutyNormIssueModel dutyNormIssueModel;
		public readonly DutyNormRepository dutyNormRepository;
		public readonly EntityEntryViewModel<Subdivision> SubdivisionEntryViewModel;
		public readonly EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		public readonly EntityEntryViewModel<Leader> LeaderEntryViewModel;

		public DutyNormViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IInteractiveService interactive,
			IEntityChangeWatcher changeWatcher,
			DutyNormIssueModel dutyNormIssueModel,
			DutyNormRepository dutyNormRepository,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {

			this.dutyNormIssueModel = dutyNormIssueModel ?? throw new ArgumentNullException(nameof(dutyNormIssueModel));
			if(changeWatcher == null) throw new ArgumentNullException(nameof(changeWatcher));

			this.interactive = interactive;
			currentTab = Entity.Id == 0 ? 0 : 1;

			changeWatcher.BatchSubscribe(DutyNormChangeEvent)
				.IfEntity<DutyNormIssueOperation>()
				.AndWhere(op => op.DutyNorm.Id == Entity.Id);
			
			dutyNormRepository.LoadFullInfo(new[] {Entity.Id});
			var entryBuilder = new CommonEEVMBuilderFactory<DutyNorm>(this, Entity, UoW, navigation, autofacScope);
			SubdivisionEntryViewModel = entryBuilder.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
			EmployeeCardEntryViewModel = entryBuilder.ForProperty(x => x.ResponsibleEmployee)
				.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
			LeaderEntryViewModel = entryBuilder.ForProperty(x => x.ResponsibleLeader)
				.UseViewModelJournalAndAutocompleter<LeadersJournalViewModel>()
				.UseViewModelDialog<LeadersViewModel>()
				.Finish();
			
			dutyNormIssueModel.FillDutyNormItems(Entity.Items.ToArray());
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("regulations.html#duty-norms");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region Свойства
		
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
				if(item != null) {
					dutyNormIssueModel.FillDutyNormItems(new[] { item });
					item.UpdateNextIssue();
				}
			}
		}

		public void RemoveItem(DutyNormItem item) {
			Entity.Items.Remove(item); 
		}

		public void AddExpense() {
			if(!Save()) 
				return;
			NavigationManager.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder, DutyNorm>(this, EntityUoWBuilder.ForCreate(), Entity);
		}
		
		//Для синхронизации с изменениями внесёнными в базу при открытом диалоге.
		private void DutyNormChangeEvent(EntityChangeEvent[] changeevents) {
			foreach(var changeEvent in changeevents) {
				if(changeEvent.EventType != TypeOfChangeEvent.Insert) {
					var op = UoW.GetById<DutyNormIssueOperation>(changeEvent.Entity.GetId());
					if (op != null)
						UoW.Session.Evict(op);
				}
			}
			Entity.UpdateItems(dutyNormIssueModel);
		}

		public void OpenProtectionTools(DutyNormItem dutyNormItem) {
			NavigationManager.OpenViewModel<ProtectionToolsViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(dutyNormItem.ProtectionTools.Id));
		}
		
		public void OpenLastDocument(DutyNormItem dutyNormItem) {
			ExpenseDutyNormItem documentItemAlias = null;
			DutyNormIssueOperation operationAlias = null;
			ExpenseDutyNorm documentAlias = null;

			var result = UoW.Session.QueryOver<ExpenseDutyNormItem>(() => documentItemAlias)
				.JoinAlias(() => documentItemAlias.Document, () => documentAlias)
				.JoinAlias(() => documentItemAlias.Operation, () => operationAlias)
				.Where(() => operationAlias.ProtectionTools.Id == dutyNormItem.ProtectionTools.Id)
				.Where(() => documentAlias.DutyNorm.Id == dutyNormItem.DutyNorm.Id)
				.OrderBy(() => documentAlias.Date).Desc()
				.Take(1);
			var lastDocItem= result.List<ExpenseDutyNormItem>().FirstOrDefault();
			
			if (lastDocItem != null) 
				NavigationManager.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(lastDocItem.Document.Id));
			else 
				interactive.ShowMessage(ImportanceLevel.Error, "Не найдена ссылка на документ выдачи");
		}
		
		public void SaveAndPrint(DutyNormSheetPrint typeSheet) {
			if(!Save()) 
				return;
			
			var reportInfo = new ReportInfo {
				Title = (typeSheet == DutyNormSheetPrint.DutyNormPage1 ? $"Лицевая сторона карточки дежурной нормы" :
						typeSheet == DutyNormSheetPrint.DutyNormPage2 ? $"Оборотная сторона карточки дежурной нормы" :
						"Дежурная норма")
						+ $"  №{Entity.Id}",
				Identifier = typeSheet.GetAttribute<ReportIdentifierAttribute>().Identifier,
				Parameters = new Dictionary<string, object> {
					{ "duty_norm_id",  Entity.Id }
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
		/// <summary>
		/// Копирует существующую в базе дежурную норму по id
		/// </summary>
		public void CopyDutyNormFrom(int dutyNormId) {
			var dutyNorm = UoW.GetById<DutyNorm>(dutyNormId);
			Entity.CopyFromDutyNorm(dutyNorm);
			Entity.UpdateItems(dutyNormIssueModel);
		}
	}
	
	public enum DutyNormSheetPrint
	{
		[Display(Name = "Лицевая сторона")]
		[ReportIdentifier("DutyNorms.DutyNormPage1")]
		DutyNormPage1,
		[Display(Name = "Обратная сторона")]
		[ReportIdentifier("DutyNorms.DutyNormPage2")]
		DutyNormPage2,
	}
}
