using System;
using Gamma.Binding.Converters;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Analytics;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Analytics;

namespace Workwear.Views.Analytics 
{
	public partial class ProtectionToolsCategoryView : EntityDialogViewBase<ProtectionToolsCategoryViewModel, ProtectionToolsCategory> 
	{
		public ProtectionToolsCategoryView(ProtectionToolsCategoryViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() 
		{
			labelId.Binding.AddBinding(Entity, e => e.Id,  w => w.Text, new IdToStringConverter()).InitializeFromSource();
			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			textComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			
			treeItems.ColumnsConfig = FluentColumnsConfig<ProtectionTools>.Create()
				.AddColumn("ИД").AddReadOnlyTextRenderer(p => p.Id.ToString())
				.AddColumn("Название").AddTextRenderer(p => p.Name)
				.AddColumn("Тип номенклатуры").AddTextRenderer(p => p.Type.Name)
				.Finish();
			treeItems.ItemsDataSource = Entity.ProtectionTools;
			treeItems.Selection.Changed += Nomenclature_Selection_Changed;
			treeItems.ButtonReleaseEvent += TreeItems_ButtonReleaseEvent;
		}
		
		private void TreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) 
		{
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = treeItems.GetSelectedObject<ProtectionTools>();
			
			var itemNomenclature = new MenuItem("Открыть номенклатуру нормы");
			itemNomenclature.Sensitive = selected != null;
			itemNomenclature.Activated += (sender, eventArgs) => ViewModel.OpenProtectionTool(selected);
			menu.Add(itemNomenclature);
			
			menu.ShowAll();
			menu.Popup();
		}
		
		void Nomenclature_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveProtectionTools.Sensitive = treeItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnAddProtectionToolsClicked(object sender, EventArgs e) 
		{
			ViewModel.AddProtectionTools();
		}

		protected void OnRemoveProtectionToolsClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveProtectionTools(treeItems.GetSelectedObjects<ProtectionTools>());
		}
	}
}
