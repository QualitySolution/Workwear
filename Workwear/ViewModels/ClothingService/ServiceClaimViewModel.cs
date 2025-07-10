using System;
using System.Collections.Generic;
using System.Linq;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control;
using QS.ViewModels.Dialog;
using Workwear.Domain.ClothingService;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.ClothingService {
	public class ServiceClaimViewModel : EntityDialogViewModelBase<ServiceClaim> {
		private readonly FeaturesService featuresService;
		public BarcodeInfoViewModel BarcodeInfoViewModel { get; }

		public ServiceClaimViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			BarcodeInfoViewModel barcodeInfoViewModel,
			INavigationManager navigation,
			FeaturesService featuresService,
			PostomatManagerService postomatService,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null) 
			: base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) 
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			BarcodeInfoViewModel = barcodeInfoViewModel ?? throw new ArgumentNullException(nameof(barcodeInfoViewModel));
			if(postomatService == null) throw new ArgumentNullException(nameof(postomatService));
			BarcodeInfoViewModel.Barcode = Entity.Barcode;
			if(this.featuresService.Available(WorkwearFeature.Postomats))
				Postomats = postomatService.GetPostomatList(PostomatListType.Aso);

			foreach(var service in Entity.Barcode.Nomenclature.UseServices) //Делаем список для заполнеия услуг во вьюшке
				services.Add(new SelectableEntity<Service>(service.Id, service.Name, entity:service)
					{Select = Entity.ProvidedServices.Any(provided => DomainHelper.EqualDomainObjects(service, provided))});

		}

		#region Postamat

		public IList<PostomatInfo> Postomats { get; } = new List<PostomatInfo>();
		
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

		#region Visible
		public bool PostomatVisible => featuresService.Available(WorkwearFeature.Postomats);
		#endregion

		#region Свойста и проброс из модели

		public virtual bool IsClosed => Entity.IsClosed;
		public virtual IObservableList<StateOperation> States => Entity.States;
		
		private IObservableList<SelectableEntity<Service>> services = new ObservableList<SelectableEntity<Service>>();
		public virtual IObservableList<SelectableEntity<Service>> Services {
			get => services;
			set => SetField(ref services, value);
		}
		
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

		#region Сохранение

		public override bool Save() {
			foreach(var s in services) {
				if(s.Select && !Entity.ProvidedServices.Any(provided => DomainHelper.EqualDomainObjects(s.Entity, provided)))
					Entity.ProvidedServices.Add(s.Entity);
				else if(!s.Select && Entity.ProvidedServices.Any(provided => DomainHelper.EqualDomainObjects(s.Entity, provided)))
                    Entity.ProvidedServices.RemoveAll(provided => DomainHelper.EqualDomainObjects(s.Entity, provided));
			}
			return base.Save();
		}

		#endregion
	}
}
