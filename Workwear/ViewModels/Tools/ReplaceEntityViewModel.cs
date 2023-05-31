using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Deletion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;

namespace Workwear.ViewModels.Tools
{
	public class ReplaceEntityViewModel : UowDialogViewModelBase
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private readonly ReplaceEntity replaceEntity;
		private readonly IGuiDispatcher guiDispatcher;

		public ReplaceEntityViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ILifetimeScope autofacScope, ReplaceEntity replaceEntity, IGuiDispatcher guiDispatcher, IValidator validator = null) : base(unitOfWorkFactory, navigation, validator)
		{
			Title = "Замена ссылок на объекты";
			this.replaceEntity = replaceEntity ?? throw new ArgumentNullException(nameof(replaceEntity));
			this.guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			var entryBuilder = new CommonEEVMBuilderFactory(this, UoW, navigation) {
				AutofacScope = autofacScope
			};
			
			TargetEntryViewModel = entryBuilder.ForEntity<ProtectionTools>().MakeByType().Finish();
			TargetEntryViewModel.Changed += TargetEntryViewModel_Changed;
		}

		#region View Properties
		public IProgressBarDisplayable Progress {
			get => replaceEntity.Progress;
			set => replaceEntity.Progress = value;
		}
		
		public IProgressBarDisplayable ProgressTotal { get; set; }

		private bool removeSource;
		public virtual bool RemoveSource {
			get => removeSource;
			set => SetField(ref removeSource, value);
		}

		public GenericObservableList<ReplaceEntityItem> SourceList = new GenericObservableList<ReplaceEntityItem>();
		#endregion

		#region Entry
		public EntityEntryViewModel<ProtectionTools> TargetEntryViewModel;
		#endregion

		#region Sensetive
		public bool SensitiveReplaceButton => !InProgress && SourceList.Any() && TargetEntryViewModel.Entity != null 
		                                      && SourceList.Sum(x => x.TotalLinks) > 0
		                                      && SourceList.All(x => !TargetEntryViewModel.Entity.IsSame(x.Entity)) ;
		#endregion

		#region Internal
		private bool inProgress;
		[PropertyChangedAlso(nameof(SensitiveReplaceButton))]
		public virtual bool InProgress {
			get => inProgress;
			set => SetField(ref inProgress, value);
		}
		#endregion

		#region Events
		void TargetEntryViewModel_Changed(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(SensitiveReplaceButton));
		}
		#endregion

		#region Действия View
		public void AddSourceItems()
		{
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Protection_OnSelectResult;
		}
		
		void Protection_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var ids = e.SelectedObjects.Select(x => x.GetId());
			var protections = UoW.GetById<ProtectionTools>(ids)
				.Where(p => SourceList.All(x => x.Entity.Id != p.Id)).ToList();
			
			guiDispatcher.RunInGuiTread(delegate {
				//Вызываем через очередь главного потока, чтобы успел закрыться журнал.
				//И пользователь видел прогресс в процессе поиска.
				InProgress = true;
				ProgressTotal.Start(protections.Count);
				foreach(var protection in protections) {
					ProgressTotal.Add(text: protection.Name);
					var item = new ReplaceEntityItem {
						Entity = protection,
						TotalLinks = replaceEntity.CalculateTotalLinks(UoW, protection)
					};
					SourceList.Add(item);
				}
				ProgressTotal.Close();
				InProgress = false;
			});
		}
		
		public void RemoveSourceItem(ReplaceEntityItem[] items)
		{
			foreach(var item in items)
				SourceList.Remove(item);
		}
		
		public void RunReplace()
		{
			InProgress = true;
			ProgressTotal.Start(SourceList.Count * 2 + (RemoveSource ? 2:1));
			int count = 0;
			foreach(var item in SourceList) {
				ProgressTotal.Add(text: item.Title);
				count += replaceEntity.ReplaceEverywhere(UoW, item.Entity, TargetEntryViewModel.Entity);
				ProgressTotal.Add();
				UoW.Commit();
			}
			
			if(RemoveSource) {
				ProgressTotal.Add(text: "Удаление исходных объектов");
				foreach(var item in SourceList) {
					UoW.Delete(item.Entity);
				}
				UoW.Commit();
			}
			ProgressTotal.Add();
			logger.Info("Заменено {0} ссылок.", count);
			SourceList.Clear();
			TargetEntryViewModel.CleanEntity();
			InProgress = false;
			ProgressTotal.Close();
		}
		#endregion
	}
	
	public class ReplaceEntityItem
	{
		public string Title => Entity.Name;
		public ProtectionTools Entity { get; set; }
		public int TotalLinks {get; set;}
	} 
}
