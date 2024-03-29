﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear;
using Workwear.Domain.Sizes;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Sizes
{
	public class SizeViewModel: EntityDialogViewModelBase<Size>
	{
		private readonly FeaturesService featuresService;
		public SizeService SizeService { get; }
		public SizeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			FeaturesService featuresService,
			SizeService sizeService,
			SizeType sizeType = null,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			SizeService = sizeService;
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, 
				new ValidationContext(Entity, new Dictionary<object, object> {{nameof(IUnitOfWork), UoW} })));
			if (UoW.IsNew) {
				Entity.SizeType = sizeType;
				IsNew = true;
			}
			else {
				if (Entity.Id <= SizeService.MaxStandardSizeId) IsStandard = true;
			}
			
			Entity.SuitableSizes.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(AllSuitable));
		}
		
		public IList<Size> AllSuitable => Entity.SuitableSizes.Union(Entity.SizesWhereIsThisSizeAsSuitable).ToList();

		public bool IsStandard { get; }
		public bool IsNew {get;}

		#region Для View
		public bool CanEdit => IsNew || !IsStandard;
		public bool CanEditAnalogs => featuresService.Available(WorkwearFeature.CustomSizes);
		#endregion

		public void AddAnalog() {
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<SizeJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.SelectedSizeType = Entity.SizeType;
			selectJournal.ViewModel.Filter.SensitiveSizeType = false;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		}

		private void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			var selects = e.GetSelectedObjects<SizeJournalNode>();
			var analogs = UoW.GetById<Size>(selects.Select(x => x.Id));
			foreach (var analog in analogs) {
				Entity.SuitableSizes.Add(analog);
			}
		}
		public void RemoveAnalog(Size analog) {
			if(Entity.SuitableSizes.Contains(analog))
				Entity.SuitableSizes.Remove(analog);
			else {
				Entity.SizesWhereIsThisSizeAsSuitable.Remove(analog);
				OnPropertyChanged(nameof(AllSuitable));
			}
		}
	}
}
