using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.Tools;

namespace workwear.ViewModels.Stock
{
	public class NomenclatureViewModel : EntityDialogViewModelBase<Nomenclature>
	{
		private readonly ILifetimeScope autofacScope;

		public NomenclatureViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ILifetimeScope autofacScope, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var entryBuilder = new CommonEEVMBuilderFactory<Nomenclature>(this, Entity, UoW, navigation, autofacScope);

			ItemTypeEntryViewModel = entryBuilder.ForProperty(x => x.Type)
				.MakeByType()
				.Finish();

			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		public NomenclatureViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ILifetimeScope autofacScope, IValidator validator = null, LineIncome lineIncome = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var entryBuilder = new CommonEEVMBuilderFactory<Nomenclature>(this, Entity, UoW, navigation, autofacScope);

			ItemTypeEntryViewModel = entryBuilder.ForProperty(x => x.Type)
				.MakeByType()
				.Finish();

			Entity.PropertyChanged += Entity_PropertyChanged;

			if(lineIncome != null) {
				Entity.Name = lineIncome.Name;
				Entity.Ozm = lineIncome.Ozm;
			}
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
						Entity.SizeStd = Entity.WearGrowthStd = null;
					}
				}
			}
			if(e.PropertyName == nameof(Entity.Sex)) {
				OnPropertyChanged(nameof(SensitiveSizeStd));
				OnPropertyChanged(nameof(SizeStdEnum));

				var growthStd = SizeHelper.GetGrowthStandart(Entity.Type.WearCategory.Value, Entity.Sex.Value);
				if(growthStd != null) {
					Entity.WearGrowthStd = SizeHelper.GetSizeStdCode(growthStd);
				}
				else {
					Entity.WearGrowthStd = null;
				}
			}
		}
	}
}
