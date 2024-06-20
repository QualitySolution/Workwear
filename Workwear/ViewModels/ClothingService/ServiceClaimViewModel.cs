using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.ClothingService;

namespace Workwear.ViewModels.ClothingService {
	public class ServiceClaimViewModel : EntityDialogViewModelBase<ServiceClaim> {
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }

		public ServiceClaimViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			BarcodeInfoViewModel barcodeInfoViewModel,
			INavigationManager navigation,
			PostomatManagerService postomatService,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null) 
			: base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) 
		{
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			if(postomatService == null) throw new ArgumentNullException(nameof(postomatService));
			BarcodeInfoViewModel.Barcode = Entity.Barcode;
			Postomats = postomatService.GetPostomatList(PostomatListType.Aso);
		}

		#region Postamat

		public IList<PostomatInfo> Postomats { get; }
		
		private PostomatInfo postomat;
		public virtual PostomatInfo Postomat {
			get => Postomats.FirstOrDefault(x => x.Id == Entity.PreferredTerminalId);
			set {
				if(SetField(ref postomat, value)) 
					Entity.PreferredTerminalId = value?.Id ?? 0;
			}
		}

		#endregion
		
		#region Sensitive
		public bool CanEdit => !Entity.IsClosed;
		public bool DefectCanEdit => CanEdit && Entity.NeedForRepair;
		#endregion

		#region Пропрос свойств модели

		public virtual bool IsClosed => Entity.IsClosed;
		public virtual IObservableList<StateOperation> States => Entity.States;
		
		public virtual bool NeedForRepair {
			get { return Entity.NeedForRepair; }
			set { if(Entity.NeedForRepair != value) {
					Entity.NeedForRepair = value;
					OnPropertyChanged(nameof(DefectCanEdit));
				}
			}
		}		
		
		public virtual string  Defect{
			get { return Entity.Defect; }
			set {if(Entity.Defect != value)
					Entity.Defect = value;
			}
		}
		
		public virtual string Comment {
			get { return Entity.Comment; }
			set {if(Entity.Comment != value)
					Entity.Comment = value;
			}
		}

		#endregion
	}
}
