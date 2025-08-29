using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Visits;

namespace Workwear.ViewModels.Visits {
	public class IssuanceRequestViewModel: EntityDialogViewModelBase<IssuanceRequest> {
		public IssuanceRequestViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation,
			IUserService userService,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null): base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			if(Entity.Id == 0)
				Entity.CreatedByUser = userService.GetCurrentUser();
			
		}

		#region Проброс свойств документа
		public virtual string Id => Entity.Id != 0 ? Entity.Id.ToString() : "Новый";
		public virtual UserBase CreatedByUser => Entity.CreatedByUser;
		public virtual DateTime ReceiptDate {
			get => Entity.ReceiptDate;
			set => Entity.ReceiptDate = value;
		}
		public IssuanceRequestStatus Status {
			get => Entity.Status;
			set => Entity.Status = value;
		}

		public string Comment {
			get => Entity.Comment;
			set => Entity.Comment = value;
		}
		#endregion
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		#region Валидация, сохранение

		public override bool Save() {
			logger.Info ("Запись заявки...");
			
			logger.Info("Валидация...");
			if(!Validate()) {
				logger.Warn("Валидация не пройдена, сохранение отменено.");
				return false;
			} else 
				logger.Info("Валидация пройдена.");
			if(Entity.Id == 0) 
				Entity.CreationDate = DateTime.Today;
			UoW.Save();
			logger.Info("Заявка сохранена");
			return true;
		}
		
		#endregion
	}
}
