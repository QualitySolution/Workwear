using System;
using NLog;
using QS.Views.Dialog;
using QSProjectsLib;
using workwear.Domain.Company;
using workwear.Repository.Company;
using workwear.ViewModels.Company;

namespace workwear.Views.Company
{
	public partial class SubdivisionView : EntityDialogViewBase<SubdivisionViewModel, Subdivision>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public SubdivisionView(SubdivisionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		void ConfigureDlg()
		{
			treeviewProperty.CreateFluentColumnsConfig<SubdivisionRecivedInfo>()
				.AddColumn("Наименование").AddTextRenderer(x => x.NomeclatureName)
				.AddColumn("% износа").AddTextRenderer(x => x.WearPercent.ToString("P0"))
				.AddColumn("Кол-во").AddTextRenderer(x => $"{x.Amount} {x.Units}")
				.AddColumn("Последняя выдача").AddTextRenderer(x => x.LastReceive.ToShortDateString())
				.AddColumn("Расположение").AddTextRenderer(x => x.Place)
				.Finish();

			treeviewProperty.Binding.AddBinding(ViewModel, vm => vm.Items, w => w.ItemsDataSource).InitializeFromSource();

			entryCode.Binding.AddBinding(Entity, e => e.Code, w => w.Text).InitializeFromSource();
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			textviewAddress.Binding.AddBinding(Entity, e => e.Address, w => w.Buffer.Text).InitializeFromSource();

			entitywarehouse.ViewModel = ViewModel.EntryWarehouse;
			entitySubdivision.ViewModel = ViewModel.EntrySubdivisionViewModel;

			lbWarehouse.Visible = entitywarehouse.Visible = ViewModel.VisibleWarehouse;
			buttonPlacement.Sensitive = buttonGive.Sensitive = buttonReturn.Sensitive = buttonWriteOff.Sensitive = !ViewModel.UoW.IsNew;
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
			ViewModel.GiveItem();
		}

		protected void OnButtonReturnClicked(object sender, EventArgs e)
		{
			ViewModel.ReturnItem();
		}

		protected void OnButtonWriteOffClicked(object sender, EventArgs e)
		{
			ViewModel.WriteOffItem();
		}
	}
}
 