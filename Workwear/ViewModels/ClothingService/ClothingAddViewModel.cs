using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Postomats;
using Workwear.Repository.Stock;
using Workwear.ViewModels.Postomats;

namespace Workwear.ViewModels.ClothingService {
	public class ClothingAddViewModel : UowDialogViewModelBase, IWindowDialogSettings {
		private readonly IUserService userService;
		private readonly BarcodeRepository barcodeRepository;
		private readonly Dictionary<string, (string title, Action<object> action)> ActionBarcodes;
		private PostomatDocumentViewModel DocumentVM;
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
			this.DocumentVM = documentVm;
			
			Title = "Добавить в документ";
//BarcodeInfoViewModel.
			BarcodeInfoViewModel.PropertyChanged += BarcodeInfoViewModelOnPropertyChanged;
		}

		
		#region Cвойства 
		ServiceClaim claim;
		public virtual ServiceClaim Claim {
			get => claim;
			set => SetField(ref claim, value);
		}

		private PostomatDocument document => DocumentVM?.Entity;
		
		#endregion
		
		#region Методы 
		private void BarcodeInfoViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(nameof(BarcodeInfoViewModel.Barcode) == e.PropertyName) {
				if(BarcodeInfoViewModel.Barcode == null) {
					Claim = null;
					BarcodeInfoViewModel.LabelInfo = "Не найдено";
					return;
				}

				Claim = barcodeRepository.GetActiveServiceClaimFor(BarcodeInfoViewModel.Barcode);
				if(Claim == null)
					BarcodeInfoViewModel.LabelInfo = $"Спецодежда не была принята в стирку.";
				else if(document != null && document.Items.Select(i => i.ServiceClaim).Any(c => DomainHelper.EqualDomainObjects(c, Claim)))
					BarcodeInfoViewModel.LabelInfo = $"Спецодежда уже добавлена.";
				else if(document != null) {
					var b = DocumentVM.AvailableCells().FirstOrDefault();
					var c = userService.GetCurrentUser();
					document.AddItem(Claim, b, c);
					document.AddItem(Claim, DocumentVM.AvailableCells().FirstOrDefault(),null);
				}
			}
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
}
