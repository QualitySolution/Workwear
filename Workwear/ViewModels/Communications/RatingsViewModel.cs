﻿using System.Collections.Generic;
using Autofac;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.Communications 
{
	public class RatingsViewModel : UowDialogViewModelBase
	{
		private readonly RatingManagerService ratingManagerService;
		public RatingsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			RatingManagerService ratingManagerService,
			Nomenclature nomenclature = null,
			IValidator validator = null, 
			string UoWTitle = null) : base(unitOfWorkFactory, navigation, validator, UoWTitle) 
		{
			EntryNomenclatureVisible = true;
			this.ratingManagerService = ratingManagerService;
			var builder = new CommonEEVMBuilderFactory<RatingsViewModel>(
				this, this, UoW, NavigationManager, autofacScope);

			EntryNomenclature = builder.ForProperty(x => x.SelectNomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclatureJournalViewModel>()
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();

			if(nomenclature is null) {
				Title = "Рейтинг номеклатуры";
			}
			else {
				Title = "Рейтинг для номенклатуры: " + nomenclature.Name;
				SelectNomenclature = UoW.GetById<Nomenclature>(nomenclature.Id);
			}
		}

		#region Свойства

		private Nomenclature selectNomenclature;
		public Nomenclature SelectNomenclature {
			get => selectNomenclature;
			set {
				if(SetField(ref selectNomenclature, value))
					if(selectNomenclature is null)
						Ratings = new List<Rating>();
					else {
						Ratings = ratingManagerService.GetRatings(value.Id);
					}
			}
		}

		private IList<Rating> ratings;

		public IList<Rating> Ratings {
			get => ratings;
			set => SetField(ref ratings, value);
		}

		private bool entryNomenclatureVisible;
		public bool EntryNomenclatureVisible {
			get => entryNomenclatureVisible;
			set => SetField(ref entryNomenclatureVisible, value);
		}

		#endregion

		#region Entry

		public EntityEntryViewModel<Nomenclature> EntryNomenclature;

		#endregion
	}
}