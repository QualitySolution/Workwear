using System;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Workwear.ReportParameters.ViewModels;

namespace Workwear.ReportParameters.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChoiceProtectionToolsView : Gtk.Bin 
	{
		private bool destroyed;
		public BindingControler<ChoiceProtectionToolsView> Binding { get; private set; }
		
		public ChoiceProtectionToolsView(){
			this.Build();
			Binding = new BindingControler<ChoiceProtectionToolsView>(this, new Expression<Func<ChoiceProtectionToolsView, object>>[] 
			{
				w => w.Sensitive
			});
		}
		
		private ChoiceProtectionToolsViewModel viewModel;
		public ChoiceProtectionToolsViewModel ViewModel {
			get => viewModel; 
			set {
				viewModel = value;
				CreateTable();
			}
		}

		private void CreateTable(){
			ytreeChoiseProtectionTools.CreateFluentColumnsConfig<SelectedProtectionTools>()
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Name).SearchHighlight()
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Sensitive = x.Highlighted)
				.Finish();
			ytreeChoiseProtectionTools.ItemsDataSource = ViewModel.Items;
			
			yentrySearch.Changed += delegate {
				ytreeChoiseProtectionTools.SearchHighlightText = yentrySearch.Text;
				ViewModel.SelectLike(yentrySearch.Text);
				ytreeChoiseProtectionTools.YTreeModel.EmitModelChanged();
			};
			ycheckbuttonChooseAll.Sensitive = ycheckbuttonUnChooseAll.Sensitive = ViewModel.Items.Any();
			ycheckbuttonChooseAll.Clicked += (s,e) => ViewModel.SelectAll();
			ycheckbuttonUnChooseAll.Clicked += (s,e) => ViewModel.UnSelectAll();
		}

		public override void Destroy() 
		{
			if (destroyed) 
			{
				return;
			}

			Binding.CleanSources();
			ViewModel = null;
			ytreeChoiseProtectionTools.Dispose();
			base.Destroy();
			
			destroyed = true;
		}
	}
}
