using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Stock {
	public class IncomeViewModel  : EntityDialogViewModelBase<Income> {
		public IncomeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			featuresService = autofacScope.Resolve<FeaturesService>();
			
			if(featuresService.Available(WorkwearFeature.Owners))
				Owners = UoW.GetAll<Owner>().ToList();
			if(featuresService.Available(WorkwearFeature.Warehouses))
				Warhoses = UoW.GetAll<Warehouse>().ToList();
			
			var entryBuilder = new CommonEEVMBuilderFactory<Income>(this, Entity, UoW, navigation, autofacScope);
			
			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}
		
		#region Проброс свойств документа

		public virtual int DocID => Entity.Id;
		public virtual string DocTitle => Entity.Title;
		public virtual string DocComment => Entity.Comment;
		public virtual UserBase DocCreatedbyUser => Entity.CreatedbyUser;
		public virtual DateTime DocDate => Entity.Date;
		public virtual Warehouse Warehouse => Entity.Warehouse;
		public virtual string NumberTN => Entity.Number;
		public virtual IObservableList<IncomeItem> Items => Entity.Items;

		#endregion

		#region Свойства ViewModel
		private readonly FeaturesService featuresService;
		private readonly SizeService sizeService = new SizeService();
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		
		//public IList<Owner> Owners = QS.DomainModel.UoW.UnitOfWorkFactory.CreateWithoutRoot().GetAll<Owner>().ToList();
		//private List<Owner> owners = new List<Owner>();
		public List<Owner> Owners {get;}
		public List<Warehouse> Warhoses {get;}
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumberText))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public virtual bool AutoDocNumber {
			get => autoDocNumber;
			set => SetField(ref autoDocNumber, value);
		}

		public virtual string DocNumberText {
			get => AutoDocNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.DocNumber;
			set => Entity.DocNumber = (AutoDocNumber || value == "авто") ? null : value;
		}

		#endregion 
		
		#region Свойства для View

		public virtual bool SensitiveDocNumber => !AutoDocNumber;
		public virtual bool OwnersVisible => featuresService.Available(WorkwearFeature.Owners);
		public virtual bool ReadInFileVisible => featuresService.Available(WorkwearFeature.Warehouses);
		public virtual bool WarehouseVisible => featuresService.Available(WorkwearFeature.Exchange1C);

		public virtual IList<Size> GetSizeVariants(IncomeItem item) {
			return sizeService.GetSize(UoW, item.WearSizeType, onlyUseInNomenclature: true).ToList();
		}
		
		public virtual IList<Size> GetHeightVariants(IncomeItem item) {
			return sizeService.GetSize(UoW, item.HeightType, onlyUseInNomenclature: true).ToList();
		}
		#endregion
	}
}
