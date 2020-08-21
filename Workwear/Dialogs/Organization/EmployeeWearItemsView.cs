using System;
using System.Linq;
using Gamma.Utilities;
using NHibernate;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.Utilities;
using workwear.Dialogs.Issuance;
using workwear.Domain.Company;

namespace workwear.Dialogs.Organization
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeWearItemsView : WidgetOnEntityDialogBase<EmployeeCard>, IMustBeDestroyed
	{
		public EmployeeWearItemsView()
		{
			this.Build();

			ytreeWorkwear.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<EmployeeCardItem>()
				.AddColumn("ТОН").AddTextRenderer(node => node.ActiveNormItem.Norm.Title)
				.AddColumn("Наименование").AddTextRenderer(node => node.Item.Name)
				.AddColumn("По норме").AddTextRenderer(node => node.Item.Units.MakeAmountShortStr(node.ActiveNormItem.Amount))
				.AddColumn("Срок службы").AddTextRenderer(node => node.ActiveNormItem.LifeText)
				.AddColumn("Дата получения").AddTextRenderer(node => String.Format("{0:d}", node.LastIssue))
				.AddColumn("Получено").AddTextRenderer(node => node.Item.Units.MakeAmountShortStr(node.Amount))
					.AddSetter((w, node) => w.Foreground = node.AmountColor)
				.AddColumn("След. получение").AddTextRenderer(node => String.Format("{0:d}", node.NextIssue))
				.AddColumn("Просрочка").AddTextRenderer(
					node => node.NextIssue.HasValue && node.NextIssue.Value < DateTime.Today
					? NumberToTextRus.FormatCase((int)(DateTime.Today - node.NextIssue.Value).TotalDays, "{0} день", "{0} дня", "{0} дней")
					: String.Empty)
				.AddColumn("На складе").AddTextRenderer(node => node.InStock != null ? node.Item.Units.MakeAmountShortStr(node.InStock.Sum(x => x.Amount)) : null)
				 .AddSetter((w, node) => w.Foreground = node.InStockState.GetEnumColor())
				.AddColumn("Подходящая номенклатура").AddTextRenderer(node => node.MatchedNomenclatureShortText)
				.AddSetter((w, node) => w.Foreground = node.InStockState.GetEnumColor())
				.Finish();
			ytreeWorkwear.Selection.Changed += ytreeWorkwear_Selection_Changed;

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<EmployeeCardItem>(HandleEntityChangeEvent);
		}

		public bool ItemsLoaded { get; private set; }

		public virtual void LoadItems()
		{
			RootEntity.FillWearInStockInfo(UoW, RootEntity.Subdivision?.Warehouse, DateTime.Now);
			RootEntity.FillWearRecivedInfo(UoW);
			ytreeWorkwear.ItemsDataSource = RootEntity.ObservableWorkwearItems;
			ItemsLoaded = true;
		}

		void HandleEntityChangeEvent(EntityChangeEvent[] changeEvents)
		{
			if(changeEvents.Select(e => e.Entity).OfType<EmployeeCardItem>().Any(x => x.EmployeeCard.IsSame(RootEntity)))
				RefreshWorkItems();
		}

		protected void OnButtonGiveWearByNormClicked(object sender, EventArgs e)
		{
			if(!MyTdiDialog.Save())
				return;

			ExpenseDocDlg winExpense = new ExpenseDocDlg(RootEntity, true);
			OpenNewTab(winExpense);
		}

		protected void RefreshWorkItems()
		{
			if(!NHibernateUtil.IsInitialized(RootEntity.WorkwearItems))
				return;

			foreach(var item in RootEntity.WorkwearItems) {
				UoW.Session.Refresh(item);
			}
			RootEntity.FillWearInStockInfo(UoW, RootEntity.Subdivision?.Warehouse, DateTime.Now);
			RootEntity.FillWearRecivedInfo(UoW);
		}

		void ytreeWorkwear_Selection_Changed(object sender, EventArgs e)
		{
			buttonTimeLine.Sensitive = ytreeWorkwear.Selection.CountSelectedRows() > 0;
		}

		public override void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Destroy();
		}

		protected void OnButtonReturnWearClicked(object sender, EventArgs e)
		{
			IncomeDocDlg winIncome = new IncomeDocDlg(RootEntity);
			OpenNewTab(winIncome);
		}

		protected void OnButtonTimeLineClicked(object sender, EventArgs e)
		{
			var row = ytreeWorkwear.GetSelectedObject<EmployeeCardItem>();
			var dlg = new EmployeeIssueGraphDlg(RootEntity, row.Item);
			OpenNewTab(dlg);
		}

		protected void OnButtonWriteOffWearClicked(object sender, EventArgs e)
		{
			WriteOffDocDlg winWriteOff = new WriteOffDocDlg(RootEntity);
			OpenNewTab(winWriteOff);
		}

		protected void OnButtonRefreshWorkwearItemsClicked(object sender, EventArgs e)
		{
			RootEntity.UpdateWorkwearItems();
		}
	}
}
