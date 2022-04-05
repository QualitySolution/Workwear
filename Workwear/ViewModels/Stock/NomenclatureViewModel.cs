using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.Tools;

namespace workwear.ViewModels.Stock
{
	public class NomenclatureViewModel : EntityDialogViewModelBase<Nomenclature>
	{
		public NomenclatureViewModel(
			BaseParameters baseParameters,
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			var entryBuilder = 
				new CommonEEVMBuilderFactory<Nomenclature>(this, Entity, UoW, navigation, autofacScope);

			ItemTypeEntryViewModel = entryBuilder.ForProperty(x => x.Type)
				.MakeByType()
				.Finish();
			Validations.Clear();
			Validations.Add(
				new ValidationRequest(Entity, 
					new ValidationContext(Entity, 
						new Dictionary<object, object> { { nameof(BaseParameters), baseParameters }, 
							{nameof(IUnitOfWork), UoW} })));

			Entity.PropertyChanged += Entity_PropertyChanged;
		}
		#region EntityViewModels
		public EntityEntryViewModel<ItemsType> ItemTypeEntryViewModel;
		#endregion
		#region Visible
		public bool VisibleClothesSex =>
			Entity.Type != null && Entity.Type.Category == ItemTypeCategory.wear;
		#endregion
		#region Sensitive
		public bool SensitiveOpenMovements => Entity.Id > 0;
		#endregion
		#region Data
		public string ClothesSexLabel => "Пол: ";
		#endregion
		#region Actions
		public void OpenMovements() {
			NavigationManager.OpenViewModel<StockMovmentsJournalViewModel>(this,
					addingRegistrations: builder => builder.RegisterInstance(Entity));
		}
		#endregion
		private void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName != nameof(Entity.Type)) return;
			if (Entity.Type != null && String.IsNullOrWhiteSpace(Entity.Name))
				Entity.Name = Entity.Type.Name;

			OnPropertyChanged(nameof(VisibleClothesSex));
			OnPropertyChanged(nameof(ClothesSexLabel));
		}
	}
}
