using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Autofac;
using MySqlConnector;
using NHibernate;
using NLog;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using QSProjectsLib;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Postomats;
using Workwear.Tools;

namespace Workwear.ViewModels.Postomats {
	public class PostomatDocumentWithdrawViewModel : EntityDialogViewModelBase<PostomatDocumentWithdraw>, IDialogDocumentation
	{
		private readonly PostomatManagerService postomatService;
		private readonly IUserService userService;
		private readonly Logger logger;
		private readonly IInteractiveService interactive;
		private IList<PostomatInfo> postomats;

		public PostomatDocumentWithdrawViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			PostomatManagerService postomatService,
			ILifetimeScope autofacScope,
			IInteractiveService interactive,
			IUserService userService,
			IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) 
		{
			this.postomatService = postomatService ?? throw new ArgumentNullException(nameof(postomatService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			logger = LogManager.GetCurrentClassLogger();
			postomats = postomatService.GetPostomatList(PostomatListType.Laundry);
			
			foreach(PostomatDocumentWithdrawItem item in Entity.Items) 
			{
				item.Postomat = postomats.FirstOrDefault(p => p.Id == item.TerminalId);
			}
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("postomat.html#postamat-pickup-document");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region View Properties

		public bool CanEdit => !ExistsEntity();

		private bool ExistsEntity() 
		{
			PostomatDocumentWithdraw docs = UoW.Session.QueryOver<PostomatDocumentWithdraw>()
				.Where(x => x.Id == Entity.Id)
				.SingleOrDefault();

			return docs?.Items.Any() ?? false;
		}
			
		#endregion
		
		#region Commands

		public void RemoveItem(PostomatDocumentWithdrawItem item) 
		{
			Entity.Items.Remove(item);
		}

		public void TryFillData() 
		{
			if(Entity.Items.Any() && !interactive.Question("Это действие приведет к отмене всех изменений. Перезаписать данные?")) 
			{
				return;
			}

			Entity.Items.Clear();
			DbDataReader rdr = null;
			try 
			{
				string sql = @"
			             select terminal_id,
			             CONCAT_WS(' ', employees.last_name, employees.first_name, employees.patronymic_name) as fio,
			             nomenclature.name,
			             claim_id
			             from clothing_service_claim
			             	left join clothing_service_states on clothing_service_claim.id = clothing_service_states.claim_id
			             	left join barcodes on barcodes.id = clothing_service_claim.barcode_id
			             	left join nomenclature on nomenclature.id = barcodes.nomenclature_id
			             	left join employees on employees.id = clothing_service_claim.employee_id
			             where state = 'InReceiptTerminal' and
			              NOT EXISTS (select * 
			              			  from clothing_service_states as cs 
			             			  where cs.claim_id = clothing_service_states.claim_id AND
			             			   cs.id != clothing_service_states.id);
			             ";
				QSMain.CheckConnectionAlive();
				DbCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				rdr = cmd.ExecuteReader();
				FillData(rdr);

				if(!Entity.Items.Any()) 
				{
					interactive.ShowMessage(ImportanceLevel.Info, "Нет данных для автозаполнения");	
				}
			}
			catch(Exception ex) 
			{
				logger.Error(ex, "Ошибка получения информации для ведомости забора спецоджды на стирку");
				interactive.ShowMessage(ImportanceLevel.Error, 
					"Ошибка получения информации для ведомости забора спецоджды на стирку!", 
					"Ошибка автозаполнения");
			}
			finally 
			{
				rdr?.Close();
			}
		}

		private void FillData(DbDataReader rdr) 
		{
			while(rdr.Read())
			{
				uint claimId = (uint)rdr["claim_id"];
				uint terminalId = (uint)rdr["terminal_id"];
				ServiceClaim serviceClaim = UoW.Session.QueryOver<ServiceClaim>()
					.Where(x => x.Id == claimId)
					.Fetch(SelectMode.Fetch, x => x.Barcode)
					.Fetch(SelectMode.Fetch, x => x.Barcode.Nomenclature)
					.SingleOrDefault();
				
				if (serviceClaim == null) continue;

				PostomatInfo postomat = postomats.FirstOrDefault(x => x.Id == terminalId);
				Entity.AddItem(serviceClaim, postomat);
			}
		}
		#endregion
		
		#region Save and Print
		public override bool Save() 
		{
			if (!Validate()) 
			{
				return false;
			}

			Entity.User = Entity.User ?? userService.GetCurrentUser();
			UoW.Save(Entity);
			UoW.Commit();
			return true;
		}
		
		public void Print() 
		{
			if(!Entity.Items.Any()) 
			{
				interactive.ShowMessage(ImportanceLevel.Warning, "Нет данных для печати. Заполните документ");
				return;
			}
			
			Save();
			var reportInfo = new ReportInfo {
				Title = $"Ведомость на забор №{Entity.Id} от {Entity.CreateTime:d}",
				Identifier = "Documents.PostomatWithdrawSheet",
				Parameters = new Dictionary<string, object> 
				{
					{ "id",  Entity.Id },
					{ "responsible_person", userService.GetCurrentUser().Name }
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}
		#endregion
	}
}
