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
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Stock;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Postomats;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.ClothingService {
	public class ClothingAddViewModel : UowDialogViewModelBase, IWindowDialogSettings {
		private readonly IUserService userService;
		private readonly BarcodeRepository barcodeRepository;
		private readonly Dictionary<string, (string title, Action<object> action)> actionBarcodes;
		private readonly PostomatDocumentViewModel postomatDocVM;
		private readonly OverNormViewModel overNormDocVM;
		private readonly ReturnViewModel returnDocVM;
		private readonly WriteOffViewModel writeoffDocVM;
		private readonly OverNormItem overNormItem;
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }

		public ClothingAddViewModel(
			BarcodeRepository barcodeRepository,
			BarcodeInfoViewModel barcodeInfoViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			//Следующие 4 модели по сути являются обязательными, вернее обязательна одна из них, но стальные должны быть принудительно переданы как null чтобы их не сознавал DI
			PostomatDocumentViewModel postomatDocVm,
			OverNormViewModel overNormDocVm,
			ReturnViewModel returnDocVm,
			WriteOffViewModel writeoffDocVm,
			IValidator validator = null,
			string UoWTitle = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(unitOfWorkFactory, navigation, validator, UoWTitle, unitOfWorkProvider)
		{
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			barcodeInfoViewModel.ActionBarcodes = SetActionBarcodes();

			this.postomatDocVM = postomatDocVm;
			this.overNormDocVM = overNormDocVm;
			this.returnDocVM = returnDocVm;
			this.writeoffDocVM = writeoffDocVm;
			overNormItem = overNormDocVm?.SelectedItem;
			_ = UoW; //Дёргаем, чтобы заполнился провайдер
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

		private IObservableList<IAddMarkNode> items = new ObservableList<IAddMarkNode>();
		public virtual IObservableList<IAddMarkNode> Items {
			get { return items; }
			set { SetField(ref items, value); }
		}

		public virtual IEnumerable<ServiceClaim> Claims =>
			Items.OfType<AddMarkServiceClaimNode>().Where(x => x.Add).Select(x => x.Claim);
		public virtual IEnumerable<ServiceClaim> InDocClaims =>
			postomatDocVM?.Entity.Items.Select(x => x.ServiceClaim).ToList()
			?? returnDocVM?.Entity.Items.Where(x => x.ServiceClaim != null).Select(x => x.ServiceClaim).ToList()
			?? Enumerable.Empty<ServiceClaim>();
		public virtual IEnumerable<Barcode> ScannedBarcodes =>
			Items.OfType<AddMarkBarcodeNode>().Where(x => x.Add).Select(x => x.Barcode);
		public virtual bool AutoAdd { get; set; } = true;
		public virtual bool CanAdd {
			get {
				if(overNormDocVM != null)
					return BarcodeInfoViewModel.Barcode != null
					       && string.IsNullOrEmpty(BarcodeInfoViewModel.LabelInfo)
					       && !Items.OfType<AddMarkBarcodeNode>().Any(x => x.Barcode.Id == BarcodeInfoViewModel.Barcode.Id);
				if(postomatDocVM != null)
					return ActiveClaim != null
					       && !Claims.Any(c => DomainHelper.EqualDomainObjects(c, ActiveClaim))
					       && !InDocClaims.Any(c => DomainHelper.EqualDomainObjects(c, ActiveClaim));
				if(returnDocVM != null) {
					if(ActiveClaim != null)
						return !Claims.Any(c => DomainHelper.EqualDomainObjects(c, ActiveClaim))
						       && !InDocClaims.Any(c => DomainHelper.EqualDomainObjects(c, ActiveClaim));
					return BarcodeInfoViewModel.Barcode != null
					       && string.IsNullOrEmpty(BarcodeInfoViewModel.LabelInfo)
					       && !Items.OfType<AddMarkBarcodeNode>().Any(x => x.Barcode.Id == BarcodeInfoViewModel.Barcode.Id);
				}
				if(writeoffDocVM != null)
					return BarcodeInfoViewModel.Barcode != null
					       && string.IsNullOrEmpty(BarcodeInfoViewModel.LabelInfo)
					       && !Items.OfType<AddMarkBarcodeNode>().Any(x => x.Barcode.Id == BarcodeInfoViewModel.Barcode.Id);
				return false;
			}
		}
		#endregion

		#region Методы
		private void BarcodeInfoViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(BarcodeInfoViewModel.Barcode) != e.PropertyName)
				return;
			
			var barcode = BarcodeInfoViewModel.Barcode;
			if(barcode == null) {
				ActiveClaim = null;
				BarcodeInfoViewModel.LabelInfo = "Не найдено";
				OnPropertyChanged(nameof(CanAdd));
				return;
			}

			if(overNormDocVM != null) {
				if(Items.OfType<AddMarkBarcodeNode>().Any(x => x.Barcode.Id == barcode.Id)) {
					BarcodeInfoViewModel.LabelInfo = $"Спецодежда {barcode.Title} уже в списке.";
					return;
				}

				var error = overNormDocVM.ValidateBarcodeForScan(barcode);
				if(error != null) {
					BarcodeInfoViewModel.LabelInfo = error;
					return;
				}

				BarcodeInfoViewModel.LabelInfo = null;
				if(AutoAdd)
					Items.Add(new AddMarkBarcodeNode(barcode, overNormItem?.Employee));
				
				OnPropertyChanged(nameof(CanAdd));
				return;
			}

			if(writeoffDocVM != null) {
				if(Items.OfType<AddMarkBarcodeNode>().Any(x => x.Barcode.Id == barcode.Id)) {
					BarcodeInfoViewModel.LabelInfo = $"Метка {barcode.Title} уже в списке.";
					return;
				}

				var writeoffError = writeoffDocVM.ValidateBarcodeForScan(barcode);
				if(writeoffError != null) {
					BarcodeInfoViewModel.LabelInfo = writeoffError;
					return;
				}

				BarcodeInfoViewModel.LabelInfo = null;
				if(AutoAdd)
					Items.Add(new AddMarkBarcodeNode(barcode, BarcodeInfoViewModel.Employee));

				OnPropertyChanged(nameof(CanAdd));
				return;
			}

			if(postomatDocVM != null || returnDocVM != null) {
				ActiveClaim = barcodeRepository.GetActiveServiceClaimFor(barcode);
				if(ActiveClaim != null) {
					if(!Claims.Contains(ActiveClaim) && InDocClaims.Any(c => DomainHelper.EqualDomainObjects(c, ActiveClaim)))
						BarcodeInfoViewModel.LabelInfo = $"Спецодежда уже добавлена.";
					else if(AutoAdd) {
						Items.Add(new AddMarkServiceClaimNode(ActiveClaim));
					}
					return;
				}

				if(Items.OfType<AddMarkBarcodeNode>().Any(x => x.Barcode.Id == barcode.Id)) {
					BarcodeInfoViewModel.LabelInfo = $"Метка {barcode.Title} уже в списке.";
					return;
				}

				var error = returnDocVM.ValidateBarcodeForScan(barcode);
				if(error != null) {
					BarcodeInfoViewModel.LabelInfo = error;
					return;
				}

				BarcodeInfoViewModel.LabelInfo = null;
				if(AutoAdd)
					Items.Add(new AddMarkBarcodeNode(barcode, BarcodeInfoViewModel.Employee));

				OnPropertyChanged(nameof(CanAdd));
			}
		}

		public void Accept() {
			postomatDocVM?.AddItems(Claims);
			returnDocVM?.AddItems(Claims);
			foreach(var barcode in ScannedBarcodes.ToList()) {
				overNormDocVM?.AddBarcode(overNormItem, barcode);
				returnDocVM?.AddBarcode(barcode);
				writeoffDocVM?.AddBarcode(barcode);
			}
			Close(false, CloseSource.Save);
		}

		public void AddClaim() {
			if(overNormDocVM != null) {
				if(BarcodeInfoViewModel.Barcode != null)
					Items.Add(new AddMarkBarcodeNode(BarcodeInfoViewModel.Barcode, overNormItem?.Employee));
				return;
			}
			if(returnDocVM != null && ActiveClaim == null) {
				if(BarcodeInfoViewModel.Barcode != null)
					Items.Add(new AddMarkBarcodeNode(BarcodeInfoViewModel.Barcode, BarcodeInfoViewModel.Employee));
				return;
			}
			if(writeoffDocVM != null) {
				if(BarcodeInfoViewModel.Barcode != null)
					Items.Add(new AddMarkBarcodeNode(BarcodeInfoViewModel.Barcode, BarcodeInfoViewModel.Employee));
				return;
			}
			Items.Add(new AddMarkServiceClaimNode(ActiveClaim));
		}

		private Dictionary<string, (string, Action)> SetActionBarcodes() {
			//Полный список в ClothingMoveViewModel
			return new Dictionary<string, (string, Action)>() {
				["2000000000206"] = ("Применить", () => Accept()),
				["2000000000213"] = ("Добавить", () => AddClaim()),
			};
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

	/// <summary>
	/// Интерфейс  для добавления со сканера как заявок на обслуживание так и промаркированных позиций
	/// </summary>
	public interface IAddMarkNode {
		bool Add { get; set; }
		string BarcodeText { get; }
		string EmployeeText { get; }
		string NomenclatureText { get; }
		string SizeText { get; }
	}

	public class AddMarkServiceClaimNode : IAddMarkNode {
		public AddMarkServiceClaimNode(ServiceClaim claim) {
			Claim = claim;
		}

		public ServiceClaim Claim { get; }
		public bool Add { get; set; } = true;
		public string BarcodeText => Claim.Barcode.Title;
		public string EmployeeText => Claim.Employee.ShortName;
		public string NomenclatureText => Claim.Barcode.Nomenclature.Name;
		public string SizeText => SizeService.SizeTitle(Claim.Barcode.Size, Claim.Barcode.Height);
	}

	public class AddMarkBarcodeNode : IAddMarkNode {
		public AddMarkBarcodeNode(Barcode barcode, EmployeeCard employee) {
			Barcode = barcode;
			Employee = employee;
		}

		public Barcode Barcode { get; }
		public EmployeeCard Employee { get; }
		public bool Add { get; set; } = true;
		public string BarcodeText => Barcode.Title;
		public string EmployeeText => Employee?.ShortName;
		public string NomenclatureText => Barcode.Nomenclature.Name;
		public string SizeText => SizeService.SizeTitle(Barcode.Size, Barcode.Height);
	}
}
