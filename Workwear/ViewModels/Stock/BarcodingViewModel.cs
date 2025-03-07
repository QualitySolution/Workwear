using System;
using System.Collections.Generic;
using Autofac;
using NLog;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools;

namespace Workwear.ViewModels.Stock {
	public class BarcodingViewModel : EntityDialogViewModelBase<Barcoding> {
		public BarcodingViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, 
			IUserService userService,
			IInteractiveService interactive,
			ILifetimeScope autofacScope,
			BaseParameters baseParameters,
			IValidator validator = null)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator) {
			this.interactive = interactive;
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
				
			var entryBuilder = new CommonEEVMBuilderFactory<Barcoding>(this, Entity, UoW, navigation) {
				AutofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope))
			};
			
			if(Entity.Id == 0) {
				Entity.CreatedbyUser = userService.GetCurrentUser();
				logger.Info($"Создание Нового документа маркировки");
			} else 
				autoDocNumber = String.IsNullOrWhiteSpace(Entity.DocNumber);
		}
//1289		
		private IInteractiveService interactive; 
//1289
		private BaseParameters baseParameters;
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#region Для View
		public bool SensitiveDocNumber => !AutoDocNumber;
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumberText))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public bool AutoDocNumber {
			get => autoDocNumber;
			set => SetField(ref autoDocNumber, value);
		}

		public string DocNumberText {
			get => AutoDocNumber ? (Entity.Id == 0 ? "авто" : Entity.Id.ToString()) : Entity.DocNumberText;
			set { 
				if(!AutoDocNumber) 
					Entity.DocNumber = value; 
			}
		}

		#endregion
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}

		public void DeleteItem(BarcodingItem item) {
			Entity.RemoveItem(item);
			CalculateTotal();
		}
		
		public void AddItems() {
			
		}
		
		private void LoadItems(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			//var selectedNodes = e.GetSelectedObjects<>();
			
			//foreach (var node in selectedNodes) 
			//	Entity.AddItem(operations.FirstOrDefault(o => o.Id == node.Id), node.Percentage);
			CalculateTotal();
		}
		
		public override bool Save() {
			logger.Info ("Запись документа...");
			
			if(AutoDocNumber)
					Entity.DocNumber = null;
			
			foreach(var item in Entity.Items) {
				
				//UoW.Save(item.operation);
			}

			if(!base.Save()) {
				logger.Info("Не Ок.");
				return false;
			}

			logger.Info ("Ok");
			return true;
		}
		
		private void CalculateTotal() {
			Total = $"Позиций в документе: {Entity.Items.Count}";
		}
		
		public void Print() {
			if(UoW.HasChanges && !interactive.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = String.Format("Документ маркировки №{0}", Entity.DocNumber ?? Entity.Id.ToString()),
				Identifier = "Documents.BarcodingSheet",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};
			//NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
	}
}
