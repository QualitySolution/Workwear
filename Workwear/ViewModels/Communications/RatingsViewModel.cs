using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Company;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Communications 
{
	public class RatingsViewModel : UowDialogViewModelBase
	{
		private readonly RatingManagerService ratingManagerService;
		private readonly EmployeeRepository employeeRepository;

		public RatingsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			RatingManagerService ratingManagerService,
			EmployeeRepository employeeRepository,
			Nomenclature nomenclature = null,
			IValidator validator = null) : base(unitOfWorkFactory, navigation, validator) 
		{
			this.ratingManagerService = ratingManagerService ?? throw new ArgumentNullException(nameof(ratingManagerService));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.employeeRepository.RepoUow = UoW;
			var builder = new CommonEEVMBuilderFactory<RatingsViewModel>(
				this, this, UoW, NavigationManager, autofacScope);

			EntryNomenclature = builder.ForProperty(x => x.SelectNomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclatureJournalViewModel>()
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
			
			SelectNomenclature = nomenclature == null ? null : UoW.GetById<Nomenclature>(nomenclature.Id);
		}

		#region Свойства

		private Nomenclature selectNomenclature;
		[PropertyChangedAlso(nameof(NomenclatureColumnVisible))]
		public Nomenclature SelectNomenclature {
			get => selectNomenclature;
			set {
				SetField(ref selectNomenclature, value);
				Title = selectNomenclature == null ? "Отзывы на выданное" : "Отзывы для: " + selectNomenclature.Name; 
				Ratings = ratingManagerService.GetRatings(value?.Id ?? -1);
				if(ratings.Any())
					RefreshNomenclatureNames(Ratings.Select(x => x.NomenclatureId).ToArray());
			}
		}
		
		public virtual bool NomenclatureColumnVisible => SelectNomenclature == null;
		public bool SensitiveOpenEmployee => SelectedRating != null;

		private IList<Rating> ratings;

		public IList<Rating> Ratings {
			get => ratings;
			set => SetField(ref ratings, value);
		}

		private Rating selectedRating;
		[PropertyChangedAlso(nameof(SensitiveOpenEmployee))]
		public Rating SelectedRating {
			get => selectedRating;
			set => SetField(ref selectedRating, value);
		}
		
		#endregion
		#region Названия номеклатуры

		private Dictionary<int, string> nomenclatureNames = new Dictionary<int, string>();
		public string GetNomenclatureName(Rating rating) {
			if(nomenclatureNames.TryGetValue(rating.NomenclatureId, out string name))
				return name;
			return null;
		}

		void RefreshNomenclatureNames(int[] ids) {
			nomenclatureNames = UoW.GetById<Nomenclature>(ids).ToDictionary(x => x.Id, x => x.Name);
		}
		#endregion
		
		#region Entry
		public EntityEntryViewModel<Nomenclature> EntryNomenclature;
		#endregion

		#region Команды
		public void OpenEmployee() {
			var employee = employeeRepository.GetEmployeeByPhone(SelectedRating.UserPhone);
			NavigationManager.OpenViewModel<EmployeeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(employee.Id));
		}
		#endregion
	}
}
