using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Tools.Features;

namespace workwear.ViewModels.Stock
{
	public class ItemTypeViewModel : EntityDialogViewModelBase<ItemsType>
	{
		private readonly FeaturesService featuresService;

		public ItemTypeViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, FeaturesService featuresService, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		#region Visible

		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense) && Entity.Category == ItemTypeCategory.wear;
		public bool VisibleWearCategory => Entity.Category == ItemTypeCategory.wear;

		#endregion

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch(e.PropertyName) {
				case nameof(Entity.Category):
					OnPropertyChanged(nameof(VisibleWearCategory));
					OnPropertyChanged(nameof(VisibleIssueType));
					break;
			}
		}

	}
}
