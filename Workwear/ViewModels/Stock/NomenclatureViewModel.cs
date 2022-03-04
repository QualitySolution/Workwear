using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Measurements;
using Workwear.Measurements;
using workwear.Repository.Stock;
using workwear.Tools;

namespace workwear.ViewModels.Stock
{
	public class NomenclatureViewModel : EntityDialogViewModelBase<Nomenclature>
	{
		private readonly ILifetimeScope autofacScope;
		private readonly BaseParameters baseParameters;
		private readonly IInteractiveService interactiveService;

		public NomenclatureViewModel(BaseParameters baseParameters, IInteractiveService interactiveService, IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ILifetimeScope autofacScope, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var entryBuilder = new CommonEEVMBuilderFactory<Nomenclature>(this, Entity, UoW, navigation, autofacScope);

			ItemTypeEntryViewModel = entryBuilder.ForProperty(x => x.Type)
				.MakeByType()
				.Finish();
			this.baseParameters = baseParameters;
			this.interactiveService = interactiveService;

			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		#region EntityViewModels
		public EntityEntryViewModel<ItemsType> ItemTypeEntryViewModel;
		#endregion

		#region Visible
		public bool VisibleClothesSex => Entity.Type != null && Entity.Type.Category == ItemTypeCategory.wear && Entity.Type.WearCategory.HasValue;
		public bool VisibleSizeStd => Entity.Type?.WearCategory != null && SizeHelper.HasСlothesSizeStd(Entity.Type.WearCategory.Value);
		#endregion

		#region Sensitive
		public bool SensitiveSizeStd => Entity.Type != null 
			&& Entity.Type.Category == ItemTypeCategory.wear 
			&& Entity.Type.WearCategory.HasValue
			&& SizeStdEnum != null;
		public bool SensitiveOpenMovements => Entity.Id > 0;
		#endregion

		#region Data
		public string ClothesSexLabel => Entity.Type?.WearCategory?.GetEnumTitle() + ":";

		public object[] DisableClothesSex {
			get {
				var standarts = SizeHelper.GetStandartsForСlothes(Entity.Type.WearCategory.Value);
				var toHide = new List<object>();
				foreach(var sexInfo in typeof(ClothesSex).GetFields()) {
					if(sexInfo.Name.Equals("value__"))
						continue;

					var sexEnum = (ClothesSex)sexInfo.GetValue(null);
					if(!standarts.Any(x => x.Sex == sexEnum && x.Use != SizeUse.HumanOnly))
						toHide.Add(sexEnum);
				}
				return toHide.ToArray();
			}
		}

		public Type SizeStdEnum => Entity.Type?.WearCategory != null && Entity.Sex != null 
			? SizeHelper.GetSizeStandartsEnum(Entity.Type.WearCategory.Value, Entity.Sex.Value)
			: null;

		#endregion

		#region Actions
		public void OpenMovements()
		{
			NavigationManager.OpenViewModel<StockMovmentsJournalViewModel>(this,
					addingRegistrations: builder => builder.RegisterInstance(Entity));
		}
		#endregion

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Type)) {
				if(Entity.Type != null && String.IsNullOrWhiteSpace(Entity.Name))
					Entity.Name = Entity.Type.Name;

				OnPropertyChanged(nameof(VisibleClothesSex));
				OnPropertyChanged(nameof(ClothesSexLabel));
				OnPropertyChanged(nameof(VisibleSizeStd));
				OnPropertyChanged(nameof(SensitiveSizeStd));
				OnPropertyChanged(nameof(DisableClothesSex));

				if(Entity.Type != null && Entity.Type.Category == ItemTypeCategory.wear && Entity.Type.WearCategory.HasValue) {
					if(!SizeHelper.HasСlothesSizeStd(Entity.Type.WearCategory.Value)) {
						Entity.SizeStd = null;
					}
				}
			}
			if(e.PropertyName == nameof(Entity.Sex)) {
				OnPropertyChanged(nameof(SensitiveSizeStd));
				OnPropertyChanged(nameof(SizeStdEnum));
			}
		}
		public override bool Save() {
			if (!Entity.Archival) return true;
			if (!baseParameters.CheckBalances) return true;
			var repository = new StockRepository();
			var nomenclatures = new List<Nomenclature>() {Entity};
			var inStocks = new List<StockBalanceDTO>();
			var warehouses = UoW.Query<Warehouse>().List();
			foreach (var warehouse in warehouses) { inStocks.AddRange(repository.StockBalances(UoW, warehouse, nomenclatures, DateTime.Now)); }
			if (!inStocks.Any(x => x.Amount > 0)) return true;
			interactiveService.ShowMessage(ImportanceLevel.Error, "Архивная номенклатура не должна иметь остатков на складе");
			return false;
		}
	}
}
