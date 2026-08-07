using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Barcodes;

namespace Workwear.ViewModels.ClothingService {
	public class BarcodeInfoViewModel : ViewModelBase {
		private readonly BarcodeRepository barcodeRepository;

		#region Свойства модели

		private string barcodeText;
		private readonly BaseParameters baseParameters;
		public Dictionary<string, (string title, Action action)> ActionBarcodes = new Dictionary<string, (string, Action)>();

		public BarcodeInfoViewModel(BarcodeRepository barcodeRepository, BaseParameters baseParameters) {
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(this.baseParameters));
		}

		public string BarcodeText {
			get => barcodeText;
			set {
				if(SetField(ref barcodeText, value.Trim())) {
					if(BarcodeService.CheckBarcode(barcodeText, baseParameters.ClothingMarkingType)) {
						if(ActionBarcodes.ContainsKey(barcodeText)) {
							LabelInfo = ActionBarcodes[barcodeText].title;
							ActionBarcodes[barcodeText].action();
							BarcodeText = String.Empty;
							return;
						}
						var barcode = barcodeRepository.GetBarcodeByString(barcodeText);
						if(barcode != null) {
							LabelInfo = null;
							Barcode = barcode;
							BarcodeText = String.Empty;
						}
						else {
							LabelInfo = "Метка не найдена";
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
					var lastOperation = barcode != null ? barcodeRepository.GetLastOperationAt(barcode) : null;
					Employee = lastOperation?.CurrentEmployee;
					Warehouse = lastOperation?.CurrentWarehouse;
					DutyNorm = lastOperation?.CurrentDutyNorm;
				}
			}
		}

		private EmployeeCard employee;
		[PropertyChangedAlso(nameof(LabelCurrent))]
		public virtual EmployeeCard Employee {
			get => employee;
			set { SetField(ref employee, value); }
		}

		private Warehouse warehouse;
		[PropertyChangedAlso(nameof(LabelCurrent))]
		public virtual Warehouse Warehouse {
			get => warehouse;
			set { SetField(ref warehouse, value); }
		}

		private DutyNorm dutyNorm;
		[PropertyChangedAlso(nameof(LabelCurrent))]
		public virtual DutyNorm DutyNorm {
			get => dutyNorm;
			set { SetField(ref dutyNorm, value); }
		}
		#endregion

		#region Свойства View

		public string LabelCurrent {
			get {
				if(Employee != null)
					return $"{Employee.FullName}";
				if(Warehouse != null)
					return $"«{Warehouse.Name}»";
				if(DutyNorm != null)
					return $"Дежурная норма №«{DutyNorm.Id}»";
				return null;
			}
		}
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
