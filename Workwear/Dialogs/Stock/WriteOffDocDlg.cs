using System;
using System.Linq;
using Autofac;
using Gamma.Binding.Converters;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Validation;
using workwear.Domain.Company;
using workwear.Domain.Stock;
using Workwear.Measurements;
using workwear.Repository;
using workwear.Repository.Stock;
using workwear.Tools.Features;

namespace workwear
{
	public partial class WriteOffDocDlg : EntityDialogBase<Writeoff>
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private readonly ILifetimeScope autofacScope;

		public WriteOffDocDlg() {
			Build();
			autofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Writeoff> ();
			Entity.Date = DateTime.Today;
			Entity.CreatedbyUser = UserRepository.GetMyUser (UoW);
			ItemsTable.CurWarehouse = 
				new StockRepository()
					.GetDefaultWarehouse(UoW, autofacScope
						.Resolve<FeaturesService>(), Entity.CreatedbyUser.Id);
			ItemsTable.SizeService = autofacScope.Resolve<SizeService>();
			ConfigureDlg ();
		}

		public WriteOffDocDlg (EmployeeCard employee) : this () { 
			ItemsTable.CurWorker = employee;
		}

		public WriteOffDocDlg (Subdivision facility) : this () {
			ItemsTable.CurObject = facility;
		}

		public WriteOffDocDlg (Writeoff item) : this (item.Id) {}

		public WriteOffDocDlg (int id) {
			Build ();
			autofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Writeoff> (id);
			ItemsTable.SizeService = autofacScope.Resolve<SizeService>();
			ConfigureDlg ();
		}

		private void ConfigureDlg() {
			ylabelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
				.InitializeFromSource ();
			ylabelCreatedBy.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
				.InitializeFromSource ();
			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
			ItemsTable.WriteOffDoc = Entity;
		}			

		public override bool Save() {
			Logger.Info ("Запись документа...");
			var valid = new QSValidator<Writeoff> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid((Gtk.Window)Toplevel))
				return false;
			
			Entity.UpdateOperations(UoW);
			if (Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			
			UoWGeneric.Save ();
			if(Entity.Items.Any(w => w.WriteoffFrom == WriteoffFrom.Employee)) {
				Logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
				foreach(var employeeGroup in 
					Entity.Items.Where(w => w.WriteoffFrom == WriteoffFrom.Employee)
						.GroupBy(w => w.EmployeeWriteoffOperation.Employee))
				{
					var employee = employeeGroup.Key;
					foreach(var itemsGroup in 
						employeeGroup.GroupBy(i => i.Nomenclature.Type.Id))
					{
						var wearItem = 
							employee.WorkwearItems.FirstOrDefault(i => i.ProtectionTools.Id == itemsGroup.Key);
						if(wearItem == null) {
							Logger.Debug ("Позиции <{0}> не требуется к выдаче, пропускаем...", 
								itemsGroup.First ().Nomenclature.Type.Name);
							continue;
						}
						wearItem.UpdateNextIssue (UoW);
					}
				}
				UoWGeneric.Commit ();
			}
			Logger.Info ("Ok");
			return true;
		}
		public override void Destroy() {
			base.Destroy();
			autofacScope.Dispose();
		}
	}
}