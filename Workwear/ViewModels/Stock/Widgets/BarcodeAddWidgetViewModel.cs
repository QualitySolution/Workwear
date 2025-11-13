using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Barcodes;

namespace Workwear.ViewModels.Stock.Widgets {
	public class BarcodeAddWidgetViewModel : UowDialogViewModelBase, IWindowDialogSettings {

		private readonly BaseParameters baseParameters;
		private readonly BarcodeRepository barcodeRepository;
		private readonly EmployeeIssueOperation issueOperation;
		private ExpenseDocItemsEmployeeViewModel docItemsVM;
		private ExpenseItem expenseItem;

		public BarcodeAddWidgetViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			BaseParameters baseParameters,
			BarcodeRepository barcodeRepository,
			ExpenseDocItemsEmployeeViewModel docItemsVM = null,
			ExpenseItem expenseItem = null,
			IValidator validator = null,
			string UoWTitle = null,
			UnitOfWorkProvider unitOfWorkProvider = null)
			: base(unitOfWorkFactory, navigation, validator, UoWTitle, unitOfWorkProvider) 
		{
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.barcodeRepository = barcodeRepository ?? throw new ArgumentNullException(nameof(barcodeRepository));
			this.docItemsVM = docItemsVM;
			this.expenseItem = expenseItem;
			_ = UoW; //Дёргаем, чтобы заполнился провайдер
		}

		#region Cвойства

		public virtual bool AutoAdd { get; set; } = true;

		public virtual bool CanEntry => expenseItem == null || 
		    (expenseItem.EmployeeIssueOperation?.BarcodeOperations?.Count ?? 0) + AddedBarcodes.Count < expenseItem.Amount ;
		public virtual bool CanAdd => CanEntry && ActiveBarcode != null;
		
		public virtual string CodeLabel => 
			baseParameters.ClothingMarkingType == BarcodeTypes.EAN13 ? "Штрихкод" :
			baseParameters.ClothingMarkingType == BarcodeTypes.EPC96 ? "Радиометка" : 
			"Макировка";

		private string chekcText = String.Empty;
		public virtual string CheckText {
			get => chekcText;
			set => SetField(ref chekcText, value);
		}
		private string chekcTextColor = String.Empty;
		public virtual string CheckTextColor {
			get => chekcTextColor;
			set => SetField(ref chekcTextColor, value);
		}

		public virtual IObservableList<Barcode> AddedBarcodes { get; } = new ObservableList<Barcode>();

		private Barcode activeBarcode;
		[PropertyChangedAlso(nameof(CanAdd))]
		public virtual Barcode ActiveBarcode {
			get => activeBarcode;
			set => SetField(ref activeBarcode, value);
		}

		private string barcodeText;
		public string BarcodeText {
			get => barcodeText;
			set {
				if(SetField(ref barcodeText, value.Trim())) {

					if(BarcodeService.CheckBarcode(barcodeText, baseParameters.ClothingMarkingType)) {
						var barcode = barcodeRepository.GetBarcodeByString(barcodeText);

						if(barcode != null) {
							if(barcode.BarcodeOperations.Count != 0) {
								//TODO в будующем при дополнении маркировки это тоже нужно отрабатывать
								CheckText = "Уже используется в другой операции";
								CheckTextColor = "purple";
							} else if(AddedBarcodes.Any(x => x.Title == barcodeText)) {
								CheckText = "Уже добавлено";
								CheckTextColor = "purple";
							} else {
								ActiveBarcode = barcode;
								if(AutoAdd)
									AddItem();
								else {
									CheckTextColor = "orange";
									CheckText = "Можно добавить";
								}
							}
						} else {
							if(baseParameters.ClothingMarkingType == BarcodeTypes.EAN13) {
								CheckTextColor = "red";
								CheckText = "Штрихкод не найден";
							}else if(AddedBarcodes.Any(x => x.Title == barcodeText)) {
                             							CheckText = "Уже добавлено";
                             							CheckTextColor = "purple";
                            } else {
								ActiveBarcode = new Barcode() { Title = barcodeText, Type = baseParameters.ClothingMarkingType };
								if(AutoAdd)
									AddItem();
								else {
									CheckTextColor = "orange";
									CheckText = "Можно добавить";
								}
							}
						}
					} else {
						if(ActiveBarcode == null)
							CheckText = String.Empty;
						ActiveBarcode = null;
					}
				}
			}
		}

		#endregion
		
		public void Accept() {
			if(docItemsVM != null && expenseItem != null) {
				UoW.Commit();
				docItemsVM.AddBarcodes(expenseItem, (List<Barcode>)AddedBarcodes);
			}
			Close(false, CloseSource.Save);
		}

		public void AddItem() {
			AddedBarcodes.Add(ActiveBarcode);
			CheckTextColor = "green";
			CheckText = "Добавлено";
			BarcodeText = String.Empty;
			OnPropertyChanged(nameof(CanEntry));
		}
		
		#region IWindowDialogSettings implementation
		public bool IsModal { get; } = true;
		public bool EnableMinimizeMaximize { get; } = false;
		public bool Resizable { get; } = true;
		public bool Deletable { get; } = true;
		public WindowGravity WindowPosition { get; } = WindowGravity.Center;
		
		#endregion

	}
}
