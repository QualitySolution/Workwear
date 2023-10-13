using System;
using QS.DomainModel.Entity;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;
using Workwear.Repository.Stock;

namespace Workwear.ViewModels.ClothingService {
	public class BarcodeInfoViewModel : ViewModelBase {
		private readonly BarcodeRepository barcodeRepository;

		#region	Свойства модели
		private string barcodeText;

		public BarcodeInfoViewModel(BarcodeRepository barcodeRepository) {
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
		}

		public string BarcodeText {
			get => barcodeText;
			set {
				if(SetField(ref barcodeText, value.Trim())) {
					if(barcodeText.Length == 13) {
						var barcode = barcodeRepository.GetBarcodeByString(barcodeText);
						if(barcode != null) {
							LabelInfo = null;
							Barcode = barcode;
						}
						else {
							LabelInfo = "Штрихкод не найден";
							Barcode = null;
						}
					}
				}
			}
		}
		
		
		private Barcode barcode;
		[PropertyChangedAlso(nameof(LabelNomenclature))]
		[PropertyChangedAlso(nameof(LabelTitle))]
		[PropertyChangedAlso(nameof(LabelCreateDate))]
		[PropertyChangedAlso(nameof(LabelHeight))]
		[PropertyChangedAlso(nameof(LabelSize))]
		[PropertyChangedAlso(nameof(VisibleBarcode))]
		public virtual Barcode Barcode {
			get => barcode;
			set {
				if(SetField(ref barcode, value)) {
					Employee = barcode != null ? barcodeRepository.GetLastEmployeeFor(barcode) : null;
				}
			}
		}
		
		private EmployeeCard employee;
		[PropertyChangedAlso(nameof(LabelEmployee))]
		public virtual EmployeeCard Employee {
			get => employee;
			set { SetField(ref employee, value); }
		}
		#endregion
		
		#region	Свойства View
		public string LabelEmployee => Employee?.FullName;
		public string LabelNomenclature => Barcode?.Nomenclature.Name;
		public string LabelTitle => Barcode?.Title;
		public string LabelCreateDate => Barcode?.CreateDate.ToShortDateString();
		public string LabelHeight => Barcode?.Size?.Name;
		public string LabelSize => Barcode?.Size?.Name;

		private string labelInfo;
		[PropertyChangedAlso(nameof(VisibleInfo))]
		public string LabelInfo {
			get => labelInfo;
			set { SetField(ref labelInfo, value); }
		}

		#region Visible
		public bool VisibleBarcode => Barcode != null;
		public bool VisibleInfo => !String.IsNullOrEmpty(LabelInfo);
		#endregion
		#endregion
	}
}
