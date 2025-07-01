using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.ClothingService;
using Workwear.Repository.Stock;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Postomats;

namespace Workwear.ViewModels.ClothingService {
	public class ClothingAddViewModel : UowDialogViewModelBase, IWindowDialogSettings {
		private readonly IUserService userService;
		private readonly BarcodeRepository barcodeRepository;
		private readonly Dictionary<string, (string title, Action<object> action)> actionBarcodes;
		private PostomatDocumentViewModel documentVM;
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }
		
		public ClothingAddViewModel(
			BarcodeRepository barcodeRepository,
			BarcodeInfoViewModel barcodeInfoViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			string UoWTitle = null,
			UnitOfWorkProvider unitOfWorkProvider = null,
			PostomatDocumentViewModel documentVm = null) 
			: base(unitOfWorkFactory, navigation, validator, UoWTitle, unitOfWorkProvider)
		{
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			this.documentVM = documentVm;
			
			Title = "Добавить в документ";
			BarcodeInfoViewModel.PropertyChanged += BarcodeInfoViewModelOnPropertyChanged;
		}

		
		#region Cвойства 
		private ServiceClaim activeClaim;
		[PropertyChangedAlso(nameof(CanAdd))]
		public virtual ServiceClaim ActiveClaim {
			get => activeClaim;
			set => SetField(ref activeClaim, value);
		}

		private IObservableList<AddServiceClaimNode> items = new ObservableList<AddServiceClaimNode>();
		public virtual IObservableList<AddServiceClaimNode> Items {
			get { return items; }
			set { SetField(ref items, value); }
		}

		public virtual IEnumerable<ServiceClaim> Claims =>
			Items.Where(x => x.Add).Select(x => x.Claim);
		public virtual IEnumerable<ServiceClaim> InDocClaims => documentVM.Entity.Items.Select(x => x.ServiceClaim).ToList();
		public virtual bool AutoAdd { get; set; } = true;
		public virtual bool CanAdd => ActiveClaim != null 
		                              && !Claims.Any(c => DomainHelper.EqualDomainObjects(c, ActiveClaim))
		                              && !InDocClaims.Any(c => DomainHelper.EqualDomainObjects(c, ActiveClaim));
		#endregion
		
		#region Методы 
		private void BarcodeInfoViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(BarcodeInfoViewModel.Barcode) == e.PropertyName) {
				if(BarcodeInfoViewModel.Barcode == null) {
					ActiveClaim = null;
					BarcodeInfoViewModel.LabelInfo = "Не найдено";
					return;
				}

				ActiveClaim = barcodeRepository.GetActiveServiceClaimFor(BarcodeInfoViewModel.Barcode);
				if(ActiveClaim == null)
					BarcodeInfoViewModel.LabelInfo = $"Спецодежда не была принята в стирку.";
				else if(!Claims.Contains(ActiveClaim) && documentVM != null && InDocClaims.Any(c => DomainHelper.EqualDomainObjects(c, ActiveClaim)))
					BarcodeInfoViewModel.LabelInfo = $"Спецодежда уже добавлена.";
				else if(AutoAdd) {
					Items.Add(new AddServiceClaimNode(ActiveClaim));
				}
			}
		}

		public void Accept() {
			documentVM.AddItems(Claims);
			Close(false, CloseSource.Save);
			Dispose();
		}

		public void AddClaim() {
			Items.Add(new AddServiceClaimNode(ActiveClaim));
		} 
		
		#endregion 
		
		
		#region IWindowDialogSettings implementation
		public bool IsModal { get; } = true;
		public bool EnableMinimizeMaximize { get; } = false;
		public bool Resizable { get; } = true;
		public bool Deletable { get; } = true;
		public WindowGravity WindowPosition { get; } = WindowGravity.Center;
		
		#endregion
	}

	public class AddServiceClaimNode {
		public AddServiceClaimNode(ServiceClaim claim) {
			Claim = claim;
		}
		
		public ServiceClaim Claim { get; }
		public bool Add { get; set; } = true;
		public string BarcodeText => Claim.Barcode.Title;
		public string EmployeeText => Claim.Employee.ShortName;
		public string NomenclatureText => Claim.Barcode.Nomenclature.Name;
		public string SizeText => SizeService.SizeTitle(Claim.Barcode.Size, Claim.Barcode.Height);
	}
}
