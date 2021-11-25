using System;
using Autofac;
using QS.Deletion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Regulations;

namespace workwear.ViewModels.Tools
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

			SourceEntryViewModel = entryBuilder.ForEntity<ProtectionTools>().MakeByType().Finish();
			TargetEntryViewModel = entryBuilder.ForEntity<ProtectionTools>().MakeByType().Finish();

			SourceEntryViewModel.Changed += SourceEntryViewModel_Changed;
			TargetEntryViewModel.Changed += TargetEntryViewModel_Changed;
		}

		#region View Properties
		public IProgressBarDisplayable Progress {
			get => replaceEntity.Progress;
			set => replaceEntity.Progress = value;
		}

		private bool removeSource;
		public virtual bool RemoveSource {
			get => removeSource;
			set => SetField(ref removeSource, value);
		}

		private int? totalLinks;
		[PropertyChangedAlso(nameof(TotalLinksText))]
		[PropertyChangedAlso(nameof(SensitiveReplaceButton))]
		public virtual int? TotalLinks {
			get => totalLinks;
			set => SetField(ref totalLinks, value);
		}

		public string TotalLinksText => TotalLinks.HasValue ? $"Найдено {TotalLinks} ссылок" : null;
		#endregion

		#region Entry
		public EntityEntryViewModel<ProtectionTools> SourceEntryViewModel;
		public EntityEntryViewModel<ProtectionTools> TargetEntryViewModel;
		#endregion

		#region Sensetive
		public bool SensitiveReplaceButton => SourceEntryViewModel.Entity != null && TargetEntryViewModel.Entity != null && TotalLinks > 0;
		#endregion

		#region Events
		void SourceEntryViewModel_Changed(object sender, EventArgs e)
		{
			guiDispatcher.RunInGuiTread(delegate { 
				//Вызываем через очередь главного потока, чтобы успел закрыться журнал.
				//И пользователь видел прогресс в процессе поиска.
				TotalLinks = SourceEntryViewModel.Entity != null ? replaceEntity.CalculateTotalLinks(UoW, SourceEntryViewModel.Entity) : (int?)null;
			});
		}

		void TargetEntryViewModel_Changed(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(SensitiveReplaceButton));
		}
		#endregion

		public void RunReplace()
		{
			var result = replaceEntity.ReplaceEverywhere(UoW, SourceEntryViewModel.Entity, SourceEntryViewModel.Entity);
			UoW.Commit();
			if(RemoveSource) {
				UoW.Delete(SourceEntryViewModel.Entity);
				UoW.Commit();
			}
			logger.Info("Заменено {0} ссылок.", result);
			SourceEntryViewModel.CleanEntity();
			TargetEntryViewModel.CleanEntity();
		}
	}
}
