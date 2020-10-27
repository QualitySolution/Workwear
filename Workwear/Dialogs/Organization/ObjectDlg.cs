using System;
using System.Linq;
using Autofac;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QSProjectsLib;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.JournalViewModels.Stock;
using workwear.Repository;
using workwear.Repository.Stock;
using workwear.Tools.Features;
using workwear.ViewModels.Stock;

namespace workwear.Dialogs.Organization
{
	public partial class ObjectDlg : EntityDialogBase<Subdivision>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		ILifetimeScope AutofacScope;
		private FeaturesService featuresService { get; set; }

		public ObjectDlg (Subdivision obj) : this(obj.Id) {}

		public ObjectDlg (int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Subdivision>(id);
			this.featuresService = new FeaturesService();

			ConfigureDlg();
			Fill(id);
		}

		public ObjectDlg()
		{
			this.Build();

			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Subdivision>();
			featuresService = new FeaturesService();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			//FIXME Временно чтобы не реализовывать более сложный механизм.
			HasChanges = true;

			treeviewProperty.CreateFluentColumnsConfig<SubdivisionRecivedInfo>()
				.AddColumn("Наименование").AddTextRenderer(x => x.NomeclatureName)
				.AddColumn("% износа").AddTextRenderer(x => x.WearPercent.ToString("P0"))
				.AddColumn("Кол-во").AddTextRenderer(x => $"{x.Amount} {x.Units}")
				.AddColumn("Последняя выдача").AddTextRenderer(x => x.LastReceive.ToShortDateString())
				.AddColumn("Расположение").AddTextRenderer(x => x.Place)
				.Finish();

			if(!UoW.IsNew)
				treeviewProperty.ItemsDataSource = SubdivisionRepository.ItemsBalance(UoW, Entity);

			entryCode.Binding.AddBinding(Entity, e => e.Code, w => w.Text).InitializeFromSource();
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			textviewAddress.Binding.AddBinding(Entity, e => e.Address, w => w.Buffer.Text).InitializeFromSource();

			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			var builder = new LegacyEEVMBuilderFactory<Subdivision>(this, Entity, UoW, MainClass.MainWin.NavigationManager, AutofacScope);

			entitywarehouse.ViewModel = builder.ForProperty(x => x.Warehouse)		
									.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
									.UseViewModelDialog<WarehouseViewModel>()
									.Finish();

			NotifyConfiguration.Instance.BatchSubscribe(SubdivisionOperationChanged)
				.IfEntity<SubdivisionIssueOperation>()
				.AndWhere(x => x.Subdivision.Id == Entity.Id);

			DisableFeatures();
		}

		private void DisableFeatures()
		{
			if(!featuresService.Available(WorkwearFeature.Warehouses)) {
				lb5.Visible = false;
				entitywarehouse.Visible = false;
				entitywarehouse.ViewModel.Entity = new StockRepository().GetDefaultWarehouse(UoW,featuresService);
			}
		}

		void SubdivisionOperationChanged(EntityChangeEvent[] changeEvents)
		{
			treeviewProperty.ItemsDataSource = SubdivisionRepository.ItemsBalance(UoW, Entity);
		}

		private void Fill(int id)
		{
			buttonPlacement.Sensitive = true;
			buttonGive.Sensitive = true;
			buttonReturn.Sensitive = true;
			buttonWriteOff.Sensitive = true;
			TabName = entryName.Text;
		}

		public override bool Save ()
		{
			logger.Info("Запись подразделения...");
			var valid = new QSValidator<Subdivision>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoW.Save();

			logger.Info("Ok");
			return true;
		}

		protected void OnButtonPlacementClicked(object sender, EventArgs e)
		{
			Reference WinPlacement = new Reference(false);
			WinPlacement.ParentFieldName = "object_id";
			WinPlacement.ParentId = Entity.Id;
			WinPlacement.SqlSelect = "SELECT id, name FROM @tablename WHERE object_id = " + Entity.Id.ToString();
			WinPlacement.SetMode(true, false, true, true, true);
			WinPlacement.FillList("object_places", "размещение", "Размещения объекта");
			WinPlacement.Show();
			WinPlacement.Run();
			WinPlacement.Destroy();
		}

		protected void OnButtonGiveClicked(object sender, EventArgs e)
		{
			Subdivision obj = UoW.GetById<Subdivision>(Entity.Id);
			ExpenseDocDlg winExpense = new ExpenseDocDlg(obj);
			OpenNewTab(winExpense);
		}

		protected void OnButtonReturnClicked(object sender, EventArgs e)
		{
			Subdivision obj = UoW.GetById<Subdivision>(Entity.Id);
			IncomeDocDlg winIncome = new IncomeDocDlg(obj);
			OpenNewTab(winIncome);
		}

		protected void OnButtonWriteOffClicked(object sender, EventArgs e)
		{
			Subdivision obj = UoW.GetById<Subdivision>(Entity.Id);
			WriteOffDocDlg winWriteOff = new WriteOffDocDlg(obj);
			OpenNewTab(winWriteOff);
		}

		public override void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Destroy();
			AutofacScope.Dispose();
		}
	}
}
 